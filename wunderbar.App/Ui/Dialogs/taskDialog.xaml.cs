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
using wunderbar.App.Core;
using System.Collections;

namespace wunderbar.App.Ui.Dialogs {
	/// <summary>
	/// Interaction logic for taskDialog.xaml
	/// </summary>
	public partial class taskDialog : Window {
		public taskDialog() {
			InitializeComponent();
		}

		public IEnumerable ListsItemSource { get { return cboLists.ItemsSource; } set { cboLists.ItemsSource = value; } }

		private void btnClose_Click(object sender, RoutedEventArgs e) {
			Close();
		}
	}
}
