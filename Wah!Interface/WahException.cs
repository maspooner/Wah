using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wah_Interface {
	public class WahException : Exception {
		public WahException(string message) : base(message) {

		}
		public WahException(string message, Exception innerException) : base(message, innerException) {

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
	public class ModuleLoadException : WahException {
		public ModuleLoadException(string message) : base(message) {

		}
	}
	public class CallFailedException : WahException {
		public CallFailedException(string message, Exception innerException) 
			: base(message, innerException) {

		}
	}

}
