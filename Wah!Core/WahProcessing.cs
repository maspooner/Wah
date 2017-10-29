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
	internal class WahProcessing {
		private const string MACRO_ID = ";;";

		private IDictionary<string, string> macros;

		public WahProcessing() {
			macros = new Dictionary<string, string>();
		}

		public void InitializeMacros() {
			macros.Add("home", "/f/");
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

		////////////////////////////////////////////////////////////////////////////////
		////  AModule methods
		////////////////////////////////////////////////////////////////////////////////
		private void InitializeCommands(Dictionary<string, string> cmds) {
			cmds.Add("macro", Cmd_Macro);
		}
		public void SetDefaultSettings(ISettings sets) {
			sets.RegisterSetting(this, "web.browser", "firefox");
		}
		/************************************************
		***  Commands
		*************************************************/
		private IData Cmd_Macro(IWah wah, IBundle bun) {
			IList<string> strArgs = bun.StringArgs();
			if (bun.Arguments.Count >= 1) {
				if (strArgs[0].Equals("add")) {
					//call: marco add home my house
					return Macro_Add(strArgs);
				}
				else if (strArgs[0].Equals("edit")) {
					//call: marco edit home this is my home
					return Macro_Edit(wah, strArgs);

				}
				else if (strArgs[0].Equals("delete")) {
					//call: marco delete home
					return Macro_Delete(strArgs);
				}
				else if (strArgs[0].Equals("list")) {
					//call: macro list
					if (bun.ArgCount(1)) {
						wah.Putln("Registered Macros: ", Color.LightGreen);
						foreach (string key in macros.Keys) {
							wah.Putln(key + " -> " + macros[key]);
						}
						return new OldNoData();
					}
					else {
						throw new IllformedInputException("subcommand list takes no arguments, silly!");
					}
				}
				else {
					throw new IllformedInputException(strArgs[0] + " is not a recognized subcommand of marco");
				}
			}
			else {
				throw new IllformedInputException("Wrong number of arguments");
			}
		}

		private OldIData Macro_Add(IList<string> args) {
			//need add + from + to
			if (args.Count >= 3) {
				string from = args[1];
				//already registered
				if (macros.ContainsKey(from)) {
					throw new InvalidStateException("macro " + from + " already has a definition.");
				}
				else {
					//remove add command
					args.RemoveAt(0);
					//remove from macro to get the to part
					args.RemoveAt(0);
					//join the rest of the line to get the to part
					string to = string.Join(" ", args);
					//add the macro
					macros.Add(from, to);
				}
			}
			else {
				throw new IllformedInputException("add subcommand must take at least 3 arguments");
			}
			return new OldNoData();
		}

		private OldIData Macro_Edit(IWah wah, IList<string> args) {
			//need edit + from + to
			if (args.Count >= 3) {
				string from = args[1];
				//remove "edit"
				args.RemoveAt(0);
				//remove from portion
				args.RemoveAt(0);
				//rest of line is to
				string to = string.Join(" ", args);
				//not already registered
				if (!macros.ContainsKey(from)) {
					//register real quick
					wah.Api.Call("macro add " + from + " " + to);
				}
				//set the macro to the rest of the line
				macros[from] = to;
			}
			else {
				throw new IllformedInputException("edit subcommand must take at least 3 arguments");
			}
			return new OldNoData();
		}

		private OldIData Macro_Delete(IList<string> args) {
			//need delete + from
			if (args.Count == 2) {
				string from = args[1];
				//already registered
				if (macros.ContainsKey(from)) {
					macros.Remove(from);
				}
				else {
					throw new InvalidStateException(from + " macro does not exist");
				}
			}
			else {
				throw new IllformedInputException("delete subcommand must take 2 arguments");
			}
			return new NoData();
		}

	}
}
