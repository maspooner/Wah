using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wah_Interface {
	/// <summary>
	/// Models a bundle of flags and arguments that can be passed to a command to give it more information about what
	/// operation the user wants to accomplish.
	/// </summary>
	public interface IBundle {
		/// <summary>
		/// Does this bundle have a flag for the given id?
		/// </summary>
		/// <param name="id">the id of the flag</param>
		bool HasFlag(string id);

		/// <summary>
		/// Does this bundle have an argument for the given id?
		/// </summary>
		/// <param name="id">the argument id</param>
		bool HasArgument(char id);

		/// <summary>
		/// Returns the raw IData for the argument with the given id.
		/// Useful for visitors for accepting multiple kinds of IData.
		/// </summary>
		/// <param name="id">the id of the argument</param>
		IData Argument(char id);

		/// <summary>
		/// Returns the IData of the given type for the argument with the given id.
		/// </summary>
		/// <typeparam name="D">the type of IData</typeparam>
		/// <param name="id">the id of the argument to return</param>
	    D Argument<D>(char id) where D : IData;

		/// <summary>
		/// Does this bundle have no arguments or flags?
		/// </summary>
		/// <returns></returns>
		bool Empty();
	}


	/// <summary>
	/// Models a default implementation of a bundle of flags and arguments.
	/// </summary>
	public class CommandBundle : IBundle {
		private ISet<string> flags;
		private IDictionary<char, IData> arguments;

		public CommandBundle(ISet<string> flags, IDictionary<char, IData> arguments) {
			this.flags = flags;
			this.arguments = arguments;
		}

		public bool HasFlag(string id) {
			return flags.Contains(id);
		}

		public bool HasArgument(char id) {
			return arguments.ContainsKey(id);
		}

		public IData Argument(char id) {
			if(HasArgument(id)) {
				return arguments[id];
			}
			else {
				throw new WahArgumentMissingException();
			}
		}

		public D Argument<D>(char id) where D : IData {
			IData data = Argument(id);
			if(data is D) {
				return (D)data;
			}
			else {
				throw new WahWrongTypesException(typeof(D), data);
			}
		}

		public bool Empty() {
			return arguments.Count == 0 && flags.Count == 0;
		}
		
	}
}
