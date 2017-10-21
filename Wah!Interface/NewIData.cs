using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wah_Interface {
	/// <summary>
	/// Models a piece of data with a specific type that is used to pass around as arguments, 
	/// or use as return values for Commands.
	/// </summary>
	public interface NewIData {
		R Accept<R>(NewIDataVisitor<R> visitor);
	}

	/// <summary>
	/// Models the absense of data.
	/// </summary>
	public class NewNoData : NewIData {

		public R Accept<R>(NewIDataVisitor<R> visitor) {
			return visitor.VisitNone(this);
		}

		public override string ToString() {
			return "NONE";
		}

	}

	/// <summary>
	/// Models String data.
	/// </summary>
	public class NewStringData : NewIData {
		public string Data { get; private set; }
		public Color Color { get; private set; }

		public NewStringData(string data, Color color) {
			Data = data;
			Color = color;
		}
		public NewStringData(string data) : this(data, Color.Yellow) { }

		public R Accept<R>(NewIDataVisitor<R> visitor) {
			return visitor.VisitString(this);
		}

		public override string ToString() {
			return Data;
		}

	}

	/// <summary>
	/// Models integer data
	/// </summary>
	public class NewIntData : NewIData {
		public int Data { get; private set; }

		public NewIntData(int data) {
			Data = data;
		}

		public R Accept<R>(NewIDataVisitor<R> visitor) {
			return visitor.VisitInt(this);
		}

		public override string ToString() {
			return Data.ToString();
		}
	}

	/// <summary>
	/// Models Image data
	/// </summary>
	public class NewImageData : NewIData {
		public Bitmap Data { get; private set; }

		public NewImageData(Bitmap data) {
			Data = data;
		}

		public R Accept<R>(NewIDataVisitor<R> visitor) {
			return visitor.VisitImage(this);
		}

		public override string ToString() {
			return "Image@" + this.GetHashCode();
		}
	}

}
