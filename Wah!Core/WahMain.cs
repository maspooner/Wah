using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Wah_Interface;

namespace Wah_Core {
	internal class WahMain : IWah {
		private const int TRIGGER_KEY = (int)Keys.OemPeriod;

		private NewWahProcessor WahPro { get; set; }
		private WahWindow WahWind { get; set; }

		public NewIApi Api { get { return WahPro; } }
		public NewIDisplay Display { get { return WahWind; } }

		private WahMain() {
			//Get the processor warmed up with the core wah!
			WahPro = new NewWahProcessor(this);
			//Create the ui with access to the processor
			WahWind = new WahWindow(WahPro);
		}

		static void Main(string[] args) {
			WahMain mainProgram = new WahMain();
			mainProgram.PreRunOperations();

			mainProgram.Log("Beginning UI loop");
			//Run the app, without showing the window
			Application.Run();

			mainProgram.Shutdown();
		}

		private void PreRunOperations() {
			Log("Application created");

			GlobalHotKeys.RegisterGlobalHotKey(TRIGGER_KEY, GlobalHotKeys.MOD_WIN, WahWind.Handle);

			WahPro.BeginListening();
		}

		internal void Shutdown() {
			//Post run
			WahWind.BeginInvoke(new Action(() => GlobalHotKeys.UnregisterGlobalHotKey(WahWind.Handle)));
			
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
