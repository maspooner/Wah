using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wah_Interface {
	public interface IVisual {
		Point Location { get; }
		Bitmap Image { get; }
		void Tick();
	}
	public class EmptyImage : IVisual {
		public Bitmap Image { get { return new Bitmap(1, 1); } }
		public Point Location { get; private set; }
		public EmptyImage(Point location) {
			Location = location;
		}
		public void Tick() { }
	}
	public class SimpleImage : IVisual {
		public Point Location { get; private set; }
		public Bitmap Image { get; private set; }
		public SimpleImage(Bitmap image){
			Image = image;
			Location = new Point(0, 0);
		}
		public void Tick() { }
	}
	public class LayeredImage : IVisual {
		private IVisual[] images;
		private Bitmap imBase;
		public Point Location { get; private set; }
		public Bitmap Image {
			get { return LayerImages(imBase, images); }
		}
        public LayeredImage(Bitmap imBase, params IVisual[] images) {
			this.imBase = imBase;
			this.images = images;
			Location = new Point(0, 0);
		}
		private Bitmap LayerImages(Bitmap imBase, params IVisual[] images) {
			using(Graphics g = Graphics.FromImage(imBase)) {
				foreach (IVisual im in images) {
					g.DrawImage(im.Image, im.Location.X, im.Location.Y);
				}
			}
			return imBase;
		}
		public void Tick() {
			foreach(IVisual im in images) {
				im.Tick();
			}
		}
	}
	public class ChangeAnimation : IVisual {
		private Bitmap[] images;
		private int i;
		public bool Repeat { get; private set; }
		public Point Location { get; private set; }
		public Bitmap Image {
			get { return images[i]; }
		}

		public ChangeAnimation(Bitmap[] images, Point location, bool repeat) {
			if(images.Length < 1) {
				throw new ArgumentException("images must contain at least 1 image");
			}
			this.images = images;
			Location = location;
			Repeat = repeat;
			i = 0;
		}

		public void Tick() {
			if (i + 1 < images.Length) {
				i++;
			}
			else if (Repeat) {
				i = 0;
			}
		}
	}
}
