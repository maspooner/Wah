﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wah_Interface;

namespace Wah_Commands {
	public class Akari : AModule {
		private const string AKARI_NO_NAMAE = "akari";
		private const string AKARI_NO_HAN = "Pomf alpha 0";
		
		public Akari() : base(AKARI_NO_NAMAE, AKARI_NO_HAN) { }

		protected override NewICommand[] CreateCommands() {
			return new NewICommand[] {
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