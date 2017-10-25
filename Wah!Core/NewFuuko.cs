using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Wah_Interface;

namespace Wah_Core {
	/// <summary>
	/// The part of the wah processing that acts as a SYSTEM module with more access to the internals of the unit.
	/// </summary>
	internal partial class NewFuukoProcessor : AModule {
		private const string FUUKO_NO_NAMAE = "wah";
		private const string FUUKO_NO_HAN = "Hitode alpha 0 or Nya alpha 0";

		protected override NewICommand[] CreateCommands() {
			return new NewICommand[] {
				new UncheckedCommand("clear", Cmd_Clear),
				new UncheckedCommand("c", Cmd_Close),
				new UncheckedCommand("exit", Cmd_Exit),
				//Temp commands
				new UncheckedCommand("chn1", Cmd_Chain1),
				new UncheckedCommand("chn1", Cmd_Chain2),
				new UncheckedCommand("chn1", Cmd_Chain3),
			};
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


		private IData Cmd_Chain1(IWah wah, IBundle bun) {
			string s = wah.Api.Call<StringData>("chn2").Data;
			wah.Putln("s: " + s);
			return new NoData();
		}

		private IData Cmd_Chain2(IWah wah, IBundle bun) {
			int b = wah.Api.Call<IntData>("chn3 --hey").Data;
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
}
