using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wah_Interface;

namespace Wah_Core {
	internal partial class WahProcessor : IParser {
		static char[] SPECIALS = { ',', '-', '=', '@', '$', '[', ']', '^', ':' };

		public Tuple<ICommand, IBundle> Parse(string input) {
			int cmdend;
			IList<string> tokens = ExpandMacros(Tokenize(input));
			ICommand cmd = ParseCommand(tokens, out cmdend);
			IBundle bun = ParseBundle(tokens, cmdend);
			return Tuple.Create(cmd, bun);
		}

		private IList<string> ExpandMacros(IList<string> tokens) {
			IList<string> newTokens = new List<string>();
			for(int i = 0; i < tokens.Count; i++) {
				if(tokens[i].Equals(":")) {
					if(i + 1 < tokens.Count) {
						string macro = tokens[i + 1];
						//valid name
						if(!SPECIALS.Contains(macro[0])) {
							if (macros.ContainsKey(macro)) {
								//insert applied macro into result
								foreach(string t in Tokenize(macros[macro])){
									newTokens.Add(t);
								}
								//skip 1 token
								i += 1;
							}
							else {
								throw new WahCommandParseException("Macro does not exist: " + macro);
							}
						}
						else {
							throw new Exception("invalid syntax at " + macro);
						}
					}
					else {
						throw new Exception("no name following macro");
					}
				}
				else {
					//just add the plain token
					newTokens.Add(tokens[i]);
				}
			}
			return newTokens;
		}

		private IList<string> Tokenize(string input) {
			IList<string> toks = new List<string>();
			StringBuilder currTok = new StringBuilder();
			bool inQuote = false;
			bool forceAdd = false;
			foreach (char c in input) {
				//immediately add the next char
				if (forceAdd) {
					currTok.Append(c);
					forceAdd = false;
				}
				//escape any next character
				else if (c == '\\') {
					forceAdd = true;
				}
				//quoting
				else if (c == '"') {
					//end quote
					if (inQuote) {
						//finish token
						toks.Add(currTok.ToString());
						currTok.Clear();
					}
					inQuote = !inQuote;
				}
				//skip whitespace outside of quotes
				else if (!inQuote && char.IsWhiteSpace(c)) {
					//add the token that was being built
					if (currTok.Length > 0) {
						toks.Add(currTok.ToString());
						currTok.Clear();
					}
				}
				else if (inQuote) {
					//just blindly add characters
					currTok.Append(c);
				}
				else if (SPECIALS.Contains(c)) {
					//end current token, add special character
					if (currTok.Length > 0) {
						toks.Add(currTok.ToString());
						currTok.Clear();
					}
					toks.Add(c.ToString());
				}
				//normal character
				else {
					currTok.Append(c);
				}
			}
			//add last token
			if (currTok.Length > 0) {
				toks.Add(currTok.ToString());
				currTok.Clear();
			}
			return toks;
		}
		private ICommand HandleModCmd(string mod, string cmd) {
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
		private ICommand HandleCmd(string cmd) {
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

		private IBundle ParseBundle(IList<string> toks, int start) {
			ISet<string> flags = new HashSet<string>();
			IDictionary<char, IData> arguments = new Dictionary<char, IData>();
			for (int i = start; i < toks.Count; i++) {
				// run another command after
				if (toks[i].Equals("!")) {
					break;
				}
				else if (toks[i].Equals("-")) {
					if (i + 1 < toks.Count) {
						//next token is special, not a name
						if (SPECIALS.Contains(toks[i + 1][0])) {
							throw new Exception("Invalid flag " + toks[i]);
						}
						else {
							//next token is a name
							flags.Add(toks[i + 1]);
							//skip over name token
							i += 1;
						}
					}
					else {
						throw new Exception("Flag requires a name");
					}

				}
				//it's a name
				else if (!SPECIALS.Contains(toks[i][0])) {
					//has two other tokens
					if (i + 2 < toks.Count) {
						if (toks[i + 1].Equals("=")) {
							//add name = argument
							//TODO make not 1 char
							char id = toks[i][0];
							arguments.Add(id, ParseData(toks[i + 2]));
							//skip over two tokens
							i += 2;
						}
						else {
							throw new Exception("Argument requires = in middle");
						}
					}
					else {
						throw new Exception("Argument requires name = and value");
					}
				}
				else {
					throw new Exception("Invalid syntax at " + toks[i]);
				}
			}
			return new CommandBundle(flags, arguments);
		}

		private ICommand ParseCommand(IList<string> toks, out int after) {
			if (toks.Count > 0) {
				string first = toks[0];
				if (toks.Count > 1) {
					string second = toks[1];
					if (second.Equals(",")) {
						if (toks.Count > 2) {
							string third = toks[2];
							//skip 3 tokens
							after = 3;
							// handle the module and command
							return HandleModCmd(first, third);
						}
						else {
							// just 2 tokens
							throw new Exception("invalid syntax, need command after pkg name");
						}
					}
					else {
						//skip 1 token
						after = 1;
						// not a module, definitely a command
						return HandleCmd(first);
					}
				}
				else {
					//skip 1 token
					after = 1;
					//just 1 token
					return HandleCmd(first);
				}
			}
			else {
				// no tokens
				throw new Exception("0 args");
			}
		}
	}
}
