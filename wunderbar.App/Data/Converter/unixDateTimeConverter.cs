using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using wunderbar.Api.Extensions;

namespace wunderbar.App.Data.Converter {
	public sealed class unixDateTimeConverter : IValueConverter {
		#region IValueConverter Members
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			return DateTime.Now.FromUnixTimeStamp((long) value);
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			return value is DateTime ? ((DateTime) value).ToUnixTimeStamp() : 0;
		}

		#endregion
	}
}