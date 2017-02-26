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
		private TextBox inputBox;
		private RichTextBox outputBox;
		public WahWindow(IProcessor wpro) {
			this.wpro = wpro;
			inputBox = new TextBox();
			outputBox = new RichTextBox();
			SuspendLayout();
			//inputBox 
			inputBox.BackColor = SystemColors.ControlDark;
			inputBox.BorderStyle = BorderStyle.FixedSingle;
			inputBox.Location = new Point(380, 299);
			inputBox.Size = new Size(200, 20);
			inputBox.TabIndex = 0;
			inputBox.ForeColor = Color.White;
			inputBox.Font = new Font(inputBox.Font, FontStyle.Bold);
			inputBox.AcceptsReturn = true;
			inputBox.PreviewKeyDown += TextBox_KeyDown;
			//outputBox
			outputBox.ReadOnly = true;
			outputBox.BackColor = Color.Black;
			outputBox.ForeColor = Color.White;
			outputBox.Size = new Size(400, 300);
			// this
			this.AutoScaleDimensions = new SizeF(6F, 13F);
			this.AutoScaleMode = AutoScaleMode.Font;
			this.BackColor = SystemColors.ControlDarkDark;
			this.ClientSize = new Size(600, 300);
			this.ControlBox = false;
			this.FormBorderStyle = FormBorderStyle.None;
			this.Name = "Wah!";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			//this.StartPosition = FormStartPosition.;
			this.Text = "Wah!";
			this.TopMost = true;
			//this.Activated += MainForm_Activate;
			//this.Deactivate += MainForm_Deactivate;

			this.Controls.Add(this.inputBox);
			this.Controls.Add(this.outputBox);
			this.ResumeLayout(false);
			this.PerformLayout();
			ResumeLayout();
		}

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData) {
			if (keyData == Keys.Enter) {
				Console.WriteLine("Enter");
				//wpro.Prepare("wah? fuko.partyhat true");
				//wpro.Prepare("fuko get gmail");
				string input = inputBox.Text;
				inputBox.Clear();
				wpro.Prepare(input);
				return true;
			}
			return base.ProcessCmdKey(ref msg, keyData);
		}
		private void TextBox_KeyDown(object sender, PreviewKeyDownEventArgs e) {
			if (e.Control && e.KeyCode == Keys.C) {
				//kill currently running command
				wpro.InterruptJob();
			}
		}

		public void Print(string txt, Color col) {
			if (outputBox.InvokeRequired) {
				outputBox.Invoke(new MethodInvoker(delegate {
					outputBox.SelectionColor = col;
					outputBox.AppendText(txt + "\n");
				}));
			}
		}

		public void ShowPersona(Bitmap persona) {
			throw new NotImplementedException();
		}

		public void ShowTitle(string title) {
			throw new NotImplementedException();
		}

	}
}
