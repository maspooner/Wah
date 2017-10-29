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
		public void OutputError(IWah wah) {
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
}
