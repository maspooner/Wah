using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wah_Interface;

namespace Wah_Commands {
	class Fuko : AModule {
		Fuko(string name) : base(name) {

		}

		public override Dictionary<string, CommandDelegate> InitializeCommands() {
			throw new NotImplementedException();
		}

		public override void InitializeSettings(ISettings sets) {
			throw new NotImplementedException();
		}

	}
}
