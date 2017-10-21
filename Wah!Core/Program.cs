using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Wah_Interface;

namespace Wah_Core {
	internal class Program : ICore, IReCore {
		private const int TRIGGER_KEY = (int)Keys.OemPeriod;
		
		private WahProcessing wpro;
		private WahDisk wdisk;
		private WahSettings wsets;
		private WahWindow wwind;

		private Program() {
			//Get the processor warmed up with the core wah!
			wpro = new WahProcessing(this);
			wdisk = new WahDisk(wpro);
			wsets = new WahSettings(wpro, wdisk);
			//Create the ui with access to the processor
			wwind = new WahWindow(wpro);
		}

		static void Main2(string[] args) {
			Program mainProgram = new Program();
			mainProgram.PreRunOperations();
			mainProgram.Log("Beginning UI loop");
			//Run the app, without showing the window
			Application.Run();
			mainProgram.PostRunOperations();
		}

		private void PreRunOperations() {
			Log("Application created");

			Log("Beginning worker loop");
			wpro.BeginListening();

			GlobalHotKeys.RegisterGlobalHotKey(TRIGGER_KEY, GlobalHotKeys.MOD_WIN, wwind.Handle);
			

			//setup
			wpro.InitializeModules();
			wpro.InitializeMacros();
			//no mod-data
			if (wdisk.AttemptFirstTimeSetup()) {
				//TODO
				Putln("Welcome, first time user!");
			}
		}

		private void PostRunOperations() {
			//Post run
			GlobalHotKeys.UnregisterGlobalHotKey(wwind.Handle);
			//main window closed
			Log("Application shutdown");
			Application.Exit();
			Environment.Exit(0);
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
		//allowed
		public IDisplay Display { get { return wwind; } }
		public IDisk Disk { get { return wdisk; } }
		public ISettings Settings { get { return wsets; } }

		//restricted
		public IProcessor Processor { get { return wpro; } }
		public IReDisk ReDisk { get { return wdisk; } }
		public IReSettings ReSettings { get { return wsets; } }

		public void Putln(string txt) {
			Putln(txt, System.Drawing.Color.White);
		}
		public void Putln(string txt, System.Drawing.Color col) {
			Put(txt + "\n", col);
		}
		public void Put(string txt) {
			Put(txt, System.Drawing.Color.White);
		}
		public void Put(string txt, System.Drawing.Color col) {
			Display.Print(txt, col);
		}
		public void PutErr(string err) {
			Putln(err, System.Drawing.Color.Red);
		}

		
	}
}
