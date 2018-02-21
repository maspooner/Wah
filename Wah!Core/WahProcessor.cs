using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Wah_Interface;

namespace Wah_Core {
	/// <summary>
	/// The part of the wah processing that handles all procesing and parsing of user input
	/// </summary>
	internal partial class WahProcessor : IProcessor, IApi {
		private ISet<IModule> modules;
		private IDictionary<string, string> macros;
		private OutputVisitor outputVisit;
		private object objLock;
		private string primedCmd;
		private Thread workerThread;
		private bool isDone;
		private WahMain coreWah;
		private IData lastOutput;

		internal WahProcessor(WahMain coreWah) : base(WAH_NO_NAMAE, Color.LightGray, WAH_NO_HAN) {
			modules = new HashSet<IModule>();
			//add the system module (this)
			modules.Add(this);
			macros = new Dictionary<string, string>();
			objLock = new object();
			primedCmd = "";
			this.coreWah = coreWah;
			isDone = false;
			lastOutput = new NoData();
			outputVisit = new OutputVisitor(coreWah);

			workerThread = new Thread(RunCommandLoop);
			//temp
			//LoadModule("Wah!Commands", "fuuko");
			macros.Add("home", "/f/");
			//settings.user.name = Matt (default is machine user name)
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
				coreWah.Put(Environment.MachineName, Color.Cyan);
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
				//call and save as the last output, expanding macros in the process
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
		//Internal methods
		////////////////////////////////////////////////////////
		internal void SetMacro(string from, string to) {
			if(macros.ContainsKey(from)) {
				macros[from] = to;
			}
			else {
				macros.Add(from, to);
			}
		}

		internal void RemoveMacro(string key) {
			if (macros.ContainsKey(key)) {
				macros.Remove(key);
			}
			else {
				throw new WahMissingInfoException(key + " macro does not exist");
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
			Tuple<ICommand, IBundle> pair = Parse(line);
			ICommand cmd = pair.Item1;
			IBundle bun = pair.Item2;
			//Verify the command
			if (cmd.Validate(bun)) {
				//run the command on the bundle
				return cmd.Run<D>(coreWah, bun);
			}
			else {
				//user error with missing parameters
				throw new WahCommandParseException(cmd.LastError());
			}
		}

		public IData ParseData(string line) {
			line = line.Trim();
			//use previous return value
			if(line.Equals("^")) {
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
			wah.Display.ShowVisual(new SimpleImage(data.Image), 1);
			return data;
		}

		public IData VisitInt(IntData data) {
			return VisitString(data);
		}

		public IData VisitNone(NoData data) {
			return data;
		}

		public IData VisitString(StringData data) {
			wah.Putln(data.String, data.Color);
			return data;
		}

		public IData VisitList(ListData data) {
			foreach(IData d in data.List) {
				Visit(d);
			}
			return data;
		}
	}
}
