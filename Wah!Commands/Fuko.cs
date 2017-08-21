using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wah_Interface;

namespace Wah_Commands {
	public class Fuko : AModule {
		private const string FUKO_NO_NAMAE = "fuko";
		private const string FUKO_NO_HAN = "Hitode alpha 0";
		public Fuko() : base(FUKO_NO_NAMAE, FUKO_NO_HAN) {

		}

		public override void InitializeCommands(Dictionary<string, CommandDelegate> cmds) {
			cmds.Add("namae", Cmd_Namae);
			cmds.Add("hitode", Cmd_HitodeKangaeru);
			cmds.Add("miru", Cmd_Miru);
		}

		public override void SetDefaultSettings(IReSettings sets) {
			sets.RegisterSetting(this, "login.timeout", "120", SettingType.INT);

		}

		/************************************************
		***  Commands
		*************************************************/
		/// <summary>
		/// States Fuuko's name
		/// </summary>
		private IData Cmd_Namae(ICore wah, CommandBundle bun) {
			bun.AssertNoArgs();
			return new StringData("Fuko desu yo~!", System.Drawing.Color.Chartreuse);
		}

		private IData Cmd_HitodeKangaeru(ICore wah, CommandBundle bun) {
			bun.AssertNoArgs();
			wah.Display.ShowPersona(new BouncingChangeAnimation(wah.Disk.LoadImageDir(this, "anime1", ".gif"), System.Drawing.Point.Empty));
			while (true);
		}

		private IData Cmd_Miru(ICore wah, CommandBundle bun) {
			bun.AssertNoArgs();
			wah.Display.ShowPersona(new SimpleImage(wah.Disk.LoadImage(this, "fuko.png")));
			return new NoData();
		}

	}
}
