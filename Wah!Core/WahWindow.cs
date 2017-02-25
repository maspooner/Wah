using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Wah_Interface;

namespace Wah_Core {
	internal class WahWindow : Form, IDisplay {
		private IProcessor wpro;
		public WahWindow(IProcessor wpro) {
			this.wpro = wpro;
			SuspendLayout();

			ResumeLayout();
		}

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData) {
			if (keyData == Keys.Enter) {
				Console.WriteLine("Enter");
				//wpro.Prepare("wah? fuko.partyhat true");
				//wpro.Prepare("fuko get gmail");
				wpro.Prepare("fuko namae");
				return true;
			}
			else if (keyData == Keys.Back) {
				Console.WriteLine("Interrupt");
				wpro.InterruptJob();
			}
			return base.ProcessCmdKey(ref msg, keyData);
		}

		public void Print(string txt) {
			throw new NotImplementedException();
		}

		public void ShowPersona(Bitmap persona) {
			throw new NotImplementedException();
		}

		public void ShowTitle(string title) {
			throw new NotImplementedException();
		}

	}
}
