using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Wah_Interface;

namespace Wah_Core {
	internal class Program : ICore {

		
		private WahProcessing wpro;
		private WahDisk wdisk;
		private WahSettings wsets;
		private WahWindow wwind;
		

		private Program() {
			wdisk = new WahDisk();
			//Get the processor warmed up with the core wah!
			wpro = new WahProcessing(this);
			
			wsets = new WahSettings(wpro, wdisk);
			//Create the ui with access to the processor
			wwind = new WahWindow(wpro);
		}

		static void Main(string[] args) {
			Program mainProgram = new Program();
			mainProgram.Log("Application created");

			mainProgram.Log("Beginning worker loop");
			mainProgram.wpro.BeginListening();

			mainProgram.Log("Beginning UI loop");
			mainProgram.BeginUILoop();
			//main window closed
			mainProgram.Log("Application shutdown");
			Application.Exit();
			Environment.Exit(0);
		}

		private void BeginUILoop() {
			//Run the app
			Application.Run(wwind);
		}

		public void Log(string line) {
			string time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff",
											System.Globalization.CultureInfo.InvariantCulture);
			Console.WriteLine("[" + time + "] : " + line);
		}
		public IApi Api { get { return wpro; } }
		public IAudio Audio {
			get {
				throw new NotImplementedException();
			}
		}
		public IDisplay Display { get { return wwind; } }
		public IDisk Disk { get { return wdisk; } }
		public ISettings Settings { get { return wsets; } }
	}
}
