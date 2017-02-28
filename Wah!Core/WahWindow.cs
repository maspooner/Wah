using System;
using System.Drawing;
using System.Windows.Forms;
using Wah_Interface;

namespace Wah_Core {
	internal class WahWindow : Form, IDisplay {
		private IProcessor wpro;
		private TextBox inputBox;
		private RichTextBox outputBox;
		private PictureBox topPic;
		private PictureBox botPic;
		public WahWindow(IProcessor wpro) {
			this.wpro = wpro;
			inputBox = new TextBox();
			outputBox = new RichTextBox();
			topPic = new PictureBox();
			botPic = new PictureBox();
			SuspendLayout();
			//inputBox 
			inputBox.BackColor = Color.DarkSlateBlue;
			inputBox.BorderStyle = BorderStyle.FixedSingle;
			inputBox.Location = new Point(5, 305);
			inputBox.Size = new Size(400, 20);
			inputBox.TabIndex = 0;
			inputBox.ForeColor = Color.White;
			inputBox.Font = new Font(inputBox.Font, FontStyle.Bold);
			inputBox.AcceptsReturn = true;
			inputBox.PreviewKeyDown += TextBox_KeyDown;
			//outputBox
			outputBox.ReadOnly = true;
			outputBox.BackColor = Color.Black;
			outputBox.BorderStyle = BorderStyle.None;
			outputBox.Font = new Font(outputBox.Font.FontFamily, 11f, FontStyle.Regular);
			outputBox.ForeColor = Color.White;
			outputBox.Location = new Point(5, 5);
			outputBox.Size = new Size(400, 300);
			outputBox.HideSelection = false;
			outputBox.TabStop = false;
			// topPic
			topPic.Size = new Size(160, 160);
			topPic.Location = new Point(405, 5);
			topPic.BackColor = Color.FromArgb(25, 25, 60);
			topPic.SizeMode = PictureBoxSizeMode.Zoom;
			// botPic
			botPic.Size = new Size(160, 160);
			botPic.Location = new Point(405, 165);
			botPic.BackColor = Color.FromArgb(25, 25, 60);
			botPic.SizeMode = PictureBoxSizeMode.Zoom;
			// this
			this.AutoScaleDimensions = new SizeF(600f, 400f);
			this.AutoScaleMode = AutoScaleMode.None;
			this.BackColor = Color.DarkSlateGray;
			this.ClientSize = new Size(570, 330);
			this.ControlBox = false;
			this.FormBorderStyle = FormBorderStyle.None;
			this.Name = "Wah!";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			//this.StartPosition = FormStartPosition.;
			this.Text = "Wah!";
			this.TopMost = true;
			this.Activated += OnActivate;
			this.Deactivate += OnDeactivate;

			this.Controls.Add(this.inputBox);
			this.Controls.Add(this.outputBox);
			this.Controls.Add(this.topPic);
			this.Controls.Add(this.botPic);
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
		protected override void OnLoad(EventArgs e) {
			base.OnLoad(e);
			Location = new Point(Screen.PrimaryScreen.Bounds.Width - Bounds.Width, 0);
		}
		protected void OnActivate(object sender, EventArgs e) {
			Console.WriteLine("Activated");
			inputBox.Clear();
		}
		protected void OnDeactivate(object sender, EventArgs e) {
			Console.WriteLine("Deactivated");
			Hide();
		}
		protected override void WndProc(ref Message m) {
			if (m.Msg == GlobalHotKeys.WM_HOTKEY && (short)m.WParam == GlobalHotKeys.HotkeyID) {
				Show();
				Activate();
			}
			else {
				base.WndProc(ref m);
			}
		}
		private void TextBox_KeyDown(object sender, PreviewKeyDownEventArgs e) {
			if (e.Control && e.KeyCode == Keys.C) {
				//kill currently running command
				wpro.InterruptJob();
			}
		}

		public void Print(string txt, Color col) {
			CallOnUI(new Action(delegate {
				outputBox.SelectionColor = col;
				//outputBox.SelectionFont = new Font(outputBox.Font, FontStyle.Bold);
				outputBox.AppendText(txt + "\n");
			}));
		}

		public void ShowPersona(Bitmap persona) {
			CallOnUI(new Action(delegate {
				botPic.Image = persona;
			}));
		}

		public void ShowExtra(Bitmap extra) {
			CallOnUI(new Action(delegate {
				topPic.Image = extra;
			}));
		}

		public void HideWindow() {
			CallOnUI(new Action(Hide));
		}

		private void CallOnUI(Action act) {
			if (InvokeRequired) {
				Invoke(act);
			}
			else {
				act();
			}
		}
	}
}
