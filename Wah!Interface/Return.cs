using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wah_Interface {
	public interface IReturn {
		R Accept<R>(IReturnVisitor<R> visitor);
	}
	public class NoReturn : IReturn {
		public R Accept<R>(IReturnVisitor<R> visitor) {
			return visitor.VisitNone(this);
		}
	}
	public class ErrorReturn : IReturn {
		public WahException Exception { get; private set; }
		public ErrorReturn(WahException exception) {
			Exception = exception;
		}
		public R Accept<R>(IReturnVisitor<R> visitor) {
			return visitor.VisitError(this);
		}
	}
	public class StringReturn : IReturn {
		public string Value { get; private set; }
		public StringReturn(string value) {
			Value = value;
		}
		public R Accept<R>(IReturnVisitor<R> visitor) {
			return visitor.VisitString(this);
		}
	}
	public interface IReturnVisitor<R> {
		R Visit(IReturn ir);
		R VisitNone(NoReturn nr);
		R VisitError(ErrorReturn er);
		R VisitString(StringReturn sr);
	}
	public class ReturnToString : IReturnVisitor<string> {
		public string Visit(IReturn ir) {
			return ir.Accept(this);
		}

		public string VisitError(ErrorReturn er) {
			throw er.Exception;
		}

		public string VisitNone(NoReturn nr) {
			throw new NoReturnException();
		}

		public string VisitString(StringReturn sr) {
			return sr.Value;
		}
	}
}
