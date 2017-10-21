using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Wah_Interface;

namespace Wah_Core {
	/// <summary>
	/// The part of the wah processing that handles all procesing and parsing of user input
	/// </summary>
	internal partial class NewFuukoProcessor : NewIProcessor, NewIParser, NewIApi {
		private const char MODULE_DELIM = '.'; //Separates modules from commands

		private char[] moduleDelimCache; // prevents creating array each time string[].Split is needed
		private ISet<NewIModule> modules;
		private NewOutputVisitor outputVisit;
		private object objLock;
		private string primedCmd;
		private Thread workerThread;
		private bool isDone;
		private NewWahMain coreWah;
		private NewIData lastOutput;

		internal NewFuukoProcessor(NewWahMain coreWah) : base(FUUKO_NO_NAMAE, FUUKO_NO_HAN) {
			modules = new HashSet<NewIModule>();
			modules.Add(this);
			moduleDelimCache = new char[] { MODULE_DELIM };
			objLock = new object();
			primedCmd = "";
			this.coreWah = coreWah;
			isDone = false;
			lastOutput = new NewNoData();
			outputVisit = new NewOutputVisitor(coreWah);

			workerThread = new Thread(RunCommandLoop);
		}

		/// <summary>
		/// Finds a module with the given name in the list of loaded modules
		/// </summary>
		/// <param name="modName">the name of the module to find</param>
		/// <returns>the found module</returns>
		private NewIModule FindModule(string modName) {
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
				coreWah.PutErr(wue.Message);
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

		public NewIData Call(string line) {
			return Call<NewIData>(line);
		}

		public D Call<D>(string line) where D : NewIData {
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

		public NewIData ParseData(string line) {
			return new NewNoData();
		}

		////////////////////////////////////////////////////////
		// IParser methods
		////////////////////////////////////////////////////////
		public NewICommand ParseCommand(string cmd) {
			cmd = cmd.Trim();
			if(cmd.Contains(MODULE_DELIM)) {
				//must be within a specific module
				string[] pieces = cmd.Split(moduleDelimCache, StringSplitOptions.RemoveEmptyEntries);
				if(pieces.Length == 2) {
					string mod = pieces[0];
					cmd = pieces[1];
					if(ModuleLoaded(mod)) {
						NewIModule module = FindModule(mod);
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
					NewIModule module = modules.First(mod => mod.HasCommand(cmd));
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

		public NewIBundle ParseBundle(string bun) {
			ISet<string> flags = new HashSet<string>();
			IDictionary<char, NewIData> arguments = new Dictionary<char, NewIData>();
			return new NewCommandBundle(flags, arguments);
		}

		
	}

	/// <summary>
	/// Models a visitor for IData that can display any type of IData.
	/// </summary>
	public class NewOutputVisitor : NewIDataVisitor<NewIData> {
		private NewIWah wah;

		public NewOutputVisitor(NewIWah wah) {
			this.wah = wah;
		}

		public NewIData Visit(NewIData data) {
			return data.Accept(this);
		}

		public NewIData VisitImage(NewImageData data) {
			wah.Putln(data.ToString(), Color.Aquamarine);
			wah.Display.ShowVisual(new SimpleImage(data.Data), 1);
			return data;
		}

		public NewIData VisitInt(NewIntData data) {
			wah.Putln(data.ToString(), Color.OrangeRed);
			return data;
		}

		public NewIData VisitNone(NewNoData data) {
			return data;
		}

		public NewIData VisitString(NewStringData data) {
			wah.Putln(data.Data, data.Color);
			return data;
		}
	}
}
