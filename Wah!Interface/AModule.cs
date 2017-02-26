using System;
using System.Collections.Generic;

namespace Wah_Interface {
	public abstract class AModule {
		private static readonly char[] DELIMS = new char[] { ' ' };
		public delegate IReturn CommandDelegate(ICore wah, string[] args); 
		public string Name { get; set; }
		public Dictionary<string, CommandDelegate> Commands { get; private set; }
		public AModule(string name) {
			Name = name;
			Commands = InitializeCommands();
		}
		public abstract Dictionary<string, CommandDelegate> InitializeCommands();
		public abstract void InitializeSettings(ISettings sets);
		public IReturn Handle(ICore wah, string cmd, string args) {
			if (Commands.ContainsKey(cmd)) {
				return Commands[cmd](wah, args.Split(DELIMS, StringSplitOptions.RemoveEmptyEntries));
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
