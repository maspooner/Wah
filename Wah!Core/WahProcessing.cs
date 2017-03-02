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
	internal class WahProcessing : AModule, IProcessor, IApi {
		private const string SYSTEM_MODULE_NAME = "SYSTEM";
		private const string SYSTEM_MODULE_VERSION = "Alpha nya 0";
		private ICore wah;
		private bool isDone;
		private object objLock;
		private string primedCmd;
		private Thread workerThread;
		private IList<AModule> modules;

		public WahProcessing(ICore wah) : base(SYSTEM_MODULE_NAME, SYSTEM_MODULE_VERSION) {
			isDone = false;
			objLock = new object();
			primedCmd = "";
			this.wah = wah;
			workerThread = new Thread(RunCommandLoop);
			modules = new List<AModule>();
		}

		public void InitializeModules() {
			LoadModule("Wah!Commands", "Fuko");
		}

		public void LoadModule(string name) {
			throw new NotImplementedException();
		}

		public void LoadModule(string dllName, string moduleName) {
			Assembly dll = wah.Disk.LoadAssembly(dllName);
			Type moduleType = dll.GetTypes().First(t => t.Name.Equals(moduleName));
			AModule newModule = (AModule)Activator.CreateInstance(moduleType);
			ValidateModule(newModule);
			modules.Add(newModule);
		}

		public void ValidateModule(AModule mod) {
			if (modules.Any(m => m.Name.Equals(mod.Name))) {
				throw new ModuleLoadException(mod.Name + " module is already loaded");
			}
		}

		public void LoadModuleLibrary(string dllName) {
			throw new NotImplementedException();
		}

		public void UnloadModule(string name) {
			if (modules.Any(m => m.Name.Equals(name))) {
				modules.Where(m => !m.Name.Equals(name));
			}
			else {
				throw new ModuleLoadException(name + " module is not loaded");
			}
		}

		public void BeginListening() {
			workerThread.Start();
		}

		public void RunCommandLoop() {
			while (!isDone) {
				wah.Putln("\n[custom-name]@[computer-name]  Wah!~", Color.Magenta);
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
		private IReturn Call(AModule mod, string cmd, string args) {
			try {
				//call the function in the context of the given module
				IReturn callResult = mod.Call(wah, cmd, args);
				//return the call
				return callResult;
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
		/// Executes the given command in the given module with the given arguments
		/// </summary>
		private void Execute(AModule mod, string cmd, string args) {
			try {
				//execute the command in the module
				mod.Execute(wah, cmd, args);
			}
			catch (AWahException waex) {
				//throw back up the call chain
				throw new CallFailedException("Error in executing module: " + mod.Name
					+ " with call to " + MashCommand(cmd, args), waex);
			}
			catch (Exception ex) {
				//throw back up the call chain
				throw new UnhandledException("Unhandled Exception thrown in: " + mod.Name
					+ " with call to " + MashCommand(cmd, args) + " 大変です", ex);
			}
		}
		/// <summary>
		/// Executes the primed command
		/// </summary>
		public void Execute() {
			wah.Putln("> " + primedCmd);
			try {
				//the first part should be a module or system command
				string firstString = ParseFirst(primedCmd);
				//module name
				if (ModuleLoaded(firstString)) {
					//find the module with that name
					AModule module = FindModule(firstString);
					// the rest of the line should be non-empty
					string notModule = ParseRest(primedCmd, false);
					//find the command name
					string cmdName = ParseFirst(notModule);
					//execute the command in the module with the possibly empty arguments
					Execute(module, cmdName, ParseRest(notModule, true));
				}
				//system command
				else {
					//execute on this module the command with the possibly empty arguments
					Execute(this, firstString, ParseRest(primedCmd, true));
				}
			}
			//top level exception handling
			catch(UnhandledException ue) {
				wah.PutErr("Wah! Error");
				wah.PutErr(ue.GetInnerMessages());
			}
			catch (AWahException waex) {
				wah.PutErr("Wah! Error");
				wah.PutErr(waex.GetInnerMessages());
			}
			catch (Exception ex) {
				wah.PutErr("Wah! Error");
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
		/// </summary>
		public IReturn Call(string line) {
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
			primedCmd = line;
			Execute();
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

		////////////////////////////////////////////////////////////////////////////////
		////  AModule methods
		////////////////////////////////////////////////////////////////////////////////
		public override Dictionary<string, CommandDelegate> InitializeCommands() {
			Dictionary<string, CommandDelegate> cmds = new Dictionary<string, CommandDelegate>();
			cmds.Add("wah!", Cmd_Wah);
			cmds.Add("wah?", Cmd_WahHuh);
			cmds.Add("cmdlist", Cmd_Cmdlist);
			cmds.Add("modlist", Cmd_Modlist);
			cmds.Add("help", Cmd_Help);
			cmds.Add("call", Cmd_Call);
			cmds.Add("c", Cmd_Close);
			cmds.Add("shutdown", Cmd_Shutdown);

			cmds.Add("chn1", Cmd_Chain1);
			cmds.Add("chn2", Cmd_Chain2);
			cmds.Add("chn3", Cmd_Chain3);
			return cmds;
		}
		public override void InitializeSettings(ISettings sets) {
			throw new NotImplementedException();
		}
		/************************************************
		***  Commands
		*************************************************/
		private IReturn Cmd_Wah(ICore wah, List<string> args, Dictionary<string, string> flags) {
			return new StringReturn("Wah!");
		}

		private IReturn Cmd_WahHuh(ICore wah, List<string> args, Dictionary<string, string> flags) {
			wah.Putln("Wah?", Color.Yellow);
			string w = wah.Api.Call("wah!").AsString();
			wah.Putln(w);
			wah.Api.Execute("wah!");
			return new StringReturn(w);
		}

		private IReturn Cmd_Cmdlist(ICore wah, List<string> args, Dictionary<string, string> flags) {
			if(args.Count == 0) {
				return Cmdlist_PrintModule(wah, this);
			}
			else if(args.Count == 1) {
				return Cmdlist_PrintModule(wah, FindModule(args[0]));
			}
			else {
				throw new IllformedInputException("Wrong number of arguments");
			}
			
		}

		private ListReturn Cmdlist_PrintModule(ICore wah, AModule mod) {
			wah.Putln("Showing commands for module " + mod.Name);
			foreach (string cmd in mod.Commands.Select(pair => pair.Key)) {
				wah.Putln(cmd, Color.Yellow);
			}
			return new ListReturn(mod.Commands.Select(pair => pair.Key).ToList());
		}

		private IReturn Cmd_Modlist(ICore wah, List<string> args, Dictionary<string, string> flags) {
			if (args.Count == 0) {
				//call: modlist
				foreach (AModule m in modules) {
					wah.Putln(m.Name, Color.Yellow);
				}
			}
			if(args.Count == 1) {
				if (args[0].StartsWith("-")) {

				}
			}
			else {
				throw new IllformedInputException("Wrong number of arguments");
			}
			return new NoReturn();
		}

		private IReturn Cmd_Call(ICore wah, List<string> args, Dictionary<string, string> flags) {
			IReturn call = wah.Api.Call(string.Join(" ", args));
			wah.Putln("Call Results:", Color.Cyan);
            wah.Putln(call.AsString());
			return call;
		}

		private IReturn Cmd_Help(ICore wah, List<string> args, Dictionary<string, string> flags) {
			if(args.Count == 0) {
				Help_Module(this);
			}
			else if(args.Count == 1) {
				if (ModuleLoaded(args[0])) {
					Help_Module(FindModule(args[0]));
				}
				else if (Commands.ContainsKey(args[0])) {
					Help_Command(this, args[0]);
				}
				else {
					throw new NoSuchItemException("no module/system command named " + args[0]);
                }
			}
			else if (args.Count == 2) {
				if (ModuleLoaded(args[0])) {
					AModule mod = FindModule(args[0]);
					if (mod.Commands.ContainsKey(args[1])) {
						Help_Command(mod, args[1]);
					}
					else {
						throw new NoSuchItemException("no command named " + args[1]);
					}
				}
				else {
					throw new NoSuchItemException("no module named " + args[0]);
				}
			}
			else {
				throw new IllformedInputException("wrong number of arguments");
			}
			return new NoReturn();
		}

		private void Help_Module(AModule mod) {
			wah.Putln("Module " + mod.Name, Color.GreenYellow);
			wah.Putln("Version " + mod.Version);
			wah.Putln("=============================");
			wah.Disk.LoadDisplayHelp(wah, mod, mod.Name);
		}

		private void Help_Command(AModule mod, string cmd) {
			wah.Putln("Command " + cmd + " in module " + mod.Name, Color.GreenYellow);
			wah.Disk.LoadDisplayHelp(wah, mod, cmd);
		}

		private IReturn Cmd_About(ICore wah, List<string> args, Dictionary<string, string> flags) {
			if (args.Count == 0) {

			}
			throw new NotImplementedException();
		}

		private IReturn Cmd_Close(ICore wah, List<string> args, Dictionary<string, string> flags) {
			if(args.Count == 0) {
				wah.Display.HideWindow();
			}
			else {
				throw new IllformedInputException("No arguments onegai");
			}
			return new NoReturn();
		}

		private IReturn Cmd_Shutdown(ICore wah, List<string> args, Dictionary<string, string> flags) {
			Environment.Exit(0);
			return new NoReturn();
		}

		//private IReturn Cmd_Config(ICore wah, string[] args) {
		//	if(args.Length <= 1) {
		//		throw new IllformedInputException("Wrong number of arguments");
		//	}
		//	else if (args.Length == 2) {
		//		// config set fuko.partyhat true
		//		// config get fuko.partyhat
		//		// config get .globalparty true

		//	}
		//	else {
		//		throw new IllformedInputException("Wrong number of arguments");
		//	}
		//	return new NoReturn();
		//}

		private IReturn Cmd_Chain1(ICore wah, List<string> args, Dictionary<string, string> flags) {
			int i = wah.Api.Call("chn2").AsInt();
			wah.Putln("i: " + i);
			return new NoReturn();
		}

		private IReturn Cmd_Chain2(ICore wah, List<string> args, Dictionary<string, string> flags) {
			bool b = wah.Api.Call("chn3 true").AsBool();
			return new IntReturn(b ? 5 : 6);
		}

		private IReturn Cmd_Chain3(ICore wah, List<string> args, Dictionary<string, string> flags) {
			if(args.Count == 0) {
				return new NoReturn();
			}
			else {
				throw new Exception("FATAL ERROR");
				//return new BoolReturn(true);
			}
		}


	}
}
