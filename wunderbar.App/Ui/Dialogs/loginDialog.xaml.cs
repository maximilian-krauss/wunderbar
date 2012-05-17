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
using wunderbar.Api;

namespace wunderbar.App.Ui.Dialogs {
	/// <summary>
	/// Interaction logic for loginDialog.xaml
	/// </summary>
	public partial class loginDialog : Window {

		public loginDialog() {
			InitializeComponent();
			Credentials = new digestCredentials();
			DataContext = Credentials;
		}

		public digestCredentials Credentials { get; private set; }

		private void btnClose_Click(object sender, RoutedEventArgs e) {
			Close();
		}

		private void btnLogin_Click(object sender, RoutedEventArgs e) {
			//TODO: Fix with Binding
			Credentials.Password = txtPassword.Password;
			DialogResult = true;
		}
	}
}