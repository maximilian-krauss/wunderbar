using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace wunderbar.Api.httpClientAttributes {
	[AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
	internal sealed class httpClientIgnorePropertyAttribute : Attribute {
	}
}
