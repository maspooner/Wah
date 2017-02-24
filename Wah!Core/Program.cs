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

		private WahSettings wsets;
		private WahProcessing wpro;
		
		private WahWindow wwind;
		

		private Program() {
			//wsets = new WahSettings();
			//Get the processor warmed up with the core wah!
			wpro = new WahProcessing(this);
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
			
			Console.WriteLine("[" + DateTime.Now.ToLongTimeString() + "] : " + line);
		}
		public IApi Api { get { return wpro; } }
		public IAudio Audio {
			get {
				throw new NotImplementedException();
			}
		}

		public IDisplay Display { get { return wwind; } }
		

		public IDisk Disk {
			get {
				throw new NotImplementedException();
			}
		}

		public ISettings Settings {
			get {
				throw new NotImplementedException();
			}
		}
	}
}
