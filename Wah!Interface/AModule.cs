using System;
using System.Collections.Generic;

namespace Wah_Interface {
	public abstract class AModule {
		private static readonly char[] DELIMS = new char[] { ' ' };
		public delegate IReturn CommandDelegate(ICore wah, IList<string> args, IDictionary<string, string> flags);
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
		public IReturn Handle(ICore wah, string cmd, string rest) {
			if (Commands.ContainsKey(cmd)) {
				IList<string> args = new List<string>(rest.Split(DELIMS, StringSplitOptions.RemoveEmptyEntries));
				IDictionary<string, string> flags = new Dictionary<string, string>();
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
				return Commands[cmd](wah, args, flags);
			}
			else {
				throw new NoSuchItemException(cmd + " does not exist, desun!");
			}
		}
	}
}
