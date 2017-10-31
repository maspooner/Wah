using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wah_Interface;

namespace Wah_Core {
	/// <summary>
	/// The part of the wah processing that acts as parser for user input into commands and bundles
	/// </summary>
	internal partial class WahProcessor : IParser {
		private const char MODULE_DELIM = '.'; //Separates modules from commands
		private const string PREVIOUS_OUTPUT_MARKER = "<>"; //Marks the user wanting to use the output of the last command
		private const string MACRO_MARKER = ";"; //Surrounds a macro which expands into an expression
		private const char ESCAPE_MARKER = '\\'; //Marks a symbol to be interpreted literally
		private const char LITERAL_MARKER = '\"'; //Encloses some symbols to be interpreted literally

		private static readonly string ESCAPED_LITERAL = ESCAPE_MARKER + "" + LITERAL_MARKER;
		private static readonly string ESCAPED_MACRO = ESCAPE_MARKER + "" + MACRO_MARKER;

		public string ExpandMacros(string line) {
			int iLit = line.IndexOf(LITERAL_MARKER);
			//contains "
			if(iLit >= 0) {
				int iEscLit = line.IndexOf(ESCAPED_LITERAL);
				//contains \"
				if(iEscLit >= 0) {
					//skip \", expand rest
					return line.Substring(0, iEscLit) 
						+ ExpandMacros(line.Substring(iEscLit + ESCAPED_LITERAL.Length));
				}
				//no \", yes "
				else {
					int iLitSec = line.IndexOf(LITERAL_MARKER, iLit);
					// second "
					if(iLitSec >= 0) {

					}
					// no second "
					else {

					}
				}
			}
			//no "
			else {
				int iMac = line.IndexOf(MACRO_MARKER);
				// yes ;
				if(iMac >= 0) {
					int iEscMac = line.IndexOf(ESCAPED_MACRO);
					// yes \;
					if(iEscMac >= 0) {

					}
					//yes ; but no \;
					else {

					}
				}
				//no ;
				else {
					//all done
					return line;
				}
			}
			//TODO 
			return line;
		}

		public ICommand ParseCommand(string cmd) {
			cmd = cmd.Trim();
			if (cmd.Contains(MODULE_DELIM)) {
				//must be within a specific module
				string[] pieces = cmd.Split(moduleDelimCache, StringSplitOptions.RemoveEmptyEntries);
				if (pieces.Length == 2) {
					string mod = pieces[0];
					cmd = pieces[1];
					if (ModuleLoaded(mod)) {
						IModule module = FindModule(mod);
						if (module.HasCommand(cmd)) {
							return module.GetCommand(cmd);
						}
						else {
							//user error: command does not exist
							throw new WahNoCommandException(cmd);
						}
					}
					else {
						//user error: module not loaded or does not exist
						throw new WahNoModuleException(mod);
					}
				}
				else {
					//user error: too many dots in module name
					throw new WahBadFormatException(pieces[0]);
				}
			}
			else {
				//can be in any module
				//how many times does the command appear accross all modules?
				int shareName = modules.Count(mod => mod.HasCommand(cmd));
				if (shareName == 0) {
					//no such command, user error
					throw new WahNoCommandException(cmd);
				}
				else if (shareName == 1) {
					//know exactly which module to run
					IModule module = modules.First(mod => mod.HasCommand(cmd));
					if (module.HasCommand(cmd)) {
						return module.GetCommand(cmd);
					}
					else {
						//user error: command does not exist
						throw new WahNoCommandException(cmd);
					}
				}
				else {
					//too many, don't know which one the user wants to run
					throw new WahAmbiguousCommandException();
				}
			}
		}

		public IBundle ParseBundle(string bun) {
			ISet<string> flags = new HashSet<string>();
			IDictionary<char, IData> arguments = new Dictionary<char, IData>();
			return new CommandBundle(flags, arguments);
		}
	}
}
