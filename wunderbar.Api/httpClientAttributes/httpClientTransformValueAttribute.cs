using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace wunderbar.Api.httpClientAttributes {

	internal enum httpClientValueTransformations {
		MD5Hash,
		JSON,
		FUON //Form Url-Encoded Object Notation, yeah, I invented that Name :-P
	}

	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
	internal sealed class httpClientTransformValueAttribute : Attribute {
		public httpClientTransformValueAttribute(httpClientValueTransformations transformTo)
			:this(transformTo, false)
		{ }
		public httpClientTransformValueAttribute(httpClientValueTransformations transformTo, bool handlesNameAndValue) {
			this.transformTo = transformTo;
			this.handlesNameAndValue = handlesNameAndValue;
		}

		public httpClientValueTransformations transformTo { get; private set; }

		public bool handlesNameAndValue { get; private set; }

	}
}
