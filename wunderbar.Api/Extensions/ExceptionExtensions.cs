using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace wunderbar.Api.Extensions {
	public static class ExceptionExtensions {
		public static Exception LowestException(this Exception exc) {
			var inner = exc;
			for (Exception e = exc; e != null; e = e.InnerException)
				inner = e;
			return inner;
		}
	}
}