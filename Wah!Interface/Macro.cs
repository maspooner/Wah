using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wah_Interface {
	public class Macro {
		string From { get; set; }
		string To { get; set; }
		public Macro(string from, string to) {
			From = from;
			To = to;
		}
	}
}
