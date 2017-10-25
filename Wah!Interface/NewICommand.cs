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
		bool Validate(IBundle bun);
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
		IData Run(IWah wah, IBundle bun);

		/// <summary>
		/// Run this command on the given command bundle with access to the given wah
		/// to return a result of type D.
		/// </summary>
		/// <typeparam name="D">the type requested</typeparam>
		/// <param name="wah">the wah</param>
		/// <param name="bun">the command bundle</param>
		/// <returns>the result of the run of the command</returns>
		D Run<D>(IWah wah, IBundle bun) where D : IData;
	}

	/// <summary>
	/// Models an abstract command with a name and default implementation of some common methods.
	/// </summary>
	public abstract class ACommand : NewICommand {
		public string Name { get; private set; }

		internal ACommand(string name) {
			Name = name;
		}

		public abstract bool Validate(IBundle bun);
		public abstract string LastError();
		public abstract IData Run(IWah wah, IBundle bun);

		public D Run<D>(IWah wah, IBundle bun) where D : IData {
			IData run = Run(wah, bun);
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
		
		private Func<IWah, IBundle, IData> commandBody;

		public UncheckedCommand(string name, Func<IWah, IBundle, IData> commandBody) : base(name) {
			this.commandBody = commandBody;
		}

		public override bool Validate(IBundle bun) {
			return true;
		}

		public override string LastError() {
			//these commands don't throw validation errors
			return null;
		}

		public override IData Run(IWah wah, IBundle bun) {
			return commandBody(wah, bun);
		}

	}

	/// <summary>
	/// Models a command that checks for valid flags and arguments, and checks for a return value of a specific type.
	/// </summary>
	public abstract class CheckedCommand<D> : ACommand where D : IData {
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

		public override bool Validate(IBundle bun) {
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

		public override sealed IData Run(IWah wah, IBundle bun) {
			IData data = Apply(wah, bun);
			//ensure returned value is of the promised type
			if(data is D) {
				return data;
			}
			else {
				throw new WahWrongTypesException();
			}
		}

		public abstract D Apply(IWah wah, IBundle bun);
	}



	public class SysCommand_Test : CheckedCommand<StringData> {
		public SysCommand_Test() : base("test",
			//Rules
			new TypeRule<StringData>('s'),
			new TypeRule<StringData>('m'),
			new TypeRule<StringData>('e'),
			new RequireRule('m'),
			new CorequireRule('s', 'e')) {}

		public override StringData Apply(IWah wah, IBundle bun) {
			string m = bun.Argument<StringData>('m').Data;
			if (bun.HasArgument('s')) {
				string s = bun.Argument<StringData>('s').Data;
				string e = bun.Argument<StringData>('e').Data;
				return new StringData(s + m + e);
			}
			else {
				return new StringData(m);
			}
		}
	}

	public class SysCommand_BImage : CheckedCommand<ImageData> {
		public SysCommand_BImage() : base("bimage",
			//Rules
			new TypeRule<IntData>('w'),
			new TypeRule<IntData>('h'),
			new RequireRule('w'),
			new CorequireRule('w', 'h')) { }

		public override ImageData Apply(IWah wah, IBundle bun) {
			return new ImageData(new System.Drawing.Bitmap(bun.Argument<IntData>('w').Data, 
				bun.Argument<IntData>('h').Data));
		}
	}

	public class SysCommand_String : CheckedCommand<StringData> {
		public SysCommand_String() :base ("string", 
			//Rules
			new RequireRule('i')) {}

		public override StringData Apply(IWah wah, IBundle bun) {
			return new StringData(bun.Argument('i').ToString());
		}

	}
}
