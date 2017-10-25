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
		private OldICore wah;
		////////////////////////////////////////////////////////////////////////////////
		////  AModule methods
		////////////////////////////////////////////////////////////////////////////////
		public override void InitializeCommands(Dictionary<string, CommandDelegate> cmds) {
			cmds.Add("wah!", Cmd_Wah);
			cmds.Add("wah?", Cmd_WahHuh);
			cmds.Add("cmdlist", Cmd_Cmdlist);
			cmds.Add("modlist", Cmd_Modlist);
			cmds.Add("help", Cmd_Help);
			cmds.Add("macro", Cmd_Macro);
			cmds.Add("echo", Cmd_Echo);
		}
		public override void SetDefaultSettings(ISettings sets) {
			sets.RegisterSetting(this, "web.browser", "firefox", SettingType.STRING);
		}
		/************************************************
		***  Commands
		*************************************************/
		private OldIData Cmd_Wah(OldICore wah, OldCommandBundle bun) {
			bun.AssertNoArgs();
			return new OldStringData("Wah!");
		}

		private OldIData Cmd_WahHuh(OldICore wah, OldCommandBundle bun) {
			bun.AssertNoArgs();
			wah.Putln("Wah?", Color.Yellow);
			return wah.Api.Call("wah!");
		}

		private OldIData Cmd_Cmdlist(OldICore wah, OldCommandBundle bun) {
			switch (bun.Arguments.Count) {
				case 0: return Cmdlist_PrintModule(wah, this);
				case 1: return Cmdlist_PrintModule(wah, FindModule(bun.Arguments[0].AsString()));
				default: throw new WrongNumberArgumentsException();
			}
		}

		private OldListData Cmdlist_PrintModule(OldICore wah, OldAModule mod) {
			wah.Putln("Showing commands for module " + mod.Name, Color.Aqua);
			IList<OldIData> cmds = mod.Commands.Select(pair => new OldStringData(pair.Key) as OldIData).ToList();
			return new OldListData(cmds);
		}

		private OldIData Cmd_Modlist(OldICore wah, OldCommandBundle bun) {
			IList<OldIData> rets = new List<OldIData>();
			bun.AssertNoArgs();
			//show loaded and unloaded modules
			if (bun.HasFlag("-a")) {
				if (!bun.HasFlag("-u"))
					bun.AddFlag("-u", "");
				if (!bun.HasFlag("-l"))
					bun.AddFlag("-l", "");
			}
			//show unloaded modules
			if (bun.HasFlag("-u")) {
				rets.Add(new OldStringData("Unloaded modules: ", Color.Chartreuse));
				//call: modlist -u
				//for each found assembly
				foreach (Assembly a in this.wah.Disk.LoadAllAssemblies()) {
					//for each AModule type in the assembly
					foreach (Type t in a.GetTypes().Where(q => q.BaseType == typeof(OldAModule))) {
						OldAModule newModule = (OldAModule)Activator.CreateInstance(t);
						//only print unloaded modules
						if (!ModuleLoaded(newModule.Name)) {
							rets.Add(new OldStringData(newModule.Name, Color.LightGray));
						}
					}
				}
			}
			if (bun.FlagCount(0) || bun.HasFlag("-l")) {
				//show loaded modules
				//call: modlist OR modlist -l
				rets.Add(new OldStringData("Loaded modules: ", Color.Gold));
				foreach (OldAModule m in new List<OldAModule>()) {
					rets.Add(new OldStringData(m.Name));
				}
			}
			return new OldListData(rets);
		}

		private OldAModule FindModule(string s) {
			throw new NotImplementedException();
		}

		private OldIData Cmd_Help(OldICore wah, OldCommandBundle bun) {
			IList<string> strArgs = bun.StringArgs();
			if (bun.ArgCount(0)) {
				//call: help
				Help_Module(this);
			}
			else if (bun.ArgCount(1)) {
				//call: help fuko
				if (ModuleLoaded(strArgs[0])) {
					Help_Module(FindModule(strArgs[0]));
				}
				//call: help cmdlist
				else if (Commands.ContainsKey(strArgs[0])) {
					Help_Command(this, strArgs[0]);
				}
				else {
					throw new NoSuchItemException("no module/system command named " + strArgs[0]);
				}
			}
			else if (bun.ArgCount(2)) {
				//call: help fuko hitode
				if (ModuleLoaded(strArgs[0])) {
					OldAModule mod = FindModule(strArgs[0]);
					if (mod.Commands.ContainsKey(strArgs[1])) {
						Help_Command(mod, strArgs[1]);
					}
					else {
						throw new NoSuchItemException("no command named " + strArgs[1]);
					}
				}
				else {
					throw new NoSuchItemException("no module named " + strArgs[0]);
				}
			}
			else {
				throw new WrongNumberArgumentsException();
			}
			return new OldNoData();
		}

		private void Help_Module(OldAModule mod) {
			wah.Putln("Module " + mod.Name, Color.GreenYellow);
			wah.Putln("Version " + mod.Version);
			wah.Putln("=============================");
			wah.Disk.LoadDisplayHelp(wah, mod, mod.Name);
		}

		private void Help_Command(OldAModule mod, string cmd) {
			wah.Putln("Command " + cmd + " in module " + mod.Name, Color.GreenYellow);
			wah.Disk.LoadDisplayHelp(wah, mod, cmd);
		}


		private OldIData Cmd_Macro(OldICore wah, OldCommandBundle bun) {
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

		private OldIData Macro_Edit(OldICore wah, IList<string> args) {
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
			return new OldNoData();
		}

		private OldIData Cmd_Echo(OldICore wah, OldCommandBundle bun) {
			string echo = string.Join(" ", bun.StringArgs());
			return new OldStringData(echo, Color.LightPink);
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

		
	}
}
