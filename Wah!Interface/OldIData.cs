using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wah_Interface {
	/// <summary>
	/// Models an abstract piece of data to be passed around in a program like native data types.
	/// Supports some casting and visitors.
	/// </summary>
	public interface OldIData {
		string AsString();
		bool AsBool();
		Bitmap AsBitmap();
	}
	public class OldNoData : OldIData {

		public bool AsBool() {
			throw new NoReturnException();
		}

		public string AsString() {
			throw new NoReturnException();
		}
		public Bitmap AsBitmap() {
			throw new NoReturnException();
		}
	}
	public class OldStringData : OldIData {
		public string Value { get; private set; }
		public Color Color { get; private set; }
		public OldStringData(string value, Color color) {
			Value = value;
			Color = color;
		}
		public OldStringData(string value) : this(value, Color.Yellow) { }
		public bool AsBool() {
			bool b = false;
			if (bool.TryParse(Value, out b)) {
				return b;
			}
			else {
				throw new IllformedInputException("data " + Value + " cannot be cast to a bool");
			}
		}

		public string AsString() {
			return Value;
		}
		public Bitmap AsBitmap() {
			throw new IllformedInputException("data " + Value + " cannot be cast to a bitmap");
		}
	}
	public class OldBoolData : OldIData {
		public bool Value { get; private set; }
		public OldBoolData(bool value) {
			Value = value;
		}

		public string AsString() {
			return Value.ToString();
		}

		public int AsInt() {
			return Value ? 1 : 0;
		}

		public bool AsBool() {
			return Value;
		}
		public Bitmap AsBitmap() {
			throw new IllformedInputException("data " + Value + " cannot be cast to a bitmap");
		}
	}

	public class OldListData : OldIData {
		public IList<OldIData> Value { get; private set; }
		public OldListData(IList<OldIData> value) {
			Value = value;
		}

		public string AsString() {
			return Value.Aggregate("", (soFar, next) => soFar + "\n" + next.AsString());
		}

		public int AsInt() {
			throw new IllformedInputException("data of ListReturn cannot be cast to an int");
		}

		public bool AsBool() {
			throw new IllformedInputException("data of ListReturn cannot be cast to a bool");
		}
		public Bitmap AsBitmap() {
			throw new IllformedInputException("data of ListReturn cannot be cast to a bitmap");
		}
	}
	public class OldBitmapData : OldIData {
		public Bitmap Value { get; private set; }
		public OldBitmapData(Bitmap value) {
			Value = value;
		}
		public Bitmap AsBitmap() {
			return Value;
		}

		public bool AsBool() {
			return Value != null;
		}

		public int AsInt() {
			throw new IllformedInputException("bitmap cannot be cast to an int");
		}

		public string AsString() {
			return Value.ToString();
		}
	}
}
