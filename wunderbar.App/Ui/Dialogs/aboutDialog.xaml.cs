using System.Windows;
using System.Windows.Documents;
using System.Diagnostics;

namespace wunderbar.App.Ui.Dialogs {
	public partial class aboutDialog : Window {
		private static aboutDialog _instance;

		public aboutDialog() {
			InitializeComponent();
		}

		public static aboutDialog Instance {
			get { return _instance ?? (_instance = new aboutDialog()); }
		}

		private void Hyperlink_Click(object sender, RoutedEventArgs e) {
			var link = (sender as Hyperlink);
			if (link != null)
				Process.Start(link.NavigateUri.ToString());
		}

		private void Button_Click(object sender, RoutedEventArgs e) {
			Close();
		}

	}
}
