using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wah_Interface {
	/// <summary>
	/// Models an operation that can be applied to all types of IData.
	/// </summary>
	/// <typeparam name="R">The return type of the operation to perform</typeparam>
	public interface IDataVisitor<R> {
		R Visit(IData data);
		R VisitNone(NoData data);
		R VisitString(StringData data);
		R VisitInt(IntData data);
		R VisitImage(ImageData data);
		R VisitList(ListData data);
	}
}
