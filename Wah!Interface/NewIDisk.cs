using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wah_Interface {
	/// <summary>
	/// Represents an interface with a specific Module's file system, including operations like opening and saving files.
	/// </summary>
	public interface NewIDisk {
		/// <summary>
		/// Gets the operating directory of the Wah program
		/// </summary>
		string ProgramDirectory { get; }

		/// <summary>
		/// Loads an image with the given file name from the data folder of the module
		/// </summary>
		/// <param name="fileName">the name of just the image file (name + extension)</param>
		/// <returns>a bitmap of the file</returns>
		Bitmap LoadImage(string fileName);

		/// <summary>
		/// Loads an entire directory of images in topological order
		/// </summary>
		/// <param name="dirName"></param>
		/// <param name="ext">the extension of files to look for</param>
		/// <returns>the list of images loaded</returns>
		Bitmap[] LoadImageDir(string dirName, string ext);

		/// <summary>
		/// Loads a list of lines of a text file
		/// </summary>
		/// <param name="fileName">the name of the text file</param>
		/// <returns>the lines of text</returns>
		string[] LoadLines(string fileName);

		/// <summary>
		/// Ensures that the given path starting from the program directory exists
		/// </summary>
		/// <param name="path">the directory to check</param>
		/// <returns>true if path had to be created</returns>
		bool EnsurePath(string path);

		/// <summary>
		/// Ensures that the given file exists
		/// </summary>
		/// <param name="file">the file to check</param>
		/// <returns>true if file had to be created</returns>
		bool EnsureFile(string file);
	}

	/// <summary>
	/// Models the internal, default implementation of a Wah disk
	/// </summary>
	internal class NewWahDisk : NewIDisk {
		private const string BASE_DATA_NAME = "mod-data";
		private const string MOD_DATA_NAME = "data";
		private const string MOD_HELP_NAME = "help";

		private const string MAGIC_COLOR = "$!";
		private const string MAGIC_COLOR_START = ",";
		private const string MAGIC_COLOR_END = ".";
		private IModule mod;
		private string programLocation;
		private string HelpDir { get { return Path.Combine(DataDir, MOD_HELP_NAME);  } }
		private string DataDir { get { return Path.Combine(programLocation, BASE_DATA_NAME, MOD_DATA_NAME); } }
		public string ProgramDirectory { get { return programLocation; } }

		internal NewWahDisk(IModule mod) {
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
			if(!Directory.Exists(path)) {
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

		/// <summary>
		/// Attempts to load the specified help file located in the current module.
		/// If the file exists, it is parsed and displayed to the wah! core's display
		/// </summary>
		public void LoadDisplayHelp(IWah wah, string cmd) {
			//get the full path to the help file
			string path = Path.Combine(HelpDir, cmd + ".txt");
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

	}
}
