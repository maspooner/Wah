using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Wah_Interface;
using System.IO;
using System.Drawing;

namespace Wah_Core {
	internal class WahDisk : IDisk {
		private const string MAGIC_COLOR = "$!";
		private const string MAGIC_COLOR_START = ",";
		private const string MAGIC_COLOR_END = ".";
		private static readonly Color HELP_COLOR = Color.LightGray;
		private const string SETTINGS_FILE = "settings.txt";

		private WahProcessing wpro;
		private string basePath;
		internal WahDisk(WahProcessing wpro) {
			this.wpro = wpro;
			basePath = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
		}
		//relative
		private string RelModuleDir(OldAModule mod) { return Path.Combine("mod-data", mod.Name); }
		private string RelDataDir(OldAModule mod) { return Path.Combine(RelModuleDir(mod), "data"); }
		//absolute
		private string FullModuleDir(OldAModule mod) { return Path.Combine(basePath, RelModuleDir(mod)); }
		private string FullDataDir(OldAModule mod) { return Path.Combine(basePath, RelDataDir(mod)); }
		private string FullHelpDir(OldAModule mod) { return Path.Combine(basePath, 
			Path.Combine(RelModuleDir(mod), "help")); }

		/// <summary>
		/// Attempts to load the specified help file located in the current module.
		/// If the file exists, it is parsed and displayed to the wah! core's display
		/// </summary>
		public void LoadDisplayHelp(OldICore wah, OldAModule mod, string helpName) {
			//get the full path to the help file
			string path = Path.Combine(FullHelpDir(mod), helpName + ".txt");
			if (File.Exists(path)) {
				string[] lines = File.ReadAllLines(path);
				//for each line
				foreach (string line in lines) {
					//if the line contains a color command
					if (line.Contains(MAGIC_COLOR)) {
						//format the color of the line, starting with gray
						FormatColor(wah, line, HELP_COLOR);
						//end the line
						wah.Putln("");
					}
					else {
						//regular line, just print, save the effort
						wah.Putln(line, HELP_COLOR);
					}
				}
			}
			else {
				wah.PutErr("No help document found.");
			}
		}
		/// <summary>
		/// Parsed the line for any color tags, printing the line bit by bit in the right color
		/// Returns what's left to parse
		/// </summary>
		/// <param name="line">what's left of the line to print</param>
		/// <param name="col">the color for the line</param>
		/// <returns>what's left of the line to parse</returns>
		private string FormatColor(OldICore wah, string line, System.Drawing.Color col) {
			//while still color to parse
			while (line.Contains(MAGIC_COLOR)) {
				//index of the color command
				int iMagic = line.IndexOf(MAGIC_COLOR);
				//get the text before the magic
				string beforeMagic = line.Substring(0, iMagic);
				//put the text before in the previous color
				if (beforeMagic.Length > 0) {
					wah.Put(beforeMagic, col);
				}
				//get what's after the magic
				string afterMagic = line.Substring(iMagic + MAGIC_COLOR.Length);
				//has stuff after the magic
				if (afterMagic.Length > 0) {
					//end tag
					if (afterMagic.StartsWith(MAGIC_COLOR_END)) {
						// pass the rest to be parsed with the previous color
						return afterMagic.Substring(MAGIC_COLOR_END.Length);
					}
					//start tag
					else if (afterMagic.Contains(MAGIC_COLOR_START)) {
						//the index of the starting tag
						int iStart = afterMagic.IndexOf(MAGIC_COLOR_START);
						//read the name of the desired color
						string colorName = afterMagic.Substring(0, iStart);
						//illformated color tag
						if (iStart + 1 >= afterMagic.Length) {
							throw new HelpParseException("A color flag is impromperly formated.");
						}
						else {
							// format the color of what's after this tag with the new color, and return what's left to parse
							line = FormatColor(wah, afterMagic.Substring(iStart + 1), System.Drawing.Color.FromName(colorName));
						}
					}
					else {
						throw new HelpParseException("A color flag is impromperly formated.");
					}
				}
				else {
					throw new HelpParseException("A color flag is impromperly formated.");
				}
			}
			//print the rest if there is any
			if (line.Length > 0) {
				wah.Put(line, col);
			}
			//no more left to parse
			return "";
		}

		/// <summary>
		/// Load lines of a text file from the given module's data folder
		/// </summary>
		/// <param name="mod">the module folder to look in</param>
		/// <param name="fileName">just the file name of the file in the module's data folder</param>
		public string[] LoadLines(OldAModule mod, string fileName) {
			string path = Path.Combine(FullDataDir(mod), fileName);
			try {
				return File.ReadAllLines(path);
			}
			catch (Exception ex) {
				throw new IOLoadException("Could not read the lines of resource " 
					+ fileName + " for module " + mod.Name, ex);
			}
		}
		private void EnsurePath(string s) {
			throw new NotImplementedException();
		}
		public string[] LoadSettings(OldAModule mod) {
			string settingsPath = Path.Combine(FullDataDir(mod), SETTINGS_FILE);
			//ensure the settings file
			EnsurePath(settingsPath);
			//load the settings
			return LoadLines(mod, SETTINGS_FILE);
		}

		public IEnumerable<Assembly> LoadAllAssemblies() {
			string[] dlls = Directory.GetFiles(basePath, "*.dll", SearchOption.TopDirectoryOnly);
			return dlls.Select(dll => Assembly.LoadFile(dll));
		}
	}
}
