using System;
using System.Collections;
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
	public interface IData {
		R Accept<R>(IDataVisitor<R> visitor);
	}

	/// <summary>
	/// Models the absense of data.
	/// </summary>
	public class NoData : IData {

		public R Accept<R>(IDataVisitor<R> visitor) {
			return visitor.VisitNone(this);
		}

		public override string ToString() {
			return "NONE";
		}

	}

	/// <summary>
	/// Models String data.
	/// </summary>
	public class StringData : IData {
		public string Data { get; private set; }
		public Color Color { get; private set; }

		public StringData(string data, Color color) {
			Data = data;
			Color = color;
		}
		public StringData(string data) : this(data, Color.Yellow) { }

		public R Accept<R>(IDataVisitor<R> visitor) {
			return visitor.VisitString(this);
		}

		public override string ToString() {
			return Data;
		}

	}

	/// <summary>
	/// Models integer data
	/// </summary>
	public class IntData : IData {
		public int Data { get; private set; }

		public IntData(int data) {
			Data = data;
		}

		public R Accept<R>(IDataVisitor<R> visitor) {
			return visitor.VisitInt(this);
		}

		public override string ToString() {
			return Data.ToString();
		}
	}

	/// <summary>
	/// Models Image data
	/// </summary>
	public class ImageData : IData {
		public Bitmap Data { get; private set; }

		public ImageData(Bitmap data) {
			Data = data;
		}

		public R Accept<R>(IDataVisitor<R> visitor) {
			return visitor.VisitImage(this);
		}

		public override string ToString() {
			return "Image@" + this.GetHashCode();
		}
	}

	/// <summary>
	/// Models list data, usually of one type of IData
	/// </summary>
	public class ListData : IData, IEnumerable<IData> {
		public IList<IData> Data { get; private set; }

		public ListData(params IData[] data) : this(data.ToList()) {}
		public ListData(IList<IData> data) {
			Data = data;
		}

		public R Accept<R>(IDataVisitor<R> visitor) {
			return visitor.VisitList(this);
		}

		public override string ToString() {
			return "List@" + this.GetHashCode();
		}

		/// <summary>
		/// Transforms this list into another list of a different type (R) using the given mapper function,
		/// assuming all elements of this list are of type D
		/// </summary>
		/// <typeparam name="D">the type to assume elements of this list are</typeparam>
		/// <typeparam name="R">the return type of the mapped list</typeparam>
		/// <param name="mapper">the function that maps D to R</param>
		/// <returns>the mapped list of elements</returns>
		public IList<R> Map<D, R>(Func<D, R> mapper) where D : IData {
			IList<R> result = new List<R>();
			foreach(IData d in Data) {
				AssertType<D>(d);
				result.Add(mapper.Invoke((D) d));
			}
			return result;
		}

		/// <summary>
		/// Transforms this list into another list of a different type (R) using the given mapper function
		/// </summary>
		/// <typeparam name="R">the type to transform the list into</typeparam>
		/// <param name="mapper">the mapper function</param>
		/// <returns>the transformed list of type R</returns>
		public IList<R> Map<R>(Func<IData, R> mapper) {
			return Data.Select(mapper).ToList();
		}

		/// <summary>
		/// Folds the data of this list into a single value of the given R type using the given folder function,
		/// assuming all elements of this list are of type D
		/// </summary>
		/// <typeparam name="D">the data type to assume all of this list are</typeparam>
		/// <typeparam name="R">the return type</typeparam>
		/// <param name="folder">the folder function</param>
		/// <param name="start">the base value for the folder function to start with</param>
		/// <returns>the final R value</returns>
		public R Fold<D, R>(Func<R, D, R> folder, R start) where D : IData {
			R result = start;
			foreach(IData d in Data) {
				AssertType<D>(d);
				result = folder.Invoke(result, (D) d);
			}
			return result;
		}

		/// <summary>
		/// Folds the data of this list into a single value of the given R type using the given folder function.
		/// </summary>
		/// <typeparam name="R">the return type</typeparam>
		/// <param name="folder">the folder function</param>
		/// <param name="start">the base value for the folder function to start with</param>
		/// <returns>the final R value</returns>
		public R Fold<R>(Func<R, IData, R> folder, R start) {
			return Data.Aggregate(start, folder);
		}

		/// <summary>
		/// Do any of the elements of this list satisfy the given predicate?
		/// (Assumes all elements of this list are of type D)
		/// </summary>
		/// <typeparam name="D">the type of all elements of this list</typeparam>
		/// <param name="pred">the predicate</param>
		/// <returns>True if any satisfy the predicate</returns>
		public bool Any<D>(Func<D, bool> pred) where D : IData {
			foreach(IData d in Data) {
				AssertType<D>(d);
				if(pred.Invoke((D) d)) {
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Do any of the elements of this list satisfy the given predicate?
		/// </summary>
		/// <param name="pred">the predicate</param>
		/// <returns>True if any satisfy the predicate</returns>
		public bool Any(Func<IData, bool> pred) {
			return Data.Any(pred);
		}

		/// <summary>
		/// Do all of the elements of this list satisfy the given predicate?
		/// (Assumes all elements of this list are of type D)
		/// </summary>
		/// <typeparam name="D">the type of all elements of this list</typeparam>
		/// <param name="pred">the predicate</param>
		/// <returns>True if all satisfy the predicate</returns>
		public bool All<D>(Func<D, bool> pred) where D : IData {
			foreach (IData d in Data) {
				AssertType<D>(d);
				if (pred.Invoke((D)d)) {
					return false;
				}
			}
			return true;
		}

		/// <summary>
		/// Do all of the elements of this list satisfy the given predicate?
		/// </summary>
		/// <param name="pred">the predicate</param>
		/// <returns>True if all satisfy the predicate</returns>
		public bool All<D>(Func<IData, bool> pred) where D : IData {
			return Data.All(pred);
		}

		/// <summary>
		/// Does this list have elements all of the specified type?
		/// </summary>
		/// <typeparam name="D">the type to check</typeparam>
		/// <returns>True if all the elements are in fact of type D</returns>
		public bool AllOfType<D>() {
			return All((IData d) => d is D);
		}

		private void AssertType<D>(IData toTest) where D : IData {
			if(!(toTest is D)) {
				throw new WahListTypeException(typeof(D), toTest);
			}
		}

		public IEnumerator<IData> GetEnumerator() {
			return Data.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}
	}

}
