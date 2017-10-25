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

	internal class NewWahDisk : NewIDisk {
		private const string BASE_DATA_NAME = "mod-data";
		private const string MOD_DATA_NAME = "data";
		private IModule mod;
		private string programLocation;
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
				throw new IOLoadException("Could not read the lines of resource "
					+ fileName + " for module " + mod.Name, ex);
			}
		}

		public Bitmap LoadImage(string fileName) {
			string path = Path.Combine(DataDir, fileName);
			if (File.Exists(path)) {
				try {
					return new Bitmap(path);
				}
				catch {
					throw new IOLoadException("Could not load bitmap from module " + mod.Name + ": " + fileName);
				}
			}
			else {
				throw new IOLoadException("Could not load bitmap from module " + mod.Name + "\'s data folder the resource " + fileName);
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

	}
}
