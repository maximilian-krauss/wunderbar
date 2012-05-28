using System.Windows;

namespace wunderbar.App.Ui.Dialogs {
	public partial class exceptionDialog : Window {
		public exceptionDialog() {
			InitializeComponent();
		}
		private void btnClose_Click(object sender, RoutedEventArgs e) {
			Close();
		}
	}
}
