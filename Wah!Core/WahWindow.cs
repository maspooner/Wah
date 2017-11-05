using System;
using System.Collections.Generic;
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
		private IList<string> history;
		private int iHistory;

		private TextBox inputBox;
		private RichTextBox outputBox;
		private VisualBox topPic;
		private VisualBox botPic;
		private Label inputLabel;
		public WahWindow(IProcessor wpro) {
			this.wpro = wpro;
			history = new List<string>();
			iHistory = -1; // -1 denotes selecting nothing from the history
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
			inputBox.PreviewKeyDown += InputBox_PreviewKeyDown;
			inputBox.KeyDown += InputBox_KeyDown;
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
			IntPtr h = Handle;
			animationTask.Start();
		}
		private void AnimationLoop() {
			while (true) {
				if (animeToken.Token.WaitHandle.WaitOne(ANIMATION_CLOCK)) {
					break;
				}
				CallOnUI(topPic.Tick);
				CallOnUI(botPic.Tick);
			}
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
		private void InputBox_KeyDown(object sender, KeyEventArgs e) {
			//ignore the default behavior of up/down keys causing cursor movement
			if (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down) {
				e.Handled = true;
			}
			else {
				base.OnKeyDown(e);
			}

		}
		private void InputBox_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e) {
			if (e.Control && e.KeyCode == Keys.C) {
				//kill currently running command
				wpro.InterruptJob();
			}
			//move to previous item in history
			else if (e.KeyCode == Keys.Up) {
				if (iHistory + 1 < history.Count) {
					iHistory++;
					ChangeInputText(history[iHistory]);
				}
			}
			//move to next item in history
			else if (e.KeyCode == Keys.Down) {
				if (iHistory - 1 >= -1) {
					iHistory--;
					ChangeInputText(iHistory == -1 ? "" : history[iHistory]);
				}
			}
			//execute command
			else if (e.KeyCode == Keys.Enter) {
				//wpro.Prepare("wah? fuko.partyhat true");
				//wpro.Prepare("fuko get gmail");
				string input = inputBox.Text;
				if (input.Trim().Length > 0) {
					//add it to the front of history
					history.Insert(0, input);
					//select none from history
					iHistory = -1;
					//clear the input box
					inputBox.Clear();
					//prepare the command for execution
					wpro.Prepare(input);
				}
			}
		}
		private void ChangeInputText(string text) {
			inputBox.Text = text;
			inputBox.SelectionStart = inputBox.TextLength;
			inputBox.SelectionLength = 0;
		}

		public void Print(string txt, Color col) {
			CallOnUI(new Action(delegate {
				outputBox.SelectionColor = col;
				//outputBox.SelectionFont = new Font(outputBox.Font, FontStyle.Bold);
				outputBox.AppendText(txt);
			}));
		}

		public void ShowVisual(IVisual visual, int id) {
			if(id > 0 || id > 1) {
				throw new WahInvalidVisualException(id);
			}
			CallOnUI(new Action(delegate {
				if(id == 0) {
					topPic.UpdateVisual(visual);
				}
				else {
					botPic.UpdateVisual(visual);
				}
			}));
		}

		public void ClearVisuals() {
			CallOnUI(new Action(delegate {
				EmptyImage ei = new EmptyImage(new Point(0, 0));
				topPic.UpdateVisual(ei);
				botPic.UpdateVisual(ei);
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
