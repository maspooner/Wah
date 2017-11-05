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
		private const char MACRO_MARKER = ';'; //Surrounds a macro which expands into an expression
		private const char ESCAPE_MARKER = '\\'; //Marks a symbol to be interpreted literally
		private const char LITERAL_MARKER = '\"'; //Encloses some symbols to be interpreted literally
		private const char FLAG_MARKER = '-'; //Comes before a string to specify a flag
		private const char ARG_SET_MARKER = '='; //Comes in between an argument id and argument data

		public string ExpandMacros(string line) {
			StringBuilder newLine = new StringBuilder();
			int pos = 0;
			while(pos < line.Length) {
				//found \
				if(line[pos].Equals(ESCAPE_MARKER)) {
					//skip escaping for now, just add characters
					newLine.Append(line[pos]);
					//don't check for faulty \ just yet
					if(pos + 1 < line.Length) {
						newLine.Append(line[pos + 1]);
					}
					//move over the 2 characters
					pos += 2;
				}
				//found "
				else if(line[pos].Equals(LITERAL_MARKER)) {
					//find where the quote ends
					int afterQuote = SkipQuoted(line, pos + 1) + 1;
					//add the whole quoted bit
					for (int i = pos; i < line.Length && i < afterQuote; i++) {
						newLine.Append(line[i]);
					}
					//skip over the quoted text
					pos = afterQuote;
					
				}
				//macro start ;
				else if(line[pos].Equals(MACRO_MARKER)) {
					//skip ;
					pos++;
					string macro = ParseMacro(line, pos);
					//skip over macro and final ;
					pos += macro.Length + 1;
					if(macros.ContainsKey(macro)) {
						//insert applied macro into result
						newLine.Append(macros[macro]);
					}
					else {
						throw new WahCommandParseException("Macro does not exist: " + macro);
					}
				}
				//other text
				else {
					//just add
					newLine.Append(line[pos]);
					pos++;
				}
			}
			return newLine.ToString();
		}

		/// <summary>
		/// Finds the first position after a literal end marker after the given position in the given line
		/// </summary>
		/// <param name="line">the line of text</param>
		/// <param name="pos">the start position</param>
		/// <returns>the position after the literal end marker</returns>
		private int SkipQuoted(string line, int pos) {
			StringBuilder quoted = new StringBuilder();
			while(pos < line.Length) {
				//skip over escaped character
				if(line[pos].Equals(ESCAPE_MARKER)) {
					pos += 2;
				}
				//found end quote
				else if(line[pos].Equals(LITERAL_MARKER)) {
					return pos;
				}
				//normal char
				else {
					pos += 1;
				}
			}
			throw new WahCommandParseException("Missing end literal: " + LITERAL_MARKER);
		}

		/// <summary>
		/// Parses the macro starting at the given position in the given line
		/// </summary>
		/// <param name="line">the line </param>
		/// <param name="pos">the position</param>
		/// <returns>the macro</returns>
		private string ParseMacro(string line, int pos) {
			StringBuilder macro = new StringBuilder();
			while (pos < line.Length) {
				//found end ;
				if (line[pos].Equals(MACRO_MARKER)) {
					return macro.ToString();
				}
				//invalid macro name
				else if (InvalidCharName(line[pos])) {
					throw new WahCommandParseException("Macro contains invalid symbol: " + MACRO_MARKER);
				}
				else {
					macro.Append(line[pos]);
					pos++;
				}
			}
			throw new WahCommandParseException("Found no end to macro: " + MACRO_MARKER);
		}

		public ICommand ParseCommand(string cmd) {
			cmd = cmd.Trim();
			if (cmd.Contains(MODULE_DELIM)) {
				//index of .
				int iMod = cmd.IndexOf(MODULE_DELIM);
				string mod = cmd.Substring(0, iMod);
				if (iMod + 1 >= cmd.Length) {
					throw new WahCommandParseException("Empty command");
				}
				else {
					cmd = cmd.Substring(iMod + 1);
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
			bun = bun.Trim();
			return ParseBundle(bun, 0);
		}

		/// <summary>
		/// Is the character allowed in the name of a flag or argument?
		/// </summary>
		private bool InvalidCharName(char c) {
			//can't be " or \ or = or ;
			return c.Equals(LITERAL_MARKER) || c.Equals(ESCAPE_MARKER)
					|| c.Equals(ARG_SET_MARKER) || c.Equals(MACRO_MARKER);
		}

		/// <summary>
		/// Parses bundle data from the line starting at the given position
		/// </summary>
		/// <param name="line">the line to parse</param>
		/// <param name="pos">the position to start at</param>
		/// <returns>the bundle</returns>
		private IBundle ParseBundle(string line, int pos) {
			ISet<string> flags = new HashSet<string>();
			IDictionary<char, IData> arguments = new Dictionary<char, IData>();
			while (pos < line.Length) {
				//whitespace ignored
				if (line[pos].Equals(' ')) {
					pos++;
				}
				//first char is -
				else if (line[pos].Equals(FLAG_MARKER)) {
					//skip -
					pos += 1;
					string flag = ParseFlag(line, pos);
					//skip over flag just processed
					pos += flag.Length;
					flags.Add(flag);
				}
				//first char is " or \ or =
				else if (InvalidCharName(line[pos])) {
					throw new WahCommandParseException("Invalid argument name: " + line[pos]);
				}
				//first char is valid arg id
				else {
					char id = line[pos];
					if(pos + 1 < line.Length) {
						//next is =
						if(line[pos + 1].Equals(ARG_SET_MARKER)) {
							//skip id and =
							pos += 2;
							//has a "
							if (pos < line.Length && line[pos].Equals(LITERAL_MARKER)) {
								//skip "
								pos++;
								//get data
								string rawData = ParseQuoted(line, pos);
								//skip over rawData just processed and 2 quotes
								pos += rawData.Length + 2;
								//add the argument (and parse the data)
								arguments.Add(id, ParseData(rawData));
							}
							else {
								//get data
								string rawData = ParseStringData(line, pos);
								//skip over rawData just processed
								pos += rawData.Length;
								//add the argument (and parse the data)
								arguments.Add(id, ParseData(rawData));
							}
							
						}
						else {
							//id either mutliple characters or not properly set up
							throw new WahCommandParseException("Argument " + id + "is not followed by the set operator "
								+ ARG_SET_MARKER);
						}
					}
					else {
						//id without =
						throw new WahCommandParseException("Argument " + id + " does not have any content.");
					}
					
				}
			}
			return new CommandBundle(flags, arguments);
		}

		/// <summary>
		/// Parses a flag string name from the given line starting at the given position
		/// </summary>
		/// <param name="line">the line</param>
		/// <param name="pos">the position</param>
		/// <returns>the flag name</returns>
		private string ParseFlag(string line, int pos) {
			StringBuilder flag = new StringBuilder();
			//while still 2 character pairs
			while(pos + 1 < line.Length) {
				//first is invalid for a name
				if(InvalidCharName(line[pos])) {
					throw new WahCommandParseException("Invalid character in flag name: " + line[pos]);
				}
				//second is = -> first is arg id -> flag done
				else if(line[pos + 1].Equals(ARG_SET_MARKER)) {
					if (flag.Length == 0) {
						throw new WahCommandParseException("Empty flag");
					}
					else {
						return flag.ToString();
					}
				}
				//valid character
				else {
					//add to flag
					flag.Append(line[pos]);
					pos++;
				}
			}
			//last character check
			if(pos == line.Length - 1) {
				if(InvalidCharName(line[pos])) {
					throw new WahCommandParseException("Invalid character in flag name: " + line[pos]);
				}
				else {
					//add to flag
					flag.Append(line[pos]);
					pos++;
				}
			}
			//no empty flags
			if (flag.Length == 0) {
				throw new WahCommandParseException("Empty flag");
			}
			else {
				return flag.ToString();
			}
		}

		/// <summary>
		/// Parses string data from the given line starting at the given position
		/// </summary>
		/// <param name="line">the line</param>
		/// <param name="pos">the position</param>
		/// <returns>the string data</returns>
		private string ParseStringData(string line, int pos) {
			StringBuilder sb = new StringBuilder();
			//while input left in line
			while (pos < line.Length) {
				//ignore whitespace
				if(line[pos].Equals(' ')) {
					pos++;
				}
				//first char is "
				else if (line[pos].Equals(LITERAL_MARKER)) {
					throw new WahCommandParseException("In argument data, no literal markers can start mid-way through data");
				}
				//first char is \
				else if (line[pos].Equals(ESCAPE_MARKER)) {
					//if there is a character following \ to escape
					if (pos + 1 < line.Length) {
						//escape the character following
						sb.Append(line[pos + 1]);
						//skip \ and char
						pos += 2;
					}
					else {
						//no character to escape
						throw new WahCommandParseException("Found no character to escape after " + ESCAPE_MARKER);
					}
				}
				//normal text
				else {
					//next char is = -> this char is argument id -> done
					if(pos + 1 < line.Length && line[pos + 1].Equals(ARG_SET_MARKER)) {
						return sb.ToString();
					}
					else {
						sb.Append(line[pos]);
						pos += 1;
					}
				}
			}
			return sb.ToString();
		}

		/// <summary>
		/// Parses string data from within a quoted context from the given line starting at the given position
		/// </summary>
		/// <param name="line">the line</param>
		/// <param name="pos">the position</param>
		/// <returns>the string data</returns>
		private string ParseQuoted(string line, int pos) {
			StringBuilder sb = new StringBuilder();
			//while input left in line
			while(pos < line.Length) {
				//first char is "
				if(line[pos].Equals(LITERAL_MARKER)) {
					//return what we have built
					return sb.ToString();
				}
				//first char is \
				else if (line[pos].Equals(ESCAPE_MARKER)) {
					//if there is a character following \ to escape
					if (pos + 1 < line.Length) {
						//escape the character following
						sb.Append(line[pos + 1]);
						//skip \ and char
						pos += 2;
					}
					else {
						//no character to escape
						throw new WahCommandParseException("Found no character to escape after " + ESCAPE_MARKER);
					}
				}
				//normal text
				else {
					sb.Append(line[pos]);
					pos += 1;
				}
			}
			//no end quote found
			throw new WahCommandParseException("No end literal marker found: " + LITERAL_MARKER);
		}
	}
}
