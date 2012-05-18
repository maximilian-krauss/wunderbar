using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace wunderbar.App.Ui.Dialogs {
	public partial class aboutDialog : Window {
		private static aboutDialog _instance;

		public aboutDialog() {
			InitializeComponent();
		}

		public static aboutDialog Instance {
			get { return _instance ?? (_instance = new aboutDialog()); }
		}

	}
}
