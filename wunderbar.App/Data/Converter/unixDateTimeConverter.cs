using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace wunderbar.App.Data.Converter {
	public sealed class unixDateTimeConverter : IValueConverter {
		#region IValueConverter Members

		//TODO: Redundant, see Api.Extensions.DateTimeExtensions

		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Local);
			origin = origin.AddHours(TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).Hours); //This is important because the value provided by wunderlist is an UTC Date
			return origin.AddSeconds((long)value);
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			if (value is DateTime) {
				var dt = (DateTime) value;
				if (dt.Year >= (DateTime.Now.Year - 2)) {
					DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
					TimeSpan diff = dt - origin;
					return Math.Floor((diff.TotalSeconds - TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).Seconds));
				}
			}
			return 0;
		}

		#endregion
	}
}
