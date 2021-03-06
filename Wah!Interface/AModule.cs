﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wah_Interface {
	/// <summary>
	/// Models an implementation of a Module with some default commands, ability to load module from a common
	/// data folder
	/// </summary>
	public abstract class AModule : IModule {
		public string Name { get; private set; }
		public Color Color { get; private set; }
		public string Version { get; private set; }
		public IDisk Disk { get; private set; }

		private ISet<ICommand> commands;

		public AModule(string name, Color color, string version) {
			Name = name;
			Color = color;
			Version = version;
			Disk = CreateDisk();

			commands = new HashSet<ICommand>();
			//add module default commands:
			commands.Add(new UncheckedCommand("version", Cmd_Version));
			commands.Add(new UncheckedCommand("cmdlist", Cmd_Cmdlist));
			commands.Add(new Cmd_Help("help", this));

			//create the coder command list
			IEnumerable<ICommand> coderCmds = CreateCommands();
			//check all commands for correctness before adding
			foreach (ICommand command in coderCmds) {
				//can't add null commands or commands with names already inside the set
				if (command == null || commands.Contains(command) || !ValidCommand(command.Name)) {
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
		protected abstract ICommand[] CreateCommands();

		/// <summary>
		/// Creates the disk for this module.
		/// </summary>
		protected virtual IDisk CreateDisk() {
			return new Disk(this);
		}

		/// <summary>
		/// Is the given command a valid string for a command?
		/// </summary>
		private bool ValidCommand(string cmd) {
			//no spaces allowed
			return !cmd.Contains(' ');
		}

		public ICommand GetCommand(string cmd) {
			return commands.First(c => c.Name.Equals(cmd));
		}

		public bool HasCommand(string cmd) {
			return commands.Any(c => c.Name.Equals(cmd));
		}

		/// <summary>
		/// A command all modules have: Prints out this module's version
		/// </summary>
		private StringData Cmd_Version(IWah wah, IBundle bun) {
			return new StringData(Version, Color.Aqua);
		}

		/// <summary>
		/// A command all modules have: Prints all commands in the module
		/// </summary>
		private NoData Cmd_Cmdlist(IWah wah, IBundle bun) {
			wah.Putln("Showing commands for module " + Name, Color.Aqua);
			foreach (ICommand c in commands) {
				wah.Putln(c.Name, Color.LightYellow);
			}
			return new NoData();
		}

	}

	/// <summary>
	/// Models a command that displays help information
	/// </summary>
	internal class Cmd_Help : CheckedCommand<NoData> {
		private AModule mod;
		internal Cmd_Help(string name, AModule mod) : base(name,
			//Rules
			new TypeRule<StringData>('f')) {
			this.mod = mod;
		}

		public override NoData Apply(IWah wah, IBundle bun) {
			if (bun.HasArgument('f')) {
				string cmd = bun.Argument<StringData>('f').String;
				if (mod.HasCommand(cmd)) {
					//display help about command
					wah.Putln("Command " + cmd + " in module " + mod.Name, Color.GreenYellow);
					mod.Disk.LoadDisplayHelp(wah, cmd);
				}
				else {
					throw new WahInvalidCommandException();
				}
			}
			else {
				//display help about module
				wah.Put("Module ", Color.GreenYellow);
				wah.Putln(mod.Name, mod.Color);
				wah.Putln("Version " + mod.Version);
				wah.Putln("=============================");
				mod.Disk.LoadDisplayHelp(wah, mod.Name);
			}
			return new NoData();
		}
	}



}
