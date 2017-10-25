using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wah_Interface {
	/// <summary>
	/// Models a rule for an argument or a flag that must hold true for a Command to validly run.
	/// </summary>
	public interface IRule {
		bool Validate(IBundle bun);
		string Violation();
	}

	/// <summary>
	/// Models a rule that requires an argument (not flag) to appear alongside a Command.
	/// </summary>
	public class RequireRule : IRule {
		private char id;

		public RequireRule(char id) {
			this.id = id;
		}

		public bool Validate(IBundle bun) {
			return bun.HasArgument(id);
		}

		public string Violation() {
			//user forgot argument
			return "The " + id + " argument must be present.";
		}
	}

	/// <summary>
	/// Models a rule that requires an argument to be of a certain type.
	/// </summary>
	public class TypeRule<D> : IRule where D : IData {
		private char id;

		public TypeRule(char id) {
			this.id = id;
		}

		public bool Validate(IBundle bun) {
			//must have argument and it be the right type or not have the argument altogether
			return !bun.HasArgument(id) || (bun.HasArgument(id) && bun.Argument(id) is D);
		}

		public string Violation() {
			//user used wrong argument type
			return "The " + id + " argument is of the wrong type";
		}
	}

	/// <summary>
	/// Models a rule that requires 2 arguments to either both appear or not appear alongside a Command.
	/// </summary>
	public class CorequireRule : IRule {
		private char id1;
		private char id2;

		public CorequireRule(char id1, char id2) {
			this.id1 = id1;
			this.id2 = id2;
		}

		public bool Validate(IBundle bun) {
			bool h1 = bun.HasArgument(id1);
			bool h2 = bun.HasArgument(id2);
			return (h1 && h2) || !(h1 || h2);
		}

		public string Violation() {
			//user put in either id1 or id2 but not both and not neither
			return "The " + id1 + " and " + id2 + " arguments must both be present.";
		}
	}
}
