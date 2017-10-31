using System;
using System.Collections.Generic;
using System.Linq;

namespace Wah_Interface {
	
	public abstract class OldAModule {

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
	}

}
