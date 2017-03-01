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
		private IProcessor wpro;
		private string basePath;
		internal WahDisk(IProcessor wpro) {
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

		public void LoadDisplayHelp(IDisplay wdisp, string helpName) {

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
