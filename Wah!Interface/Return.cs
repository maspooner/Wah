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
	public interface IData {
		string AsString();
		int AsInt();
		bool AsBool();
		Bitmap AsBitmap();
		R Accept<R>(IDataVisitor<R> irv);
	}
	public interface IDataVisitor<R> {
		R VisitNo(NoData nr);
		R VisitString(StringData sr);
		R VisitInt(IntData ir);
		R VisitBool(BoolData br);
		R VisitList(ListData lr);
		R VisitBitmap(BitmapData br);
	}
	public class NoData : IData {

		public bool AsBool() {
			throw new NoReturnException();
		}

		public int AsInt() {
			throw new NoReturnException();
		}

		public string AsString() {
			throw new NoReturnException();
		}
		public Bitmap AsBitmap() {
			throw new NoReturnException();
		}
		public R Accept<R>(IDataVisitor<R> irv) {
			return irv.VisitNo(this);
		}
	}
	public class StringData : IData {
		public string Value { get; private set; }
		public Color Color { get; private set; }
		public StringData(string value, Color color) {
			Value = value;
			Color = color;
		}
		public StringData(string value) : this(value, Color.Yellow) { }
		public bool AsBool() {
			bool b = false;
			if (bool.TryParse(Value, out b)) {
				return b;
			}
			else {
				throw new IllformedInputException("data " + Value + " cannot be cast to a bool");
			}
		}

		public int AsInt() {
			int o = 0;
			if (int.TryParse(Value, out o)) {
				return o;
			}
			else {
				throw new IllformedInputException("data " + Value + " cannot be cast to an int");
			}
		}

		public string AsString() {
			return Value;
		}
		public Bitmap AsBitmap() {
			throw new IllformedInputException("data " + Value + " cannot be cast to a bitmap");
		}
		public R Accept<R>(IDataVisitor<R> irv) {
			return irv.VisitString(this);
		}
	}
	public class IntData : IData {
		public int Value { get; private set; }
		public IntData(int value) {
			Value = value;
		}

		public string AsString() {
			return Value.ToString();
		}

		public int AsInt() {
			return Value;
		}

		public bool AsBool() {
			return Value != 0;
		}
		public Bitmap AsBitmap() {
			throw new IllformedInputException("data " + Value + " cannot be cast to a bitmap");
		}
		public R Accept<R>(IDataVisitor<R> irv) {
			return irv.VisitInt(this);
		}
	}
	public class BoolData : IData {
		public bool Value { get; private set; }
		public BoolData(bool value) {
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
		public R Accept<R>(IDataVisitor<R> irv) {
			return irv.VisitBool(this);
		}
	}

	public class ListData : IData {
		public IList<IData> Value { get; private set; }
		public ListData(IList<IData> value) {
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
		public R Accept<R>(IDataVisitor<R> irv) {
			return irv.VisitList(this);
		}
	}
	public class BitmapData : IData {
		public Bitmap Value { get; private set; }
		public BitmapData(Bitmap value) {
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
		public R Accept<R>(IDataVisitor<R> irv) {
			return irv.VisitBitmap(this);
		}
	}

	public class OutputVisitor : IDataVisitor<object> {
		private ICore wah;
		public OutputVisitor(ICore wah) {
			this.wah = wah;
		}
		public object VisitBitmap(BitmapData br) {
			wah.Putln("Bitmap", Color.Aquamarine);
			wah.Display.ShowExtra(new SimpleImage(br.AsBitmap()));
			return null;
		}

		public object VisitBool(BoolData br) {
			wah.Putln(br.AsString(), Color.Honeydew);
			return null;
		}

		public object VisitInt(IntData ir) {
			wah.Putln(ir.AsString(), Color.OrangeRed);
			return null;
		}

		public object VisitList(ListData lr) {
			foreach (IData ir in lr.Value) {
				ir.Accept(this);
			}
			return null;
		}

		public object VisitNo(NoData nr) {
			return null;
		}

		public object VisitString(StringData sr) {
			wah.Putln(sr.Value, sr.Color);
			return null;
		}
	}

}
