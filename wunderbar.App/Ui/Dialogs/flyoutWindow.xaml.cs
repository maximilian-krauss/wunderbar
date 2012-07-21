﻿using System;
using System.Windows;
using System.Windows.Controls;
using wunderbar.App.Core;
using wunderbar.App.Ui.FlyoutViews;

namespace wunderbar.App.Ui.Dialogs {
	public partial class flyoutWindow {
		private readonly applicationSession _session;
		private IView _view;

		public flyoutWindow(applicationSession session) : this(session, null, null) {
		}
		public flyoutWindow(applicationSession session, IView view, object argument) {
			InitializeComponent();
			_session = session;
			if(view != null)
				ShowView(view,argument);
			else
				ShowView(new ListsView(), null);			
		}

		private void ShowView(IView view, object args) {
			if (view.Session == null)
				view.Session = _session;

			_view = view;
			dpnView.Children.Clear();
			dpnView.Children.Add((UserControl)view);
			txbTitle.Text = view.Title;
			btnBack.Visibility = (view.SupportsBack ? Visibility.Visible : Visibility.Collapsed);
			view.ViewLoaded(args);
			view.ShowView += (o, e) => ShowView(e.View, e.Arguments);

		}

		private void Window_Loaded(object sender, RoutedEventArgs e) {
			var desktopWorkingArea = SystemParameters.WorkArea;
			Left = desktopWorkingArea.Right - (Width + 10);
			Top = desktopWorkingArea.Bottom - (Height + 10);
		}

		private void Window_Deactivated(object sender, EventArgs e) {
			Close();
		}

		private void btnBack_Click(object sender, RoutedEventArgs e) {
			if (_view != null)
				_view.GoBack();
		}
	}
}
