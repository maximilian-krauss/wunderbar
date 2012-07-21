using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace wunderbar.App.Data.Converter {
	public sealed class prettyDateViewConverter : prettyDateConverter {
		public override object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			string rValue = base.Convert(value, targetType, parameter, culture).ToString();
			return rValue == NO_DATE_STRING ? string.Empty : rValue;
		}
	}
}
