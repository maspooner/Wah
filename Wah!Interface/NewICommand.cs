using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wah_Interface {
	/// <summary>
	/// Models a function that takes in arguments from the user, and performs a specific action
	/// with those arguments to return a result.
	/// </summary>
	public interface NewICommand {
		string Name { get; }
		/// <summary>
		/// If this command is given the command bundle, will it be able to function properly?
		/// </summary>
		/// <param name="bun">the command bundle</param>
		bool Validate(NewIBundle bun);
		/// <summary>
		/// Returns the last error reported by any validation errors for this command
		/// </summary>
		string LastError();
		/// <summary>
		/// Run this command on the given command bundle with access to the given wah to return a result
		/// </summary>
		/// <param name="wah">the wah</param>
		/// <param name="bun">the command bundle</param>
		/// <returns>the result of the run of the command</returns>
		NewIData Run(NewIWah wah, NewIBundle bun);

		/// <summary>
		/// Run this command on the given command bundle with access to the given wah
		/// to return a result of type D.
		/// </summary>
		/// <typeparam name="D">the type requested</typeparam>
		/// <param name="wah">the wah</param>
		/// <param name="bun">the command bundle</param>
		/// <returns>the result of the run of the command</returns>
		D Run<D>(NewIWah wah, NewIBundle bun) where D : NewIData;
	}

	/// <summary>
	/// Models an abstract command with a name and default implementation of some common methods.
	/// </summary>
	public abstract class ACommand : NewICommand {
		public string Name { get; private set; }

		internal ACommand(string name) {
			Name = name;
		}

		public abstract bool Validate(NewIBundle bun);
		public abstract string LastError();
		public abstract NewIData Run(NewIWah wah, NewIBundle bun);

		public D Run<D>(NewIWah wah, NewIBundle bun) where D : NewIData {
			NewIData run = Run(wah, bun);
			if (run is D) {
				return (D)run;
			}
			else {
				//wrong type, throw exception
				throw new WahWrongTypesException();
			}
		}

		/// <summary>
		/// Commands are compared solely by their name
		/// </summary>
		public override bool Equals(object obj) {
			if(obj is ACommand) {
				return Name.Equals((obj as ACommand).Name);
			}
			else {
				return false;
			}
			
		}

		/// <summary>
		/// Commands are compared solely by their name
		/// </summary>
		public override int GetHashCode() {
			return Name.GetHashCode();
		}
	}

	/// <summary>
	/// Models a Command that does not validate any arguments or flags or return value.
	/// </summary>
	public sealed class UncheckedCommand : ACommand {
		
		private Func<NewIWah, NewIBundle, NewIData> commandBody;

		public UncheckedCommand(string name, Func<NewIWah, NewIBundle, NewIData> commandBody) : base(name) {
			this.commandBody = commandBody;
		}

		public override bool Validate(NewIBundle bun) {
			return true;
		}

		public override string LastError() {
			//these commands don't throw validation errors
			return null;
		}

		public override NewIData Run(NewIWah wah, NewIBundle bun) {
			return commandBody(wah, bun);
		}

	}

	/// <summary>
	/// Models a command that checks for valid flags and arguments, and checks for a return value of a specific type.
	/// </summary>
	public abstract class CheckedCommand<D> : ACommand where D : NewIData {
		private readonly IRule[] rules;
		private string lastError;

		/// <summary>
		/// Makes a checked command with the given name and rule set
		/// </summary>
		/// <param name="name">the name of this command</param>
		/// <param name="rules">the rules for this command to follow</param>
		public CheckedCommand(string name, params IRule[] rules) : base(name) {
			this.rules = rules;
			lastError = null;
		}

		public override bool Validate(NewIBundle bun) {
			foreach(IRule rule in rules) {
				if (!rule.Validate(bun)) {
					//record the rule violation
					lastError = rule.Violation();
					return false;
				}	
			}
			return true;
		}

		public override string LastError() {
			return lastError;
		}

		public override sealed NewIData Run(NewIWah wah, NewIBundle bun) {
			NewIData data = Apply(wah, bun);
			//ensure returned value is of the promised type
			if(data is D) {
				return data;
			}
			else {
				throw new WahWrongTypesException();
			}
		}

		public abstract D Apply(NewIWah wah, NewIBundle bun);
	}



	public class SysCommand_Test : CheckedCommand<NewStringData> {
		public SysCommand_Test() : base("test",
			//Rules
			new TypeRule<NewStringData>('s'),
			new TypeRule<NewStringData>('m'),
			new TypeRule<NewStringData>('e'),
			new RequireRule('m'),
			new CorequireRule('s', 'e')) {}

		public override NewStringData Apply(NewIWah wah, NewIBundle bun) {
			string m = bun.Argument<NewStringData>('m').Data;
			if (bun.HasArgument('s')) {
				string s = bun.Argument<NewStringData>('s').Data;
				string e = bun.Argument<NewStringData>('e').Data;
				return new NewStringData(s + m + e);
			}
			else {
				return new NewStringData(m);
			}
		}
	}

	public class SysCommand_BImage : CheckedCommand<NewImageData> {
		public SysCommand_BImage() : base("bimage",
			//Rules
			new TypeRule<NewIntData>('w'),
			new TypeRule<NewIntData>('h'),
			new RequireRule('w'),
			new CorequireRule('w', 'h')) { }

		public override NewImageData Apply(NewIWah wah, NewIBundle bun) {
			return new NewImageData(new System.Drawing.Bitmap(bun.Argument<NewIntData>('w').Data, 
				bun.Argument<NewIntData>('h').Data));
		}
	}

	public class SysCommand_String : CheckedCommand<NewStringData> {
		public SysCommand_String() :base ("string", 
			//Rules
			new RequireRule('i')) {}

		public override NewStringData Apply(NewIWah wah, NewIBundle bun) {
			return new NewStringData(bun.Argument('i').ToString());
		}

	}
}
