using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Wah_Interface;
using System.IO;

namespace Wah_Core {
	internal class WahDisk : IDisk {
		private const string MAGIC_COLOR = "$!";
		private const string MAGIC_COLOR_START = ",";
		private const string MAGIC_COLOR_END = ".";
		private static readonly System.Drawing.Color HELP_COLOR = System.Drawing.Color.LightGray;

		private WahProcessing wpro;
		private string basePath;
		internal WahDisk(WahProcessing wpro) {
			this.wpro = wpro;
            basePath = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
		}

		private string ModDir(string module) { return Path.Combine(basePath, Path.Combine("mod-data", module)); }
		private string ModData(string module, string fileName) { return Path.Combine(ModDir(module), Path.Combine("data", fileName)); }
		private string ModHelp(string module, string fileName) { return Path.Combine(ModDir(module), Path.Combine("help", fileName)); }
		private string ModSettings(string module) { return Path.Combine(ModDir(module), "settings.bin"); }

		/// <summary>
		/// Ensures a directory path exists, by creating it if it doesn't exist
		/// </summary>
		/// <returns>TRUE if had to create a new directory</returns>
		public bool EnsureDir(string dirName) {
			if(Directory.Exists(Path.Combine(basePath, dirName))) {
				return false;
			}
			else {
				Directory.CreateDirectory(Path.Combine(basePath, dirName));
				return true;
			}
		}

		/// <summary>
		/// Attempts to load the specified help file located in the current module.
		/// If the file exists, it is parsed and displayed to the wah! core's display
		/// </summary>
		public void LoadDisplayHelp(ICore wah, AModule mod, string helpName) {
			//get the full path to the help file
			string path = ModHelp(mod.Name, helpName + ".txt");
			if (File.Exists(path)) {
				string[] lines = File.ReadAllLines(path);
				//for each line
				foreach(string line in lines) {
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
		private string FormatColor(ICore wah, string line, System.Drawing.Color col) {
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
					else if(afterMagic.Contains(MAGIC_COLOR_START)) {
						//the index of the starting tag
						int iStart = afterMagic.IndexOf(MAGIC_COLOR_START);
						//read the name of the desired color
						string colorName = afterMagic.Substring(0, iStart);
						//illformated color tag
						if(iStart + 1 >= afterMagic.Length) {
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
			if(line.Length > 0) {
				wah.Put(line, col);
			}
			//no more left to parse
			return "";
		}

		public bool AttemptFirstTimeSetup() {
			if (EnsureDir("mod-data/" + wpro.Name)) {

				return true;
			}
			return false;
		}

		//public byte[] LoadData(string fileName) {
		//	string path = ModDataPath() + fileName;
		//}

		public byte[] Load(string fileName) {
			throw new NotImplementedException();
		}

		public Assembly LoadAssembly(string name) {
			Console.WriteLine(basePath + "/" + name + ".dll");
			return Assembly.LoadFile(basePath + "/" + name + ".dll");
		}

		public void RunShutdownOperations() {
			throw new NotImplementedException();
		}

		public void Save(string fileName, byte[] data) {
			throw new NotImplementedException();
		}
	}
}
