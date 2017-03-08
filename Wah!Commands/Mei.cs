using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wah_Interface;

namespace Wah_Commands {
	/// <summary>
	/// Mei handles utility commands
	/// </summary>
	public class Mei : AModule {
		private const string MEI_NO_NAMAE = "mei";
		private const string MEI_NO_HAN = "Onii alpha 0";
		public Mei() : base(MEI_NO_NAMAE, MEI_NO_HAN) {

		}

		public override Dictionary<string, CommandDelegate> InitializeCommands() {
			Dictionary<string, CommandDelegate> cmds = new Dictionary<string, CommandDelegate>();
			return cmds;
		}

		public override void SetDefaultSettings(IReSettings sets) {
			sets.RegisterSetting(this, "image.quality", "0", SettingType.INT);
		}

		/************************************************
		***  Commands
		*************************************************/
		private IReturn Cmd_Scale(ICore wah, IList<string> args, IDictionary<string, string> flags) {
			//Usage: mei scale -i=[img-name|dir-name] -o=[out-file|out-dir] -h=300 -w=200
			//TODO ImageReturn, display automatically image in picture box
			return new NoReturn();
		}
		private IReturn Cmd_Chop(ICore wah, IList<string> args, IDictionary<string, string> flags) {
			//Usage: mei chop -i=[img-name|dir-name] -o=[out-file|out-dir] -l=50 -r=200
			return new NoReturn();
		}
	}
}
