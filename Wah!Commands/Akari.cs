using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wah_Interface;
using System.Drawing;

namespace Wah_Commands {
	/// <summary>
	/// Akari handles ecchi
	/// </summary>
	public class Akari : AModule {
		private const string AKARI_NO_NAMAE = "akari";
		private const string AKARI_NO_HAN = "Pomf alpha 0";
		
		public Akari() : base(AKARI_NO_NAMAE, Color.Crimson, AKARI_NO_HAN) { }

		protected override ICommand[] CreateCommands() {
			return new ICommand[] {
				//new SimpleCommand(ListServices));
				//Commands.Add(new AkariDownloadCmd()
			};
		}

		//Define in AModule, ISettings InitSettings() {
		//settings.add("akari.proxy", "none")
		//}

		private IData Cmd_(IWah wah, IBundle bun) {
			return new NoData();
		}

	}
}
