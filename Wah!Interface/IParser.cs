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
		/// Replaces any macros with their representation on the given line of text. Note: Macros can only
		/// be expanded one layer (no nested macros).
		/// </summary>
		/// <param name="input">the line to have macros converted</param>
		/// <returns>the expanded result</returns>
		string ExpandMacros(string input);

		/// <summary>
		/// Parses the given string into a command that can be run
		/// </summary>
		/// <param name="cmd">the string command to parse</param>
		/// <returns>the command object</returns>
		ICommand ParseCommand(string cmd);

		/// <summary>
		/// Parses the given string into a command bundle.
		/// </summary>
		/// <param name="bun">the string bundle to parse</param>
		/// <returns>the parsed bundle</returns>
		IBundle ParseBundle(string bun);
	}
}
