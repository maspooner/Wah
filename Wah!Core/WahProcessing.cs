using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Drawing;
using Wah_Interface;

namespace Wah_Core {
	internal partial class WahProcessing : AModule, IProcessor, IApi {
		private const string SYSTEM_MODULE_NAME = "SYSTEM";
		private const string SYSTEM_MODULE_VERSION = "Alpha nya 0";
		private const string MACRO_ID = ";;";

		private Program wah;
		private OutputVisitor outVisit;
		private bool isDone;
		private object objLock;
		private string primedCmd;
		private Thread workerThread;
		private IList<AModule> modules;
		private IDictionary<string, string> macros;
		private IData previousReturn;

		public WahProcessing(Program wah) : base(SYSTEM_MODULE_NAME, SYSTEM_MODULE_VERSION) {
			isDone = false;
			objLock = new object();
			primedCmd = "";
			this.wah = wah;
			workerThread = new Thread(RunCommandLoop);
			modules = new List<AModule>();
			macros = new Dictionary<string, string>();
			outVisit = new OutputVisitor(wah);
			previousReturn = null;
		}

		public void InitializeModules() {
			//include the system settings
			wah.ReSettings.IncludeSettings(wah.Disk, this);
			LoadModule("Wah!Commands", "Fuko");
		}
		public void InitializeMacros() {
			macros.Add("home", "/f/");
		}

		public void LoadModule(string name) {
			throw new NotImplementedException();
		}

		public void LoadModule(string dllName, string moduleName) {
			Assembly dll = wah.ReDisk.LoadAssembly(dllName);
			Type moduleType = dll.GetTypes().First(t => t.Name.Equals(moduleName));
			AModule newModule = (AModule)Activator.CreateInstance(moduleType);
			ValidateModule(newModule);
			modules.Add(newModule);
			//set all settings from the settings file
			wah.ReSettings.IncludeSettings(wah.Disk, newModule);
		}

		public void ValidateModule(AModule mod) {
			if (modules.Any(m => m.Name.Equals(mod.Name))) {
				throw new IOLoadException(mod.Name + " module is already loaded");
			}
		}

		public void LoadModuleLibrary(string dllName) {
			throw new NotImplementedException();
		}

		public void UnloadModule(string name) {
			if (modules.Any(m => m.Name.Equals(name))) {
				AModule mod = modules.First(m => m.Name.Equals(name));
				wah.ReSettings.ExcludeSettings(mod);
				modules.Remove(mod);
			}
			else {
				throw new IOLoadException(name + " module is not loaded");
			}
		}

		public void BeginListening() {
			workerThread.Start();
		}

		public void RunCommandLoop() {
			while (!isDone) {
				wah.Put("[custom-name]", Color.Yellow);
				wah.Put("@", Color.YellowGreen);
				wah.Put("[computer-name] ", Color.Cyan);
				wah.Putln(" Wah!~", Color.Magenta);
				lock (objLock) {
					Monitor.Wait(objLock);
				}
				if (!isDone) {
					Execute();
				}
			}
		}

		public void Prepare(string line) {
			primedCmd = line;
			lock (objLock) {
				Monitor.Pulse(objLock);
			}
		}
		/// <summary>
		/// Calls the specified module with the given command and arguments
		/// </summary>
		/// <returns>The return value of the call</returns>
		private IData Call(AModule mod, string cmd, string args) {
			try {
				//call the function in the context of the given module
				previousReturn = mod.Handle(wah, cmd, args, previousReturn);
				//TODO test
				return previousReturn;
			}
			catch (AWahException waex) {
				//throw back up the call chain
				throw new CallFailedException("Error in call to module: " + mod.Name + " with call to " + MashCommand(cmd, args), waex);
			}
			catch (Exception ex) {
				//throw back up the call chain
				throw new UnhandledException("Unhandled Exception thrown in: " + mod.Name
					+ " with call to " + MashCommand(cmd, args) + " 大変です", ex);
			}
		}
		/// <summary>
		/// Executes the primed command, expanding macros beforehand
		/// </summary>
		public void Execute() {
			try {
				//expand the macros on the line
				primedCmd = ExpandMacros(primedCmd);
				wah.Putln("> " + primedCmd);
				//call and output to the console
				Call(primedCmd).Accept(outVisit);
			}
			//top level exception handling
			catch (AWahException waex) {
				waex.OutputError(wah);
			}
			catch (Exception ex) {
				wah.PutErr("Fatal Wah! Error");
				wah.PutErr(ex.StackTrace);
			}
		}

		private string MashCommand(string cmd, string args) {
			return "\"" + (args.Length == 0 ? cmd : cmd + " " + args) + "\"";
		}

		public void InterruptJob() {
			wah.PutErr("死んちゃったｗ");
			workerThread.Abort();
			workerThread = new Thread(RunCommandLoop);
			workerThread.Start();
		}

		public AModule FindModule(string name) {
			if (ModuleLoaded(name)) {
				return modules.First(a => a.Name.Equals(name));
			}
			else {
				throw new NoSuchItemException("no module named " + name);
			}
		}

		private string ParseFirst(string line) {
			line = line.Trim();
			int space = line.IndexOf(' ');
			return space < 0 ? line : line.Substring(0, space).Trim();
		}
		private string ParseRest(string line, bool allowEmpty) {
			line = line.Trim();
			int space = line.IndexOf(' ');
			if (space < 0) {
				if (allowEmpty) {
					return "";
				}
				else {
					throw new IllformedInputException("Expected space in line " + line);
				}
			}
			else {
				return line.Substring(space + 1).Trim();
			}
		}
		private string ExpandMacros(string line) {
			if (line.Contains(MACRO_ID)) {
				int iMac = line.IndexOf(MACRO_ID);
				//stuff before the macro
				string lineBefore = line.Substring(0, iMac);
				//stuff after the macro
				string lineRest = line.Substring(iMac + MACRO_ID.Length);
				foreach (string key in macros.Keys) {
					if (lineRest.StartsWith(key)) {
						return ExpandMacros(lineBefore + macros[key] + lineRest.Substring(key.Length));
					}
				}
				throw new IllformedInputException("Could not find definition of a macro starting with " + MACRO_ID);
			}
			else {
				return line;
			}
		}


		////////////////////////////////////////////////////////////////////////////////
		////  IApi methods
		////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Is the module with the given name loaded?
		/// </summary>
		public bool ModuleLoaded(string module) {
			return modules.Any(a => a.Name.Equals(module));
		}
		/// <summary>
		/// Calls the command specified by the line
		/// Does not look at macros
		/// </summary>
		public IData Call(string line) {
			//first should be either a module or system command
			string firstString = ParseFirst(line);
			//module
			if (ModuleLoaded(firstString)) {
				//find the module with that name
				AModule module = FindModule(firstString);
				//the rest of the line must be non-empty
				string notModule = ParseRest(line, false);
				//module command should follow
				string cmdName = ParseFirst(notModule);
				//call the module with the command and arguments (which may be empty)
				return Call(module, cmdName, ParseRest(notModule, true));
			}
			//system command
			else {
				//call this module with the command and optional arguments
				return Call(this, firstString, ParseRest(line, true));
			}
		}
		/// <summary>
		/// Executes the command specified by the given line
		/// </summary>
		public void Execute(string line) {
			Call(line);
		}

		/// <summary>
		/// Blocks the current thread, waiting for user input to return to the command
		/// </summary>
		public string AwaitRead() {
			lock (objLock) {
				Monitor.Wait(objLock);
			}
			return primedCmd;
		}
	}
}
