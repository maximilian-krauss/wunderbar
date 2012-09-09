using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using wunderbar.Api;
using wunderbar.App.Core;

namespace wunderbar.App.Ui.FlyoutViews {
	public partial class LoginView : IView {
		public LoginView() {
			InitializeComponent();
			ErrorImage.Source = Imaging.CreateBitmapSourceFromHIcon(SystemIcons.Error.Handle, Int32Rect.Empty,
			                                                        BitmapSizeOptions.FromEmptyOptions());
		}

		#region Implementation of IView

		public event EventHandler<ShowViewEventArgs> ShowView;

		public void onShowView(ShowViewEventArgs e) {
			var handler = ShowView;
			if (handler != null) handler(this, e);
		}

		public string Title { get { return "Login to wunderlist"; } }
		public applicationSession Session { get; set; }
		public void ViewLoaded(object args) {
			
		}

		public bool SupportsBack { get { return false; } }
		public void GoBack() {
			throw new NotImplementedException();
		}

		public string ActionName { get; private set; }
		public Action Action { get; set; }

		#endregion

		private void Login_Click(object sender, RoutedEventArgs e) {
			bsy.IsBusy = true;
			var bgw = new BackgroundWorker();
			bgw.DoWork += (o, ev) => {
				              var credentials = (digestCredentials) ev.Argument;
				              try {
					              if (!Session.wunderClient.Login(credentials.eMail, credentials.Password))
						              throw new Exception("Login failed!");

					              Session.Settings.eMail = credentials.eMail;
					              Session.Settings.Password = credentials.Password;
					              Session.wunderClient.Synchronize();
				              }
				              catch (Exception exc) {
					              ev.Result = exc;
				              }
			              };
			bgw.RunWorkerCompleted += (o, ev) => {
				                          bsy.IsBusy = false;
										  if (ev.Result == null) {
											  onShowView(new ShowViewEventArgs(new ListsView()));
											  return;
										  }
										  grdError.Visibility = Visibility.Visible;
			                          };
			grdError.Visibility = Visibility.Collapsed;
			bgw.RunWorkerAsync(new digestCredentials {eMail = txtEMail.Text, Password = txtPassword.Password});
		}

	}
}
