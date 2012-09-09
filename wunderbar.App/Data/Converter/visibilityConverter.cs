using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace wunderbar.App.Data.Converter {
	public class visibilityConverter : IValueConverter {
		#region IValueConverter Members

		public virtual object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			if (value != null && value is int)
				return (int)value >= 1 ? Visibility.Visible : Visibility.Collapsed;
			if (value != null && value is bool)
				return (bool) value ? Visibility.Visible : Visibility.Collapsed;
			return null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			return null;
		}

		#endregion
	}
}
