using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Wah_Interface;

namespace Wah_Core {
	internal partial class WahProcessing {
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
			cmds.Add("clr", Cmd_Clear);
			cmds.Add("macro", Cmd_Macro);

			cmds.Add("chn1", Cmd_Chain1);
			cmds.Add("chn2", Cmd_Chain2);
			cmds.Add("chn3", Cmd_Chain3);
			return cmds;
		}
		public override void SetDefaultSettings(IReSettings sets) {
			sets.RegisterSetting(this, "web.browser", "firefox", SettingType.STRING);
		}
		/************************************************
		***  Commands
		*************************************************/
		private IReturn Cmd_Wah(ICore wah, IList<string> args, IDictionary<string, string> flags) {
			return new StringReturn("Wah!");
		}

		private IReturn Cmd_WahHuh(ICore wah, IList<string> args, IDictionary<string, string> flags) {
			wah.Putln("Wah?", Color.Yellow);
			string w = wah.Api.Call("wah!").AsString();
			wah.Putln(w);
			wah.Api.Execute("wah!");
			return new StringReturn(w);
		}

		private IReturn Cmd_Cmdlist(ICore wah, IList<string> args, IDictionary<string, string> flags) {
			if (args.Count == 0) {
				return Cmdlist_PrintModule(wah, this);
			}
			else if (args.Count == 1) {
				return Cmdlist_PrintModule(wah, FindModule(args[0]));
			}
			else {
				throw new IllformedInputException("Wrong number of arguments");
			}

		}

		private ListReturn Cmdlist_PrintModule(ICore wah, AModule mod) {
			wah.Putln("Showing commands for module " + mod.Name, Color.Aqua);
			foreach (string cmd in mod.Commands.Select(pair => pair.Key)) {
				wah.Putln(cmd, Color.Yellow);
			}
			IList<string> cmds = mod.Commands.Select(pair => pair.Key).ToList();
			IList<IReturn> rets = new List<IReturn>();
			foreach (string cmd in cmds) {
				rets.Add(new StringReturn(cmd));
			}
			return new ListReturn(rets);
		}

		private IReturn Cmd_Modlist(ICore wah, IList<string> args, IDictionary<string, string> flags) {
			if (args.Count == 0) {
				//show loaded and unloaded modules
				if (flags.ContainsKey("-a")) {
					if (!flags.ContainsKey("-u"))
						flags.Add("-u", "");
					if (!flags.ContainsKey("-l"))
						flags.Add("-l", "");
				}
				//show unloaded modules
				if (flags.ContainsKey("-u")) {
					wah.Putln("Unloaded modules: ", Color.Chartreuse);
					//call: modlist -u
					//for each found assembly
					foreach (Assembly a in this.wah.ReDisk.LoadAllAssemblies()) {
						//for each AModule type in the assembly
						foreach (Type t in a.GetTypes().Where(q => q.BaseType == typeof(AModule))) {
							AModule newModule = (AModule)Activator.CreateInstance(t);
							//only print unloaded modules
							if (!ModuleLoaded(newModule.Name)) {
								wah.Putln(newModule.Name, Color.LightGray);
							}
						}
					}
				}
				if (flags.Count == 0 || flags.ContainsKey("-l")) {
					//show loaded modules
					//call: modlist OR modlist -l
					wah.Putln("Loaded modules: ", Color.Gold);
					foreach (AModule m in modules) {
						wah.Putln(m.Name, Color.Yellow);
					}
				}
			}
			else {
				throw new IllformedInputException("Wrong number of arguments");
			}
			return new NoReturn();
		}

		private IReturn Cmd_Call(ICore wah, IList<string> args, IDictionary<string, string> flags) {
			IReturn call = wah.Api.Call(string.Join(" ", args));
			wah.Putln("Call Results:", Color.Cyan);
			wah.Putln(call.AsString());
			return call;
		}

		private IReturn Cmd_Help(ICore wah, IList<string> args, IDictionary<string, string> flags) {
			if (args.Count == 0) {
				//call: help
				Help_Module(this);
			}
			else if (args.Count == 1) {
				//call: help fuko
				if (ModuleLoaded(args[0])) {
					Help_Module(FindModule(args[0]));
				}
				//call: help cmdlist
				else if (Commands.ContainsKey(args[0])) {
					Help_Command(this, args[0]);
				}
				else {
					throw new NoSuchItemException("no module/system command named " + args[0]);
				}
			}
			else if (args.Count == 2) {
				//call: help fuko hitode
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
			wah.ReDisk.LoadDisplayHelp(wah, mod, mod.Name);
		}

		private void Help_Command(AModule mod, string cmd) {
			wah.Putln("Command " + cmd + " in module " + mod.Name, Color.GreenYellow);
			wah.ReDisk.LoadDisplayHelp(wah, mod, cmd);
		}

		private IReturn Cmd_About(ICore wah, IList<string> args, IDictionary<string, string> flags) {
			if (args.Count == 0) {

			}
			throw new NotImplementedException();
		}

		private IReturn Cmd_Close(ICore wah, IList<string> args, IDictionary<string, string> flags) {
			if (args.Count == 0) {
				wah.Display.HideWindow();
			}
			else {
				throw new IllformedInputException("No arguments onegai");
			}
			return new NoReturn();
		}

		private IReturn Cmd_Shutdown(ICore wah, IList<string> args, IDictionary<string, string> flags) {
			Environment.Exit(0);
			return new NoReturn();
		}

		private IReturn Cmd_Clear(ICore wah, IList<string> args, IDictionary<string, string> flags) {
			wah.Display.ClearWindow();
			return new NoReturn();
		}

		private IReturn Cmd_Macro(ICore wah, IList<string> args, IDictionary<string, string> flags) {
			if (args.Count >= 1) {
				if (args[0].Equals("add")) {
					//call: marco add home my house
					return Macro_Add(args);
				}
				else if (args[0].Equals("edit")) {
					//call: marco edit home this is my home
					return Macro_Edit(wah, args);

				}
				else if (args[0].Equals("delete")) {
					//call: marco delete home
					return Macro_Delete(args);
				}
				else if (args[0].Equals("list")) {
					//call: macro list
					if (args.Count == 1) {
						wah.Putln("Registered Macros: ", Color.LightGreen);
						foreach (string key in macros.Keys) {
							wah.Putln(key + " -> " + macros[key]);
						}
						return new NoReturn();
					}
					else {
						throw new IllformedInputException("subcommand list takes no arguments, silly!");
					}
				}
				else {
					throw new IllformedInputException(args[0] + " is not a recognized subcommand of marco");
				}
			}
			else {
				throw new IllformedInputException("Wrong number of arguments");
			}
		}

		private IReturn Macro_Add(IList<string> args) {
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
			return new NoReturn();
		}

		private IReturn Macro_Edit(ICore wah, IList<string> args) {
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
			return new NoReturn();
		}

		private IReturn Macro_Delete(IList<string> args) {
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

		private IReturn Cmd_Chain1(ICore wah, IList<string> args, IDictionary<string, string> flags) {
			int i = wah.Api.Call("chn2").AsInt();
			wah.Putln("i: " + i);
			return new NoReturn();
		}

		private IReturn Cmd_Chain2(ICore wah, IList<string> args, IDictionary<string, string> flags) {
			bool b = wah.Api.Call("chn3 true").AsBool();
			return new IntReturn(b ? 5 : 6);
		}

		private IReturn Cmd_Chain3(ICore wah, IList<string> args, IDictionary<string, string> flags) {
			if (args.Count == 0) {
				return new NoReturn();
			}
			else {
				throw new Exception("FATAL ERROR");
				//return new BoolReturn(true);
			}
		}
	}
}
