using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wah_Interface {
	public interface IReturn {
		string AsString();
		int AsInt();
		bool AsBool();
	}
	public class NoReturn : IReturn {
		public bool AsBool() {
			throw new NoReturnException();
		}

		public int AsInt() {
			throw new NoReturnException();
		}

		public string AsString() {
			throw new NoReturnException();
		}
	}
	public class StringReturn : IReturn {
		public string Value { get; private set; }
		public StringReturn(string value) {
			Value = value;
		}
		public bool AsBool() {
			bool b = false;
			if (bool.TryParse(Value, out b)) {
				return b;
			}
			else {
				throw new IllformedInputException("data " + Value + " cannot be cast to a bool");
			}
		}

		public int AsInt() {
			int o = 0;
			if (int.TryParse(Value, out o)) {
				return o;
			}
			else {
				throw new IllformedInputException("data " + Value + " cannot be cast to an int");
			}
		}

		public string AsString() {
			return Value;
		}
	}
	public class IntReturn : IReturn {
		public int Value { get; private set; }
		public IntReturn(int value) {
			Value = value;
		}

		public string AsString() {
			return Value.ToString();
		}

		public int AsInt() {
			return Value;
		}

		public bool AsBool() {
			return Value != 0;
		}
	}
	public class BoolReturn : IReturn {
		public bool Value { get; private set; }
		public BoolReturn(bool value) {
			Value = value;
		}

		public string AsString() {
			return Value.ToString();
		}

		public int AsInt() {
			return Value ? 1 : 0;
		}

		public bool AsBool() {
			return Value;
		}
	}

	public class ListReturn : IReturn {
		public IList<string> Value { get; private set; }
		public ListReturn(IList<string> value) {
			Value = value;
		}

		public string AsString() {
			return string.Join("\n", Value);
		}

		public int AsInt() {
			throw new IllformedInputException("data of ListReturn cannot be cast to an int");
		}

		public bool AsBool() {
			throw new IllformedInputException("data of ListReturn cannot be cast to a bool");
		}
	}

}
