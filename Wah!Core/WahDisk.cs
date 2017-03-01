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

		private WahProcessing wpro;
		private string basePath;
		internal WahDisk(WahProcessing wpro) {
			this.wpro = wpro;
            basePath = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath) + "/";
		}


		private string ModData(string fileName) { return basePath + wpro.ActiveModule.Name + "/data/" + fileName; }
		private string ModHelp(string fileName) { return basePath + wpro.ActiveModule.Name + "/hlp/" + fileName; }
		private string ModSettings() { return basePath + wpro.ActiveModule.Name + "/settings.bin"; }

		/// <summary>
		/// Ensures a directory path exists, by creating it if it doesn't exist
		/// </summary>
		/// <returns>TRUE if had to create a new directory</returns>
		public bool EnsureDir(string dirName) {
			if(Directory.Exists(basePath + dirName)) {
				return false;
			}
			else {
				Directory.CreateDirectory(basePath + dirName);
				return true;
			}
		}

		public void LoadDisplayHelp(ICore wah, string helpName) {
			string path = ModHelp(helpName);
			if (File.Exists(path)) {
				string[] lines = File.ReadAllLines(path);
				foreach(string line in lines) {
					if (line.Contains(MAGIC_COLOR)) {
						int iMagic = line.IndexOf(MAGIC_COLOR);
						string beforeMagic = line.Substring(0, iMagic);
						if(beforeMagic.Length > 0) {
							wah.Put(beforeMagic);
						}
						string afterMagic = line.Substring(iMagic);
						if (afterMagic.Contains(MAGIC_COLOR_START) || afterMagic.Contains(MAGIC_COLOR_END)) {
							//TODO
						}
						System.Drawing.Color.FromName();
					}
					else {
						wah.Putln(line);
					}
				}
			}
			else {
				wah.PutErr("No help document found.");
			}
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
