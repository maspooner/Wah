using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wah_Interface;

namespace Wah_Commands {
	public class Fuko : AModule {
		private const string FUKO_NO_NAMAE = "fuuko";
		private const string FUKO_NO_HAN = "Hitode alpha 0";
		public Fuko() : base(FUKO_NO_NAMAE, FUKO_NO_HAN) {}

		protected override NewICommand[] CreateCommands() {
			return new NewICommand[]{
				new UncheckedCommand("namae", Cmd_Namae),
				new UncheckedCommand("hitode", Cmd_HitodeKangaeru),
				new UncheckedCommand("miru", Cmd_Miru)
			};
		}
		/************************************************
		***  Commands
		*************************************************/
		/// <summary>
		/// States Fuuko's name
		/// </summary>
		private StringData Cmd_Namae(IWah wah, IBundle bun) {
			return new StringData("Fuko desu yo~!");
		}

		private NoData Cmd_HitodeKangaeru(IWah wah, IBundle bun) {
			Bitmap[] kangaeru = Disk.LoadImageDir("anime1", ".gif");
			wah.Display.ShowVisual(new BouncingChangeAnimation(kangaeru, Point.Empty), 0);
			while (true);
		}

		private IData Cmd_Miru(IWah wah, IBundle bun) {
			wah.Display.ShowVisual(new SimpleImage(Disk.LoadImage("fuko.png")), 0);
			return new NoData();
		}

	}
}
