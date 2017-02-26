using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Wah_Interface;

namespace Wah_Core {
	internal class WahProcessing : AModule, IProcessor, IApi {
		private const string SYSTEM_MODULE_NAME = "SYSTEM";
		private ICore wah;
		private bool isDone;
		private object objLock;
		private string primedCmd;
		private Thread workerThread;
		private IList<AModule> modules;
		public AModule ActiveModule { get; set; }

		public WahProcessing(ICore wah) : base(SYSTEM_MODULE_NAME) {
			isDone = false;
			objLock = new object();
			primedCmd = "";
			this.wah = wah;
			workerThread = new Thread(RunCommandLoop);
			ActiveModule = this;
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
			if (modules.Any(m => m.Name.Equals(newModule.Name))) {
				throw new ModuleLoadException(moduleName + " module is already loaded");
			}
			modules.Add(newModule);
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
			//save the module before and switch the active module to the given module
			AModule parentModule = ActiveModule;
			ActiveModule = mod;
			try {
				//call the function in the context of the given module
				IReturn callResult = mod.Call(wah, cmd, args);
				//switch the module back to the parent module
				ActiveModule = parentModule;
				//return the call
				return callResult;
			}
			catch (AWahException waex) {
				//switch the module back to the parent module
				ActiveModule = parentModule;
				//throw back up the call chain
				throw new CallFailedException("Error in call to module: " + mod.Name + " with call to " + MashCommand(cmd, args), waex);
			}
			catch (Exception ex) {
				//switch the module back to the parent module
				ActiveModule = parentModule;
				//throw back up the call chain
				throw new UnhandledException("Unhandled Exception thrown in: " + mod.Name
					+ " with call to " + MashCommand(cmd, args) + " 大変です", ex);
			}
		}

		/// <summary>
		/// Executes the given command in the given module with the given arguments
		/// </summary>
		private void Execute(AModule mod, string cmd, string args) {
			//save the module before and switch the active module to the given module
			AModule parentModule = ActiveModule;
			ActiveModule = mod;
			try {
				//execute the command in the module
				mod.Execute(wah, cmd, args);
				//switch the module back to the parent module
				ActiveModule = parentModule;
			}
			catch (AWahException waex) {
				//switch the module back to the parent module
				ActiveModule = parentModule;
				//throw back up the call chain
				throw new CallFailedException("Error in executing module: " + mod.Name
					+ " with call to " + MashCommand(cmd, args), waex);
			}
			catch (Exception ex) {
				//switch the module back to the parent module
				ActiveModule = parentModule;
				//throw back up the call chain
				throw new UnhandledException("Unhandled Exception thrown in: " + mod.Name
					+ " with call to " + MashCommand(cmd, args) + " 大変です", ex);
			}
		}
		/// <summary>
		/// Executes the primed command
		/// </summary>
		public void Execute() {
			wah.Log("execute: " + primedCmd);
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
				wah.Log("Wah! Error");
				wah.Log(ue.GetMessages());
			}
			catch (AWahException waex) {
				wah.Log("Wah! Error");
				wah.Log(waex.GetMessages());
			}
			catch (Exception ex) {
				wah.Log("Wah! Error");
				wah.Log(ex.StackTrace);
			}
			ActiveModule = this;
		}

		private string MashCommand(string cmd, string args) {
			return "\"" + (args.Length == 0 ? cmd : cmd + " " + args) + "\"";
		}

		public void InterruptJob() {
			wah.Log("死んちゃったｗ");
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
			cmds.Add("help", Cmd_Help);
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
		private IReturn Cmd_Wah(ICore wah, string[] args) {
			return new StringReturn("Wah!");
		}

		private IReturn Cmd_WahHuh(ICore wah, string[] args) {
			wah.Log("Wah?");
			string w = wah.Api.Call("wah!").AsString();
			wah.Log(w);
			wah.Api.Execute("wah!");
			return new StringReturn(w);
		}

		private IReturn Cmd_Cmdlist(ICore wah, string[] args) {
			if(args.Length == 0) {
				foreach(string cmd in this.Commands.Select(pair => pair.Key)) {
					wah.Log(cmd);
				}
			}
			else if(args.Length == 1) {
				string module = args[0];
				foreach (string cmd in FindModule(module).Commands.Select(pair => pair.Key)) {
					wah.Log(cmd);
				}
			}
			else {
				throw new IllformedInputException("Wrong number of arguments");
			}
			return new NoReturn();
		}

		private IReturn Cmd_Help(ICore wah, string[] args) {
			throw new NotImplementedException();
		}

		private IReturn Cmd_Chain1(ICore wah, string[] args) {
			int i = wah.Api.Call("chn2").AsInt();
			wah.Log("i: " + i);
			return new NoReturn();
		}

		private IReturn Cmd_Chain2(ICore wah, string[] args) {
			bool b = wah.Api.Call("chn3 true").AsBool();
			return new IntReturn(b ? 5 : 6);
		}

		private IReturn Cmd_Chain3(ICore wah, string[] args) {
			if(args.Length == 0) {
				return new NoReturn();
			}
			else {
				throw new Exception("FATAL ERROR");
				return new BoolReturn(true);
			}
		}


	}
}
