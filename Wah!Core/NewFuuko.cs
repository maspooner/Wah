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
	internal partial class NewFuukoProcessor : NewAModule {
		private const string FUUKO_NO_NAMAE = "fuuko or wah";
		private const string FUUKO_NO_HAN = "Hitode alpha 0 or Nya alpha 0";

		protected override NewICommand[] CreateCommands() {
			return new NewICommand[] {
				new UncheckedCommand("exit", Cmd_Exit)
			};
		}

		private NewNoData Cmd_Exit(NewIWah wah, NewIBundle bun) {
			//finish execute loop
			isDone = true;
			coreWah.Shutdown();
			return new NewNoData();
		}
	}
}
