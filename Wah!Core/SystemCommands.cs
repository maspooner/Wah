using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Wah_Interface;

namespace Wah_Core {
	/// <summary>
	/// The part of the wah processing that acts as a SYSTEM module with more access to the internals of the unit.
	/// </summary>
	internal partial class WahProcessor : AModule {
		private const string WAH_NO_NAMAE = "wah";
		private const string WAH_NO_HAN = "Ichi alpha 0";

		protected override ICommand[] CreateCommands() {
			return new ICommand[] {
				new PlainCommand("modlist", Cmd_Modlist),
				new PlainCommand("clear", Cmd_Clear),
				new PlainCommand("c", Cmd_Close),
				new PlainCommand("exit", Cmd_Exit),
				new Cmd_Macro_Set("macro.set", this),
				new Cmd_Macro_Remove("macro.remove", this),
				new PlainCommand("macro.list", Cmd_Macro_List),
				//Temp commands
				new PlainCommand("wah!", Cmd_Wah),
				new PlainCommand("wah?", Cmd_WahHuh),
				new PlainCommand("chn1", Cmd_Chain1),
				new PlainCommand("chn2", Cmd_Chain2),
				new PlainCommand("chn3", Cmd_Chain3),
			};
		}

		// Lists all loaded modules
		private NoData Cmd_Modlist(IWah wah, IBundle bun) {
			wah.Putln("Loaded modules: ", Color.Gold);
			foreach(IModule mod in modules) {
				wah.Putln(mod.Name, mod.Color);
			}
			return new NoData();
		}

		// Clears the window
		private NoData Cmd_Clear(IWah wah, IBundle bun) {
			wah.Display.ClearWindow();
			return new NoData();
		}

		// Closes the window
		private NoData Cmd_Close(IWah wah, IBundle bun) {
			wah.Display.HideWindow();
			return new NoData();
		}

		// Exits the Wah program
		private NoData Cmd_Exit(IWah wah, IBundle bun) {
			//finish execute loop
			isDone = true;
			coreWah.Shutdown();
			return new NoData();
		}

		// List all of the macros
		private NoData Cmd_Macro_List(IWah wah, IBundle bun) {
			wah.Putln("Registered Macros: ", Color.LightGreen);
			foreach (string key in macros.Keys) {
				wah.Putln(key + " -> " + macros[key]);
			}
			return new NoData();
		}


		//test commands

		private StringData Cmd_Wah(IWah wah, IBundle bun) {
			return new StringData("Wah!");
		}

		private StringData Cmd_WahHuh(IWah wah, IBundle bun) {
			wah.Putln("Wah?", Color.Yellow);
			return GetCommand("wah!").Run<StringData>(wah, bun);
		}


		private IData Cmd_Chain1(IWah wah, IBundle bun) {
			string s = wah.Api.Call<StringData>("chn2").String;
			wah.Putln("s: " + s);
			return new NoData();
		}

		private IData Cmd_Chain2(IWah wah, IBundle bun) {
			int b = wah.Api.Call<IntData>("chn3 --hey").Int;
			return new StringData(b.ToString());
		}

		private IntData Cmd_Chain3(IWah wah, IBundle bun) {
			if (bun.HasFlag("hey")) {
				return new IntData(5);
			}
			else {
				throw new Exception("FATAL ERROR");
				//return new BoolReturn(true);
			}
		}
	}

	/// <summary>
	/// Models a command that sets a macro
	/// </summary>
	internal class Cmd_Macro_Set : CheckedCommand<NoData> {
		private WahProcessor pro;
		internal Cmd_Macro_Set(string name, WahProcessor pro) : base(name,
			//Rules
			new RequireRule('f'),
			new RequireRule('t'),
			new TypeRule<StringData>('f'),
			new TypeRule<StringData>('t')) {
			this.pro = pro;
		}
		public override NoData Apply(IWah wah, IBundle bun) {
			string from = bun.Argument<StringData>('f').String;
			string to = bun.Argument<StringData>('t').String;
			pro.SetMacro(from, to);
			return new NoData();
		}
	}

	/// <summary>
	/// Models a command that removes a macro
	/// </summary>
	internal class Cmd_Macro_Remove : CheckedCommand<NoData> {
		private WahProcessor pro;
		internal Cmd_Macro_Remove(string name, WahProcessor pro) : base(name,
			//Rules
			new RequireRule('k'),
			new TypeRule<StringData>('k')) {
			this.pro = pro;
		}

		public override NoData Apply(IWah wah, IBundle bun) {
			string key = bun.Argument<StringData>('k').String;
			pro.RemoveMacro(key);
			return new NoData();
		}
	}

}
