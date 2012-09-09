using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace wunderbar.App.Data.Converter {
	public sealed class inverseVisibilityConverter : visibilityConverter {
		public override object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			var result = base.Convert(value, targetType, parameter, culture);
			if (result != null) {
				return ((Visibility) result) == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
			}
			return null;
		}
	}
}
