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

		

		/// <summary>
		/// Load lines of a text file from the given module's data folder
		/// </summary>
		/// <param name="mod">the module folder to look in</param>
		/// <param name="fileName">just the file name of the file in the module's data folder</param>
		public string[] LoadLines(string fileName) {
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
		public string[] LoadSettings() {
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
