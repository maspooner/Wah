using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wah_Interface {
	public abstract class AWahException : Exception {
		public AWahException(string message) : base(message) {

		}
		public AWahException(string message, Exception innerException) : base(message, innerException) {

		}
		public void OutputError(ICore wah) {
			if (!IsThreadAbort()) {
				wah.PutErr(GetMessages());
			}
		}
		private bool IsThreadAbort() {
			if(InnerException is System.Threading.ThreadAbortException) {
				return true;
			}
			if (InnerException != null && InnerException is AWahException) {
				return (InnerException as AWahException).IsThreadAbort();
			}
			return false;
		}
		public string GetMessages() {
			return GetMessages(this, 0);
		}
		protected string GetMessages(Exception ex, int tabs) {
			string sTabs = new string(' ', tabs * 2);
			if (ex.InnerException == null) {
				return sTabs + ex.Message;
			}
			else {
				return sTabs + ex.Message + "\n" + GetMessages(ex.InnerException, tabs + 1);
			}
		}
	}
	internal class NoReturnException : AWahException {
		public NoReturnException() : base("Attempted to use the value of a call to a command with no return value") {

		}
	}
	public class NoSuchItemException : AWahException {
		public NoSuchItemException(string message) : base(message) {

		}
	}
	public class WrongDataTypeException : AWahException {
		public WrongDataTypeException(string message) : base(message) {

		}
	}
	public class IllformedInputException : AWahException {
		public IllformedInputException(string message) : base(message) {

		}
	}
	public class IOLoadException : AWahException {
		public IOLoadException(string message) : base(message) {

		}
		public IOLoadException(string message, Exception inner) : base(message, inner) {

		}
	}
	public class HelpParseException : AWahException {
		public HelpParseException(string message) : base(message) {

		}
	}
	public class InvalidStateException : AWahException {
		public InvalidStateException(string message) : base(message) {

		}
	}


	//Chain exceptions
	public class CallFailedException : AWahException {
		public CallFailedException(string message, AWahException innerException) 
			: base(message, innerException) {

		}
	}
	public class UnhandledException : AWahException {
		public UnhandledException(string message, Exception innerException) 
			: base(message, innerException) {

		}
	}

}
