﻿using System;
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
