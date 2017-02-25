using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wah_Interface;

namespace Wah_Commands {
	public class Fuko : AModule {
		private const string FUKO_NO_NAMAE = "fuko";
		public Fuko() : base(FUKO_NO_NAMAE) {

		}

		public override Dictionary<string, CommandDelegate> InitializeCommands() {
			Dictionary<string, CommandDelegate> cmds = new Dictionary<string, CommandDelegate>();
			cmds.Add("namae", Cmd_Namae);
			cmds.Add("hitode", Cmd_HitodeKangaeru);
			return cmds;
		}

		public override void InitializeSettings(ISettings sets) {
			throw new NotImplementedException();
		}

		/************************************************
		***  Commands
		*************************************************/
		private IReturn Cmd_Namae(ICore wah, string args) {
			wah.Log("FUKO MODULE DESU");
			return new StringReturn("Fuko desu yo~!");
		}

		private IReturn Cmd_HitodeKangaeru(ICore wah, string args) {
			while (true) ;
		}

	}
}
