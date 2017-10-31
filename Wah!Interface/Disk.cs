using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wah_Interface {
	/// <summary>
	/// Models the internal, default implementation of a Wah disk
	/// </summary>
	internal class Disk : IDisk {
		private const string BASE_DATA_NAME = "mod-data";
		private const string MOD_DATA_NAME = "data";
		private const string MOD_HELP_NAME = "help";

		private const string MAGIC_COLOR = "$!";
		private const string MAGIC_COLOR_START = ",";
		private const string MAGIC_COLOR_END = ".";
		private IModule mod;
		private string programLocation;
		private string HelpDir { get { return Path.Combine(DataDir, MOD_HELP_NAME); } }
		private string DataDir { get { return Path.Combine(programLocation, BASE_DATA_NAME, MOD_DATA_NAME); } }
		public string ProgramDirectory { get { return programLocation; } }

		internal Disk(IModule mod) {
			this.mod = mod;
			programLocation =
				Path.GetDirectoryName(new Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).LocalPath);
			//make sure path to data exisits
			EnsurePath(DataDir);
		}

		public string[] LoadLines(string fileName) {
			string path = Path.Combine(DataDir, fileName);
			try {
				return File.ReadAllLines(path);
			}
			catch (Exception ex) {
				throw new WahIOLoadException("Could not read the lines of resource "
					+ fileName + " for module " + mod.Name + " Message: " + ex.Message);
			}
		}

		public Bitmap LoadImage(string fileName) {
			string path = Path.Combine(DataDir, fileName);
			if (File.Exists(path)) {
				try {
					return new Bitmap(path);
				}
				catch {
					throw new WahIOLoadException("Could not load bitmap from module " + mod.Name + ": " + fileName);
				}
			}
			else {
				throw new WahIOLoadException("Could not load bitmap from module " + mod.Name + "\'s data folder the resource " + fileName);
			}
		}

		public Bitmap[] LoadImageDir(string dirName, string ext) {
			List<Bitmap> bits = new List<Bitmap>();
			foreach (string file in Directory.EnumerateFiles(Path.Combine(DataDir, dirName))) {
				if (file.EndsWith(ext)) {
					bits.Add(LoadImage(file));
				}
			}
			return bits.ToArray();
		}

		public bool EnsurePath(string path) {
			if (!Directory.Exists(path)) {
				Directory.CreateDirectory(path);
				return true;
			}
			return false;
		}

		public bool EnsureFile(string file) {
			if (File.Exists(file)) {
				return false;
			}
			else {
				//create any necessary folders in the path
				EnsurePath(Path.GetDirectoryName(file));
				//create file and immediately close it
				File.Create(file).Dispose();
				return true;
			}
		}

		public void LoadDisplayHelp(IWah wah, string topic) {
			//get the full path to the help file
			string path = Path.Combine(HelpDir, topic + ".txt");
			if (File.Exists(path)) {
				string[] lines = File.ReadAllLines(path);
				//for each line
				foreach (string line in lines) {
					//if the line contains a color command
					if (line.Contains(MAGIC_COLOR)) {
						//format the color of the line, starting with gray
						FormatColor(wah, line, Color.LightGray);
						//end the line
						wah.Putln("");
					}
					else {
						//regular line, just print, save the effort
						wah.Putln(line, Color.LightGray);
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
		private string FormatColor(IWah wah, string line, Color col) {
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
							throw new WahHelpParseException("A color flag is impromperly formated.");
						}
						else {
							// format the color of what's after this tag with the new color, and return what's left to parse
							line = FormatColor(wah, afterMagic.Substring(iStart + 1), Color.FromName(colorName));
						}
					}
					else {
						throw new WahHelpParseException("A color flag is impromperly formated.");
					}
				}
				else {
					throw new WahHelpParseException("A color flag is impromperly formated.");
				}
			}
			//print the rest if there is any
			if (line.Length > 0) {
				wah.Put(line, col);
			}
			//no more left to parse
			return "";
		}

	}
}
