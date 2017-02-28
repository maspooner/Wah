using System;
using System.Collections.Generic;

namespace Wah_Interface {
	public abstract class AModule {
		private static readonly char[] DELIMS = new char[] { ' ' };
		public delegate IReturn CommandDelegate(ICore wah, List<string> args, Dictionary<string, string> flags);
		public string Name { get; set; }
		public Dictionary<string, CommandDelegate> Commands { get; private set; }
		public AModule(string name) {
			Name = name;
			Commands = InitializeCommands();
		}
		public abstract Dictionary<string, CommandDelegate> InitializeCommands();
		public abstract void InitializeSettings(ISettings sets);
		public IReturn Handle(ICore wah, string cmd, string rest) {
			if (Commands.ContainsKey(cmd)) {
				List<string> args = new List<string>(rest.Split(DELIMS, StringSplitOptions.RemoveEmptyEntries));
				Dictionary<string, string> flags = new Dictionary<string, string>();
				for(int i = 0; i < args.Count; i++) {
					if(args[i].StartsWith("-") || args[i].StartsWith("--")) {
						string flag = args[i];
						args.RemoveAt(i);
						i--;
						string[] flagValue = flag.Split(new char[] { '=' }, 2);
						if(flagValue.Length == 1) {
							//only a flag
							flags.Add(flag, "");
						}
						else {
							//flag=value
							flags.Add(flagValue[0], flagValue[1]);
						}
					}
				}
                return Commands[cmd](wah, args, flags);
			}
			else {
				throw new NoSuchItemException(cmd + " does not exist, desun!");
			}
		}
		public void Execute(ICore wah, string cmd, string args) {
			Handle(wah, cmd, args);
		}
		public IReturn Call(ICore wah, string cmd, string args) {
			return Handle(wah, cmd, args);
		}

	}
}
