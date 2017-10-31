using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wah_Interface {
	public abstract class NewAWahException : Exception {
		/// <summary>
		/// Displays the exception message on the given wah
		/// </summary>
		public abstract void DisplayOn(IWah wah);
	}
	////////////////////////////////////////////////////////
	//CODER FAULTS
	////////////////////////////////////////////////////////

	/// <summary>
	/// Models an exception that occurs when a resource a coder is trying to load cannot be loaded
	/// </summary>
	public class WahHelpParseException : NewAWahException {
		private string message;
		public WahHelpParseException(string message) {
			this.message = message;
		}
		public override void DisplayOn(IWah wah) {
			wah.PutErr(message);
		}
	}

	/// <summary>
	/// Models an exception that occurs when a resource a coder is trying to load cannot be loaded
	/// </summary>
	public class WahIOLoadException : NewAWahException {
		private string message;
		public WahIOLoadException(string message) {
			this.message = message;
		}
		public override void DisplayOn(IWah wah) {
			wah.PutErr(message);
		}
	}

	/// <summary>
	/// Models an exception that occurs when a coder tries to cast something to a wrong type.
	/// </summary>
	public class WahWrongTypesException : NewAWahException {
		private string from, to;
		public WahWrongTypesException(Type expectedType, IData actual) {
			from = actual.GetType().Name;
			to = actual.GetType().Name;
		}

		public override void DisplayOn(IWah wah) {
			wah.PutErr("Code error: cannot convert type " + from + " into type " + to);
		}
	}

	/// <summary>
	/// Models an exception that occurs when a coder tries to access an argument that the user did not provide.
	/// It is not the user's fault, however, because the coder's enforced rules should have caught the argument
	/// missing if it was supposed to be the user's fault.
	/// </summary>
	public class WahArgumentMissingException : NewAWahException {
		public WahArgumentMissingException() { }

		public override void DisplayOn(IWah wah) {
			wah.PutErr("Code error: accessed argument that did not exist: check enforced" +
				"rules to ensure the argument will be there");
		}
	}

	/// <summary>
	/// Models an exception that occurs when a coder makes an invalid command
	/// </summary>
	public class WahInvalidCommandException : NewAWahException {
		public WahInvalidCommandException() { }

		public override void DisplayOn(IWah wah) {
			wah.PutErr("Code error: cannot create a null command or a command with the same" +
			"name as another in the same module");
		}
	}

	/// <summary>
	/// Models an exception that occurs when a coder makes calls an invalid id for a visual to display
	/// </summary>
	public class WahInvalidVisualException : NewAWahException {
		private int id;
		public WahInvalidVisualException(int id) : base() {
			this.id = id;
		}

		public override void DisplayOn(IWah wah) {
			wah.PutErr("Code error: cannot display visual,"
				+ " too high/low an identifier: " + id);
		}
	}

	/// <summary>
	/// Models an exception that occurs when a coder tries to apply a list function to a specific type, but the 
	/// list does not solely contain elements of that type
	/// </summary>
	public class WahListTypeException : WahWrongTypesException {
		public WahListTypeException(Type expectedType, IData actual) : base(actual.GetType(), actual) { }
	}
	////////////////////////////////////////////////////////
	//USER FAULTS
	////////////////////////////////////////////////////////
	/// <summary>
	/// Models an exception caused by some sort of user error
	/// </summary>
	public abstract class WahUserException : NewAWahException {
		
	}

	/// <summary>
	/// Models an exception that occurs when the user tries to call a command that has the same name in two or more
	/// different modules.
	/// </summary>
	public class WahAmbiguousCommandException : WahUserException {
		public WahAmbiguousCommandException() { }

		public override void DisplayOn(IWah wah) {
			wah.PutErr("Error: Multiple modules have a command with the same"
			+ " name, please specify which command you want run");
		}
	}

	/// <summary>
	/// Models an exception that occurs when the user tries to call a command that doesn't exist
	/// </summary>
	public class WahNoCommandException : WahUserException {
		private string cmd;
		public WahNoCommandException(string cmd) {
			this.cmd = cmd;
		}
		public override void DisplayOn(IWah wah) {
			wah.PutErr("Error: The given command \"" + cmd + "\" does not exist.");
		}
	}

	/// <summary>
	/// Models an exception that occurs when the user tries to call a module that doesn't exist
	/// </summary>
	public class WahNoModuleException : WahUserException {
		private string module;
		public WahNoModuleException(string module) {
			this.module = module;
		}
		public override void DisplayOn(IWah wah) {
			wah.PutErr("Error: The given module \"" + module + "\" does not exist or is not loaded.");
		}
	}

	/// <summary>
	/// Models an exception that occurs when the user formats a command or argument or flag incorrectly
	/// </summary>
	public class WahBadFormatException : WahUserException {
		private string str;
		public WahBadFormatException(string str) {
			this.str = str;
		}
		public override void DisplayOn(IWah wah) {
			wah.PutErr("Error: The given string \"" + str + "\" is not a valid input.");
		}
	}

	/// <summary>
	/// Models an exception that occurs when the user requests something that is missing
	/// </summary>
	public class WahMissingInfoException : WahUserException {
		private string message;
		public WahMissingInfoException(string message) {
			this.message = message;
		}
		public override void DisplayOn(IWah wah) {
			wah.PutErr(message);
		}
	}
}
