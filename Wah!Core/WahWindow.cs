using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Wah_Interface;

namespace Wah_Core {
	internal class WahWindow : Form, IDisplay {
		private const int ANIMATION_CLOCK = 100;
		private IProcessor wpro;
		private CancellationTokenSource animeToken;
		private Task animationTask;

		private TextBox inputBox;
		private RichTextBox outputBox;
		private VisualBox topPic;
		private VisualBox botPic;
		private Label inputLabel;
		public WahWindow(IProcessor wpro) {
			this.wpro = wpro;
			animeToken = new CancellationTokenSource();
			animationTask = new Task(new Action(AnimationLoop), animeToken.Token, TaskCreationOptions.LongRunning);
			inputBox = new TextBox();
			outputBox = new RichTextBox();
			topPic = new VisualBox();
			botPic = new VisualBox();
			inputLabel = new Label();
			SuspendLayout();
			//inputBox 
			inputBox.BackColor = Color.DarkSlateBlue;
			inputBox.BorderStyle = BorderStyle.FixedSingle;
			inputBox.Location = new Point(25, 305);
			inputBox.Size = new Size(380, 20);
			inputBox.TabIndex = 0;
			inputBox.ForeColor = Color.White;
			inputBox.Font = new Font(inputBox.Font, FontStyle.Bold);
			inputBox.AcceptsReturn = true;
			inputBox.PreviewKeyDown += TextBox_KeyDown;
			//outputBox
			outputBox.ReadOnly = true;
			outputBox.BackColor = Color.Black;
			outputBox.BorderStyle = BorderStyle.None;
			outputBox.Font = new Font(outputBox.Font.FontFamily, 10f, FontStyle.Regular);
			outputBox.ForeColor = Color.White;
			outputBox.Location = new Point(5, 5);
			outputBox.Size = new Size(400, 300);
			outputBox.HideSelection = false;
			outputBox.TabStop = false;
			// topPic
			topPic.Size = new Size(160, 160);
			topPic.Location = new Point(405, 5);
			topPic.BackColor = Color.FromArgb(25, 25, 60);
			// botPic
			botPic.Size = new Size(160, 160);
			botPic.Location = new Point(405, 165);
			botPic.BackColor = Color.FromArgb(25, 25, 60);
			//inputLabel
			inputLabel.Size = new Size(35, 20);
			inputLabel.Location = new Point(10, 305);
			inputLabel.Text = "> ";
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
			this.Text = "Wah!";
			this.TopMost = true;
			this.Activated += OnActivate;
			this.Deactivate += OnDeactivate;

			this.Controls.Add(this.inputBox);
			this.Controls.Add(this.outputBox);
			this.Controls.Add(this.topPic);
			this.Controls.Add(this.botPic);
			this.Controls.Add(this.inputLabel);
			this.ResumeLayout(false);
			this.PerformLayout();
			ResumeLayout();

			//force create the handle
			var h = Handle;
			animationTask.Start();
		}
		private void AnimationLoop() {
			for (;;) {
				if (animeToken.Token.WaitHandle.WaitOne(ANIMATION_CLOCK)) {
					break;
				}
				CallOnUI(topPic.Tick);
				CallOnUI(botPic.Tick);
			}
		}

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData) {
			if (keyData == Keys.Enter) {
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
				outputBox.AppendText(txt);
			}));
		}

		public void ShowPersona(IVisual persona) {
			CallOnUI(new Action(delegate {
				botPic.UpdateVisual(persona);
			}));
		}

		public void ShowExtra(IVisual extra) {
			CallOnUI(new Action(delegate {
				topPic.UpdateVisual(extra);
			}));
		}

		public void HideWindow() {
			CallOnUI(new Action(Hide));
		}
		public void ClearWindow() {
			CallOnUI(new Action(outputBox.Clear));
		}

		private void CallOnUI(Action act) {
			////no window handle
			//if (!IsHandleCreated) {
			//	//force the creation of the handle by calling its accessor
			//	var handle = Handle;
			//}
			//invoke the action
			BeginInvoke(act);
		}
	}
	internal class VisualBox : Panel {
		private volatile IVisual visual;
		public VisualBox() : this(new EmptyImage(new Point(0, 0))) { }
		public VisualBox(IVisual visual) {
			this.visual = visual;
			DoubleBuffered = true;
		}

		protected override void OnPaint(PaintEventArgs e) {
			base.OnPaint(e);
			e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBilinear;
			e.Graphics.DrawImage(visual.Image, visual.Location);
        }

		public void UpdateVisual(IVisual vis) {
			this.visual = vis;
			Refresh();
		}

		public void Tick() {
			this.visual.Tick();
			Refresh();
		}

	}
}
