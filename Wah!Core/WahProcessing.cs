using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Wah_Interface;

namespace Wah_Core {
	internal class WahProcessing : IProcessor, IApi {
		private ICore wah;
		private bool isDone;
		private object objLock;
		private string primedCmd;
		private Thread workerThread;
		private IList<Macro> macros;
		private IList<AModule> modules;
		
		public string Module { get; set; }

		public WahProcessing(ICore wah) {
			isDone = false;
			objLock = new object();
			primedCmd = "";
			this.wah = wah;
			workerThread = new Thread(RunCommandLoop);
			Module = "";
			macros = new List<Macro>();
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

		public string Call(string line) {
			//find module, send command and return result
			//save the current module before calling another
			string beforeModule = Module;
			try {
				//try to find the first argument in the line and parse it to find
				// the module associated with that name
				AModule module = FindModule(ParseFirst(line));
				//change the module name to this new module
				Module = module.Name;
				//call the module using the rest of the line
				string callResult = module.Call(wah, ParseRest(line));
				//reset the module
				Module = beforeModule;
				//return the call to the other module
				return callResult;
			}
			catch {
				//in case of error, reset module
				Module = beforeModule;
				//continue throwing up the call chain
				throw;
			}
		}

		public void Execute() {
			wah.Log(primedCmd);
			try {
				AModule module = FindModule(ParseFirst(primedCmd));
				Module = module.Name;
				module.Execute(wah, ParseRest(primedCmd));
				Module = "";
			}
			catch(Exception e) {
				wah.Log(e.Message);
			}
		}

		public void InterruptJob() {
			workerThread.Abort();
			workerThread = new Thread(RunCommandLoop);
			workerThread.Start();
		}

		public AModule FindModule(string name) {
			Func<AModule, bool> matchesName = a => a.Name.Equals(name);
			if (modules.Any(matchesName)) {
				return modules.First(matchesName);
			}
			else {
				throw new NoSuchItemException("no module named " + name);
            }
		}

		private string ParseFirst(string line) {
			int space = line.IndexOf(' ');
			return space < 0 ? line : line.Substring(0, space).Trim();
		}
		private string ParseRest(string line) {
			int space = line.IndexOf(' ');
			if(space < 0) {
				throw new IllformedInputException("Expected space in line " + line);
			}
			else {
				return line.Substring(space + 1).Trim();
			}
		}
	}
}
