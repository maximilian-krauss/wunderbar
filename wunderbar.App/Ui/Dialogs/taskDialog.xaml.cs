using System.Windows;
using System.Collections;
using wunderbar.Api.dataContracts;

namespace wunderbar.App.Ui.Dialogs {
	public partial class taskDialog : Window {
		public taskDialog() {
			InitializeComponent();
		}

		public IEnumerable ListsItemSource {
			set { cboLists.ItemsSource = value; } }

		private void btnClose_Click(object sender, RoutedEventArgs e) {
			DialogResult = true;
			Close();
		}

		private void btnDone_Click(object sender, RoutedEventArgs e) {
			(DataContext as taskType).markAsDone();
			Close();
		}

		private void btnPostpone_Click(object sender, RoutedEventArgs e) {
			(DataContext as taskType).Postpone();
			Close();
		}

		private void Window_Loaded(object sender, RoutedEventArgs e) {
			if ((DataContext as taskType).Id == 0) {
				btnClose.Content = "Add Task";
				btnPostpone.Visibility = Visibility.Hidden;
				btnDone.Visibility = Visibility.Hidden;
			}
		}

		private void Button_Click(object sender, RoutedEventArgs e) {
			ppDate.IsOpen = true;
		}

		private void btnNoDate_Click(object sender, RoutedEventArgs e) {
			(DataContext as taskType).Date = 0;
		}
	}
}
