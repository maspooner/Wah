using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
		public void AssertNoArgs() {
			if(Arguments.Count != 0) {
				throw new WrongNumberArgumentsException();
			}
		}
	}
}
