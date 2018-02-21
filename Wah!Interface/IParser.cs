using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wah_Interface {
	/// <summary>
	/// Models a parser to turn command line strings into command bundles and command objects
	/// </summary>
	public interface IParser {
		/// <summary>
		/// Parses the given string into a command and bundle.
		/// </summary>
		/// <returns>the parsed command and bundle</returns>
		Tuple<ICommand, IBundle> Parse(string input);
	}
}
