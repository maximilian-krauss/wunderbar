using System;
using System.Windows;
using System.Windows.Controls;
using wunderbar.App.Core;
using wunderbar.App.Ui.FlyoutViews;
using System.Diagnostics;

namespace wunderbar.App.Ui.Dialogs {
	public partial class flyoutWindow {
		private readonly applicationSession _session;
		private IView _view;

		public flyoutWindow(applicationSession session) : this(session, null, null) {
		}
		public flyoutWindow(applicationSession session, IView view, object argument) {
			InitializeComponent();
			_session = session;
			var desktopWorkingArea = SystemParameters.WorkArea;
			Left = desktopWorkingArea.Right - (Width + 20);
			Top = desktopWorkingArea.Bottom - (Height + 20);

			if (view != null)
				ShowView(view, argument);
			else
				ShowView(session.wunderClient.loggedIn ? (IView) new ListsView() : new LoginView(), null);
		}

		private void ShowView(IView view, object args) {
			if (view.Session == null)
				view.Session = _session;

			_view = view;
			view.ViewLoaded(args);
			dpnView.Children.Clear();
			dpnView.Children.Add((UserControl)view);
			txbTitle.Text = view.Title;
			btnBack.Visibility = (view.SupportsBack ? Visibility.Visible : Visibility.Collapsed);
			view.ShowView += (o, e) => ShowView(e.View, e.Arguments);
		}

		private void Window_Deactivated(object sender, EventArgs e) {
			Close();
		}

		private void btnBack_Click(object sender, RoutedEventArgs e) {
			if (_view != null)
				_view.GoBack();
		}

		private void Hyperlink_Click(object sender, RoutedEventArgs e) {
			Process.Start("https://www.wunderlist.com");
		}
	}
}
