using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using wunderbar.Api.Extensions;

namespace wunderbar.App.Data.Converter {
	public class prettyDateConverter : IValueConverter {
		protected const string NO_DATE_STRING = "No date set";

		#region IValueConverter Members

		public virtual object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			long? ticks = (long?) value;
			if (ticks == null || ticks == 0)
				return NO_DATE_STRING;

			return DateTime.Now.FromUnixTimeStamp(ticks).ToRelativeDate();
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			return null; //Not needed
		}

		#endregion
	}
}
