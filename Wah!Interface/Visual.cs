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
	public abstract class AStatic : IVisual {
		public Point Location { get; private set; }
		public abstract Bitmap Image { get; }
		//public abstract Bitmap Image { get; }
		internal AStatic(Point location) {
			Location = location;
		}
		public virtual void Tick() { }
	}
	public abstract class AMove : IVisual {
		public Point Location { get; private set; }
		public bool Repeat { get; private set; }
		public abstract Bitmap Image { get; }
		internal AMove(Point location, bool repeat) {
			Location = location;
			Repeat = repeat;
		}
		public abstract void Tick();
	}
	public class EmptyImage : AStatic {
		public override Bitmap Image { get { return new Bitmap(1, 1); } }
		public EmptyImage(Point location) : base(location) { }
	}
	public class SimpleImage : AStatic {
		private Bitmap image;
		public override Bitmap Image { get { return image; } }
		public SimpleImage(Bitmap image) : base(new Point(0, 0)){
			this.image = image;
		}
	}
	public class LayeredImage : AStatic {
		private IVisual[] images;
		private Bitmap imBase;
		public override Bitmap Image {
			get { return LayerImages(imBase, images); }
		}
        public LayeredImage(Bitmap imBase, params IVisual[] images) : base(new Point(0, 0)) {
			this.imBase = imBase;
			this.images = images;
		}
		private Bitmap LayerImages(Bitmap imBase, params IVisual[] images) {
			using(Graphics g = Graphics.FromImage(imBase)) {
				foreach (IVisual im in images) {
					g.DrawImage(im.Image, im.Location.X, im.Location.Y);
				}
			}
			return imBase;
		}
		public override void Tick() {
			foreach(IVisual im in images) {
				im.Tick();
			}
		}
	}
	public class ChangeAnimation : AMove {
		private Bitmap[] images;
		private int i;
		public override Bitmap Image {
			get { return images[i]; }
		}

		public ChangeAnimation(Bitmap[] images, Point location, bool repeat) : base(location, repeat) {
			if(images.Length < 1) {
				throw new ArgumentException("images must contain at least 1 image");
			}
			this.images = images;
			i = 0;
		}

		public override void Tick() {
			if (i + 1 < images.Length) {
				i++;
			}
			else if (Repeat) {
				i = 0;
			}
		}
	}
}
