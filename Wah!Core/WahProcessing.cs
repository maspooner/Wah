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
	internal partial class WahProcessing : OldAModule, IProcessor, IApi {
		private const string SYSTEM_MODULE_NAME = "SYSTEM";
		private const string SYSTEM_MODULE_VERSION = "Alpha nya 0";
		private const string MACRO_ID = ";;";

		private IDictionary<string, string> macros;
		private OldIData previousReturn;

		public WahProcessing() : base(SYSTEM_MODULE_NAME, SYSTEM_MODULE_VERSION) {
			macros = new Dictionary<string, string>();
			previousReturn = null;
		}

		public void InitializeMacros() {
			macros.Add("home", "/f/");
		}

		/// <summary>
		/// Calls the specified module with the given command and arguments
		/// </summary>
		/// <returns>The return value of the call</returns>
		private OldIData Call(OldAModule mod, string cmd, string args) {
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

		private string MashCommand(string cmd, string args) {
			return "\"" + (args.Length == 0 ? cmd : cmd + " " + args) + "\"";
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

		public bool ModuleLoaded(string module) {
			throw new NotImplementedException();
		}

		public OldIData Call(string line) {
			throw new NotImplementedException();
		}

		public string AwaitRead() {
			throw new NotImplementedException();
		}
	}
}
