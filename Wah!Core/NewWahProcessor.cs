using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Wah_Interface;

namespace Wah_Core {
	/// <summary>
	/// The part of the wah processing that handles all procesing and parsing of user input
	/// </summary>
	internal partial class NewWahProcessor : NewIProcessor, IParser, NewIApi {
		private const char MODULE_DELIM = '.'; //Separates modules from commands
		private const string PREVIOUS_OUTPUT_MARKER = "<>"; //Marks the user wanting to use the output of the last command

		private char[] moduleDelimCache; // prevents creating array each time string[].Split is needed
		private ISet<IModule> modules;
		private OutputVisitor outputVisit;
		private object objLock;
		private string primedCmd;
		private Thread workerThread;
		private bool isDone;
		private WahMain coreWah;
		private IData lastOutput;
		private string programDir;

		internal NewWahProcessor(WahMain coreWah) : base(WAH_NO_NAMAE, Color.LightGray, WAH_NO_HAN) {
			modules = new HashSet<IModule>();
			//add the system module (this)
			modules.Add(this);
			moduleDelimCache = new char[] { MODULE_DELIM };
			objLock = new object();
			primedCmd = "";
			this.coreWah = coreWah;
			isDone = false;
			lastOutput = new NoData();
			outputVisit = new OutputVisitor(coreWah);

			workerThread = new Thread(RunCommandLoop);
			LoadModule("Wah!Commands", "fuuko");
		}

		/// <summary>
		/// Finds a module with the given name in the list of loaded modules
		/// </summary>
		/// <param name="modName">the name of the module to find</param>
		/// <returns>the found module</returns>
		private IModule FindModule(string modName) {
			return modules.First(mod => mod.Name.Equals(modName));
		}

		/// <summary>
		/// Runs the main command loop REPL for accepting commands
		/// </summary>
		private void RunCommandLoop() {
			while (!isDone) {
				coreWah.Put("[custom-name]", Color.Yellow);
				coreWah.Put("@", Color.YellowGreen);
				coreWah.Put("[computer-name] ", Color.Cyan);
				coreWah.Putln(" Wah!~", Color.Magenta);
				//wait for input
				lock (objLock) {
					Monitor.Wait(objLock);
				}
				if (!isDone) {
					Execute();
				}
			}
		}

		/// <summary>
		/// Runs the latest string command that the user input.
		/// </summary>
		private void Execute() {
			try {
				coreWah.Putln("> " + primedCmd);
				//call and save as the last output
				lastOutput = Call(primedCmd);
				//show the result on screen
				lastOutput.Accept(outputVisit);
			}
			//top level exception handling
			catch(WahUserException wue) {
				//user fault
				wue.DisplayOn(coreWah);
			}
			catch(NewAWahException awe) {
				//coder fault
				coreWah.PutErr("A module returned an error:");
				awe.DisplayOn(coreWah);
			}
			catch (Exception ex) {
				coreWah.PutErr("Fatal Wah! Error");
				coreWah.PutErr(ex.Message);
				coreWah.PutErr(ex.StackTrace);
			}
		}

		////////////////////////////////////////////////////////
		// IProcessor methods
		////////////////////////////////////////////////////////
		public void BeginListening() {
			workerThread.Start();
		}
		public void Prepare(string line) {
			primedCmd = line;
			lock (objLock) {
				Monitor.Pulse(objLock);
			}
		}

		public void InterruptJob() {
			coreWah.PutErr("死んじゃったｗ");
			coreWah.Display.ClearVisuals();
			workerThread.Abort();
			workerThread = new Thread(RunCommandLoop);
			workerThread.Start();
		}

		public void LoadModule(string dllName, string moduleName) {
			if(!ModuleLoaded(moduleName)) {
				Assembly dll = Assembly.LoadFile(Path.Combine(Disk.ProgramDirectory, dllName + ".dll"));
				//grab the type that matches the module name
				Type moduleType = dll.GetTypes().First(t =>
					t.Name.Equals(moduleName, StringComparison.InvariantCultureIgnoreCase));
				IModule newModule = (IModule)Activator.CreateInstance(moduleType);
				modules.Add(newModule);
			}
		}

		public void LoadModuleLibrary(string dllName) {
			Assembly dll = Assembly.LoadFile(Path.Combine(Disk.ProgramDirectory, dllName + ".dll"));
			foreach(Type t in dll.GetTypes()) {
				if(t is IModule) {
					if(!ModuleLoaded(t.Name)) {
						IModule newModule = (IModule)Activator.CreateInstance(t);
					}
				}
			}
		}


		public void UnloadModule(string modName) {
			if (ModuleLoaded(modName)) {
				modules.Remove(FindModule(modName));
			}
		}

		////////////////////////////////////////////////////////
		// IApi methods
		////////////////////////////////////////////////////////
		public bool ModuleLoaded(string modName) {
			return modules.Any(mod => mod.Name.Equals(modName));
		}

		public string AwaitRead() {
			lock (objLock) {
				Monitor.Wait(objLock);
			}
			return primedCmd;
		}

		public IData Call(string line) {
			return Call<IData>(line);
		}

		public D Call<D>(string line) where D : IData {
			line = line.Trim();
			//split into command and bundle
			int iFirstSpace = line.IndexOf(' ');
			string sCmd = iFirstSpace < 0 ? line : line.Substring(0, iFirstSpace).Trim();
			//no spaces? bundle string is nothing
			string sBun = iFirstSpace < 0 ? "" :
				line.Substring(iFirstSpace + 1).Trim(); // should never have first space be at end -> never OB

			//run the command on the bundle
			return ParseCommand(sCmd).Run<D>(coreWah, ParseBundle(sBun));
		}

		public IData ParseData(string line) {
			line = line.Trim();
			//use previous return value
			if(line.Equals(PREVIOUS_OUTPUT_MARKER)) {
				return lastOutput;
			}
			int iParse = 0;
			//int
			if(int.TryParse(line, out iParse)) {
				return new IntData(iParse);
			}
			//TODO parse list, etc.
			return new StringData(line);
		}

		////////////////////////////////////////////////////////
		// IParser methods
		////////////////////////////////////////////////////////
		public ICommand ParseCommand(string cmd) {
			cmd = cmd.Trim();
			if(cmd.Contains(MODULE_DELIM)) {
				//must be within a specific module
				string[] pieces = cmd.Split(moduleDelimCache, StringSplitOptions.RemoveEmptyEntries);
				if(pieces.Length == 2) {
					string mod = pieces[0];
					cmd = pieces[1];
					if(ModuleLoaded(mod)) {
						IModule module = FindModule(mod);
						if(module.HasCommand(cmd)) {
							return module.GetCommand(cmd);
						}
						else {
							//user error: command does not exist
							throw new WahNoCommandException(cmd);
						}
					}
					else {
						//user error: module not loaded or does not exist
						throw new WahNoModuleException(mod);
					}
				}
				else {
					//user error: too many dots in module name
					throw new WahBadFormatException(pieces[0]);
				}
			}
			else {
				//can be in any module
				//how many times does the command appear accross all modules?
				int shareName = modules.Count(mod => mod.HasCommand(cmd));
				if(shareName == 0) {
					//no such command, user error
					throw new WahNoCommandException(cmd);
				}
				else if(shareName == 1) {
					//know exactly which module to run
					IModule module = modules.First(mod => mod.HasCommand(cmd));
					if(module.HasCommand(cmd)) {
						return module.GetCommand(cmd);
					}
					else {
						//user error: command does not exist
						throw new WahNoCommandException(cmd);
					}
				}
				else {
					//too many, don't know which one the user wants to run
					throw new WahAmbiguousCommandException();
				}
			}
		}

		public IBundle ParseBundle(string bun) {
			ISet<string> flags = new HashSet<string>();
			IDictionary<char, IData> arguments = new Dictionary<char, IData>();
			return new CommandBundle(flags, arguments);
		}

		
	}

	/// <summary>
	/// Models a visitor for IData that can display any type of IData.
	/// </summary>
	public class OutputVisitor : IDataVisitor<IData> {
		private IWah wah;

		public OutputVisitor(IWah wah) {
			this.wah = wah;
		}

		public IData Visit(IData data) {
			return data.Accept(this);
		}

		public IData VisitImage(ImageData data) {
			wah.Putln(data.ToString(), Color.Aquamarine);
			wah.Display.ShowVisual(new SimpleImage(data.Data), 1);
			return data;
		}

		public IData VisitInt(IntData data) {
			wah.Putln(data.ToString(), Color.OrangeRed);
			return data;
		}

		public IData VisitNone(NoData data) {
			return data;
		}

		public IData VisitString(StringData data) {
			wah.Putln(data.Data, data.Color);
			return data;
		}

		public IData VisitList(ListData data) {
			foreach(IData d in data.Data) {
				Visit(d);
			}
			return data;
		}
	}
}
