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
		public override void InitializeCommands(Dictionary<string, CommandDelegate> cmds) {
			cmds.Add("wah!", Cmd_Wah);
			cmds.Add("wah?", Cmd_WahHuh);
			cmds.Add("cmdlist", Cmd_Cmdlist);
			cmds.Add("modlist", Cmd_Modlist);
			cmds.Add("help", Cmd_Help);
			cmds.Add("c", Cmd_Close);
			cmds.Add("shutdown", Cmd_Shutdown);
			cmds.Add("clr", Cmd_Clear);
			cmds.Add("macro", Cmd_Macro);
			cmds.Add("echo", Cmd_Echo);

			cmds.Add("chn1", Cmd_Chain1);
			cmds.Add("chn2", Cmd_Chain2);
			cmds.Add("chn3", Cmd_Chain3);
		}
		public override void SetDefaultSettings(IReSettings sets) {
			sets.RegisterSetting(this, "web.browser", "firefox", SettingType.STRING);
		}
		/************************************************
		***  Commands
		*************************************************/
		private IData Cmd_Wah(ICore wah, CommandBundle bun) {
			bun.AssertNoArgs();
			return new StringData("Wah!");
		}

		private IData Cmd_WahHuh(ICore wah, CommandBundle bun) {
			bun.AssertNoArgs();
			wah.Putln("Wah?", Color.Yellow);
			return wah.Api.Call("wah!");
		}

		private IData Cmd_Cmdlist(ICore wah, CommandBundle bun) {
			switch (bun.Arguments.Count) {
				case 0: return Cmdlist_PrintModule(wah, this);
				case 1: return Cmdlist_PrintModule(wah, FindModule(bun.Arguments[0].AsString()));
				default: throw new WrongNumberArgumentsException();
			}
		}

		private ListData Cmdlist_PrintModule(ICore wah, AModule mod) {
			wah.Putln("Showing commands for module " + mod.Name, Color.Aqua);
			IList<IData> cmds = mod.Commands.Select(pair => new StringData(pair.Key) as IData).ToList();
			return new ListData(cmds);
		}

		private IData Cmd_Modlist(ICore wah, CommandBundle bun) {
			IList<IData> rets = new List<IData>();
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
				rets.Add(new StringData("Unloaded modules: ", Color.Chartreuse));
				//call: modlist -u
				//for each found assembly
				foreach (Assembly a in this.wah.ReDisk.LoadAllAssemblies()) {
					//for each AModule type in the assembly
					foreach (Type t in a.GetTypes().Where(q => q.BaseType == typeof(AModule))) {
						AModule newModule = (AModule)Activator.CreateInstance(t);
						//only print unloaded modules
						if (!ModuleLoaded(newModule.Name)) {
							rets.Add(new StringData(newModule.Name, Color.LightGray));
						}
					}
				}
			}
			if (bun.FlagCount(0) || bun.HasFlag("-l")) {
				//show loaded modules
				//call: modlist OR modlist -l
				rets.Add(new StringData("Loaded modules: ", Color.Gold));
				foreach (AModule m in modules) {
					rets.Add(new StringData(m.Name));
				}
			}
			return new ListData(rets);
		}

		private IData Cmd_Help(ICore wah, CommandBundle bun) {
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
					AModule mod = FindModule(strArgs[0]);
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
			return new NoData();
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

		private IData Cmd_About(ICore wah, CommandBundle bun) {
			bun.AssertNoArgs();
			throw new NotImplementedException();
		}

		private IData Cmd_Close(ICore wah, CommandBundle bun) {
			bun.AssertNoArgs();
			wah.Display.HideWindow();
			return new NoData();
		}

		private IData Cmd_Shutdown(ICore wah, CommandBundle bun) {
			bun.AssertNoArgs();
			Environment.Exit(0);
			return new NoData();
		}

		private IData Cmd_Clear(ICore wah, CommandBundle bun) {
			bun.AssertNoArgs();
			wah.Display.ClearWindow();
			return new NoData();
		}

		private IData Cmd_Macro(ICore wah, CommandBundle bun) {
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
						return new NoData();
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

		private IData Macro_Add(IList<string> args) {
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
			return new NoData();
		}

		private IData Macro_Edit(ICore wah, IList<string> args) {
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
			return new NoData();
		}

		private IData Macro_Delete(IList<string> args) {
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

		private IData Cmd_Echo(ICore wah, CommandBundle bun) {
			string echo = string.Join(" ", bun.StringArgs());
			return new StringData(echo, Color.LightPink);
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

		private IData Cmd_Chain1(ICore wah, CommandBundle bun) {
			int i = wah.Api.Call("chn2").AsInt();
			wah.Putln("i: " + i);
			return new NoData();
		}

		private IData Cmd_Chain2(ICore wah, CommandBundle bun) {
			bool b = wah.Api.Call("chn3 true").AsBool();
			return new IntData(b ? 5 : 6);
		}

		private IData Cmd_Chain3(ICore wah, CommandBundle bun) {
			if (bun.ArgCount(0)) {
				return new NoData();
			}
			else {
				throw new Exception("FATAL ERROR");
				//return new BoolReturn(true);
			}
		}
	}
}
