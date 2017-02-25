using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Wah_Interface;

namespace Wah_Core {
	internal class WahDisk : IDisk {
		private string basePath;
		internal WahDisk() {
			basePath = System.IO.Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
		}

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
