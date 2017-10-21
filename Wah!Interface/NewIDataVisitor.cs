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
	public interface NewIDataVisitor<R> {
		R Visit(NewIData data);
		R VisitNone(NewNoData data);
		R VisitString(NewStringData data);
		R VisitInt(NewIntData data);
		R VisitImage(NewImageData data);
	}
}
