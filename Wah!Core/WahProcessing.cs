using System;
using System.Collections.Generic;
using System.Linq;
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

		public string AwaitRead() {
			lock (objLock) {
				Monitor.Wait(objLock);
			}
			return primedCmd;
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

		public void LoadModules() {
			throw new NotImplementedException();
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
		private string Call(AModule mod, string cmd, string args) {
			//save the module before and switch the active module to the given module
			AModule parentModule = ActiveModule;
			ActiveModule = mod;
			try {
				//call the function in the context of the given module
				string callResult = mod.Call(wah, cmd, args);
				//switch the module back to the parent module
				ActiveModule = parentModule;
				//return the call
				return callResult;
			}
			catch {
				//switch back to the parent
				ActiveModule = parentModule;
				//throw up the call chain
				throw;
			}
		}
		/// <summary>
		/// Calls the command specified by the line
		/// </summary>
		public string Call(string line) {
			try {
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
			catch {
				//continue throwing up the call chain
				throw;
			}
		}
		/// <summary>
		/// Executes the given command in the given module with the given arguments
		/// </summary>
		private void Execute(AModule mod, string cmd, string args) {
			//save the module before and switch the active module to the given module
			AModule beforeModule = ActiveModule;
			ActiveModule = mod;
			try {
				//execute the command in the module
				mod.Execute(wah, cmd, args);
				//switch the module back to the parent module
				ActiveModule = beforeModule;
			}
			catch {
				//switch the module back to the parent module
				ActiveModule = beforeModule;
				//throw back up the call chain
				throw;
			}
		}
		/// <summary>
		/// Executes the primed command
		/// </summary>
		public void Execute() {
			wah.Log(primedCmd);
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
			catch(Exception e) {
				wah.Log(e.Message);
			}
			ActiveModule = this;
		}

		public void InterruptJob() {
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
			if(space < 0) {
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

		public bool ModuleLoaded(string module) {
			return modules.Any(a => a.Name.Equals(module));
		}

		////////////////////////////////////////////////////////////////////////////////
		///  AModule methods
		////////////////////////////////////////////////////////////////////////////////
		public override Dictionary<string, CommandDelegate> InitializeCommands() {
			Dictionary<string, CommandDelegate> cmds = new Dictionary<string, CommandDelegate>();
			cmds.Add("wah!", Cmd_Wah);
			cmds.Add("wah?", Cmd_WahHuh);
			return cmds;
		}
		public override void InitializeSettings(ISettings sets) {
			throw new NotImplementedException();
		}

		private IReturn Cmd_Wah(ICore wah, string args) {
			wah.Log("Wah!");
			return new StringReturn("Wah!");
		}

		private IReturn Cmd_WahHuh(ICore wah, string args) {
			wah.Log("Wah?");
			string w = wah.Api.Call("wah!");
			wah.Log(w);
			return new StringReturn(w);
		}

		
	}
}
