﻿using System;
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

		public override Dictionary<string, CommandDelegate> InitializeCommands() {
			Dictionary<string, CommandDelegate> cmds = new Dictionary<string, CommandDelegate>();
			cmds.Add("namae", Cmd_Namae);
			cmds.Add("hitode", Cmd_HitodeKangaeru);
			cmds.Add("miru", Cmd_Miru);
			return cmds;
		}

		public override void InitializeSettings(ISettings sets) {
			sets.RegisterSetting(this, "login.timeout", "120", SettingType.INT);

		}

		/************************************************
		***  Commands
		*************************************************/
		private IReturn Cmd_Namae(ICore wah, IList<string> args, IDictionary<string, string> flags) {
			wah.Putln("FUKO MODULE DESU");
			return new StringReturn("Fuko desu yo~!");
		}

		private IReturn Cmd_HitodeKangaeru(ICore wah, IList<string> args, IDictionary<string, string> flags) {
			wah.Display.ShowPersona(new BouncingChangeAnimation(wah.Disk.LoadImageDir(this, "anime1", ".gif"), System.Drawing.Point.Empty));
			while (true);
		}

		private IReturn Cmd_Miru(ICore wah, IList<string> args, IDictionary<string, string> flags) {
			wah.Display.ShowPersona(new SimpleImage(wah.Disk.LoadImage(this, "fuko.png")));
			return new NoReturn();
		}

	}
}
