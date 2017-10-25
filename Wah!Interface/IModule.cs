using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wah_Interface {
	/// <summary>
	/// Models a collection of commands that share a similar function and can be easily added or removed from the main
	/// Wah program.
	/// </summary>
	public interface IModule {
		string Name { get; }

		/// <summary>
		/// Does this module contain a command with the given name?
		/// </summary>
		/// <param name="cmd">the name of the command</param>
		bool HasCommand(string cmd);

		/// <summary>
		/// Retrieves the command with the given name that is in this module.
		/// </summary>
		/// <param name="cmd">the string command name</param>
		/// <returns>the command</returns>
		NewICommand GetCommand(string cmd);
		
	}

	/// <summary>
	/// Models an implementation of a Module with some default commands, ability to load module from a common
	/// data folder, 
	/// </summary>
	public abstract class AModule : IModule {
		public string Name { get; private set; }
		public string Version { get; private set; }
		protected NewIDisk Disk { get; private set; }

		private ISet<NewICommand> commands;
		

		public AModule(string name, string version) {
			Name = name;
			Version = version;
			Disk = new NewWahDisk(this);
			
			commands = new HashSet<NewICommand>();
			//add module default commands:
			commands.Add(new UncheckedCommand("version", Cmd_Version));

			//create the coder command list
			IEnumerable<NewICommand> coderCmds = CreateCommands();
			//check all commands for correctness before adding
			foreach (NewICommand command in coderCmds) {
				//can't add null commands or commands with names already inside the set
				if(command == null || commands.Contains(command)) {
					throw new WahInvalidCommandException();
				}
				else {
					commands.Add(command);
				}
			}
			
		}

		/// <summary>
		/// Creates the commands that are a part of this module
		/// </summary>
		protected abstract NewICommand[] CreateCommands();

		public NewICommand GetCommand(string cmd) {
			return commands.First(c => c.Name.Equals(cmd));
		}

		public bool HasCommand(string cmd) {
			return commands.Any(c => c.Name.Equals(cmd));
		}

		/// <summary>
		/// A command all modules have: Prints out this module's version
		/// </summary>
		private StringData Cmd_Version(IWah wah, IBundle bun) {
			return new StringData(Version, System.Drawing.Color.Aqua);
		}

	}
}
