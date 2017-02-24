using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wah_Interface {
	public class WahException : Exception {
		public WahException(string message) : base(message) {

		}
	}
	internal class NoReturnException : WahException {
		public NoReturnException() : base("") {

		}
	}
	public class NoSuchItemException : WahException {
		public NoSuchItemException(string message) : base(message) {

		}
	}
	public class WrongDataTypeException : WahException {
		public WrongDataTypeException(string message) : base(message) {

		}
	}
	public class IllformedInputException : WahException {
		public IllformedInputException(string message) : base(message) {

		}
	}

}
