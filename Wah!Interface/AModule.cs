using System;
using System.Collections.Generic;
using System.Linq;

namespace Wah_Interface {
	/// <summary>
	/// Designed to hold the information a command needs to execute properly
	/// </summary>
	public class CommandBundle {
		public IList<IData> Arguments { get; private set; }
		public IDictionary<string, string> Flags { get; private set; }

		public CommandBundle(IList<IData> arguments, IDictionary<string, string> flags) {
			Arguments = arguments;
			Flags = flags;
		}

		public bool HasFlag(string key) {
			return Flags.ContainsKey(key);
		}
		public void AddFlag(string key, string value) {
			Flags.Add(key, value);
		}
		public string ArgStringOf(int i) {
			return Arguments[i].AsString();
		}
		public bool ArgCount(int c) {
			return Arguments.Count == c;
		}
		public bool FlagCount(int c) {
			return Flags.Count == c;
		}
		public IList<string> StringArgs() {
			return Arguments.Select(data => data.AsString()).ToList();
		}
	}
	public abstract class AModule {
		private static readonly char[] DELIMS = new char[] { ' ' };
		//the identifier used to use the previous returned value
		private const string PREVIOUS_ID = "<>";

		public delegate IData CommandDelegate(ICore wah, CommandBundle bun);
		public string Name { get; private set; }
		public string Version { get; private set; }
		public Dictionary<string, CommandDelegate> Commands { get; private set; }
		public AModule(string name, string version) {
			Name = name;
			Version = version;
			Commands = InitializeCommands();
		}
		public abstract Dictionary<string, CommandDelegate> InitializeCommands();
		public abstract void SetDefaultSettings(IReSettings sets);
		public IData Handle(ICore wah, string cmd, string rest, IData previous) {
			if (Commands.ContainsKey(cmd)) {
				IList<string> args = new List<string>(rest.Split(DELIMS, StringSplitOptions.RemoveEmptyEntries));
				IDictionary<string, string> flags = new Dictionary<string, string>();
				//move the flags from the arguments list to the new flags list
				ParseFlags(args, flags);
				//convert string data into object data
				IList<IData> valArgs = ToDataList(args, previous);
				return Commands[cmd](wah, new CommandBundle(valArgs, flags));
			}
			else {
				throw new NoSuchItemException(cmd + " does not exist, desun!");
			}
		}
		/// <summary>
		/// Takes a list of string arguments and parses out the ones that are really flags,
		/// removing them from the argument list and adding them to the given flags list
		/// </summary>
		private void ParseFlags(IList<string> args, IDictionary<string, string> flags) {
			for (int i = 0; i < args.Count; i++) {
				if (args[i].StartsWith("-") || args[i].StartsWith("--")) {
					string flag = args[i];
					args.RemoveAt(i);
					i--;
					string[] flagValue = flag.Split(new char[] { '=' }, 2);
					if (!flags.ContainsKey(flagValue[0])) {
						if (flagValue.Length == 1) {
							//only a flag
							flags.Add(flag, "");
						}
						else {
							string val = flagValue[1];
							//surrounded by "", remove them
							if (val.StartsWith("\"") && val.EndsWith("\"")) {
								val = val.Substring(1, val.Length - 2);
							}
							//flag=value | flag="multi word flag"
							flags.Add(flagValue[0], val);
						}
					}
				}
			}
		}
		/// <summary>
		/// Converts the given string list into one of IData, doing any conversions necessary
		/// </summary>
		private IList<IData> ToDataList(IList<string> args, IData previous) {
			IList<IData> values = new List<IData>();
			
			foreach(string arg in args) {
				//use previous value
				if(arg.Equals(PREVIOUS_ID)) {
					//no previous value, should not be using the previous value marker
					if(previous == null) {
						throw new Wah_Interface.NoSuchItemException("No previous returned value, nothing done");
					}
					values.Add(previous);
				}
				//TODO recognize special image strings to convert
				//else if (arg.StartsWith("@_") {

				//}
				else {
					//just use a normal string
					values.Add(new StringData(arg));
				}
			}
			return values;
		}
	}
}
