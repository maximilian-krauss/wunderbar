using System.Windows;
using System.Windows.Documents;
using System.Diagnostics;

namespace wunderbar.App.Ui.Dialogs {
	public partial class aboutDialog : Window {
		public aboutDialog() {
			InitializeComponent();
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
