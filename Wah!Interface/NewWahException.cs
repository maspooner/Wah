using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wah_Interface {
	////////////////////////////////////////////////////////
	//CODER FAULTS
	////////////////////////////////////////////////////////
	/// <summary>
	/// Models an exception that occurs when a coder tries to cast something to a wrong type.
	/// </summary>
	public class WahWrongTypesException : Exception {
		public WahWrongTypesException() : base("Code error: cast failed exception") { }
	}

	/// <summary>
	/// Models an exception that occurs when a coder tries to access an argument that the user did not provide.
	/// It is not the user's fault, however, because the coder's enforced rules should have caught the argument
	/// missing if it was supposed to be the user's fault.
	/// </summary>
	public class WahArgumentMissingException : Exception {
		public WahArgumentMissingException() : base("Code error: accessed argument that did not exist: check enforced" +
			"rules to ensure the argument will be there") { }
	}

	/// <summary>
	/// Models an exception that occurs when a coder makes an invalid command
	/// </summary>
	public class WahInvalidCommandException : Exception {
		public WahInvalidCommandException() : base("Code error: cannot create a null command or a command with the same" +
			"name as another in the same module") { }
	}

	/// <summary>
	/// Models an exception that occurs when a coder makes calls an invalid id for a visual  to display
	/// </summary>
	public class WahInvalidVisualException : Exception {
		public WahInvalidVisualException(int id) : base("Code error: cannot display visual,"
			+ " too high/low an identifier: " + id) { }
	}
	////////////////////////////////////////////////////////
	//USER FAULTS
	////////////////////////////////////////////////////////
	/// <summary>
	/// Models an exception caused by some sort of user error
	/// </summary>
	public abstract class WahUserException : Exception {
		public WahUserException(string message) : base(message) { }
	}

	/// <summary>
	/// Models an exception that occurs when the user tries to call a command that has the same name in two or more
	/// different modules.
	/// </summary>
	public class WahAmbiguousCommandException : WahUserException {
		public WahAmbiguousCommandException() : base("Error: Multiple modules have a command with the same"
			+" name, please specify which command you want run") { }
	}

	/// <summary>
	/// Models an exception that occurs when the user tries to call a command that doesn't exist
	/// </summary>
	public class WahNoCommandException : WahUserException {
		public WahNoCommandException(string cmd) : 
			base("Error: The given command \"" + cmd + "\" does not exist.") { }
	}

	/// <summary>
	/// Models an exception that occurs when the user tries to call a module that doesn't exist
	/// </summary>
	public class WahNoModuleException : WahUserException {
		public WahNoModuleException(string module) : 
			base("Error: The given module \"" + module + "\" does not exist or is not loaded.") { }
	}

	/// <summary>
	/// Models an exception that occurs when the user formats a command or argument or flag incorrectly
	/// </summary>
	public class WahBadFormatException : WahUserException {
		public WahBadFormatException(string str) :
			base("Error: The given string \"" + str + "\" is not a valid input.") { }
	}
}
