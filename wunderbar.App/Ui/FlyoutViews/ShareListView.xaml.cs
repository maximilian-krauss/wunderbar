using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using wunderbar.Api.dataContracts;
using wunderbar.Api.Extensions;

namespace wunderbar.App.Ui.FlyoutViews {
	public partial class ShareListView : IView {
		private listType _list;

		public ShareListView() {
			InitializeComponent();
		}

		#region IView Members

		public event EventHandler<ShowViewEventArgs> ShowView;

		public string Title {
			get { return "Share this list"; }
		}

		public Core.applicationSession Session { get; set; }

		public void ViewLoaded(object args) {
			_list = (listType) args;
			DataContext = _list;
			if (_list.Shared == 1)
				loadSharedWith();
		}

		public bool SupportsBack {
			get { return true; }
		}

		public void GoBack() {
			if (ShowView != null)
				ShowView(this, new ShowViewEventArgs(new ListsView()));
		}

		public string ActionName {
			get { throw new NotImplementedException(); }
		}

		public Action Action { get; set; }

		#endregion

		private void loadSharedWith() {
			bsy.IsBusy = true;

			var bgw = new BackgroundWorker();
			bgw.DoWork += (o, e) => {
				try {
					e.Result = Session.wunderClient.listSharedWith(_list);
				}
				catch (Exception exc) {
					Session.Logger.WarnException("Failed to load shared emails", exc);
				}
			};
			bgw.RunWorkerCompleted += (o, e) => {
				bsy.IsBusy = false;
				if (e.Result != null)
					lstSharedWith.ItemsSource = (List<string>) e.Result;
			};
			bgw.RunWorkerAsync();
		}

		private void Unshare_Click(object sender, RoutedEventArgs e) {
			var email = (string) ((ListBoxItem) lstSharedWith.ContainerFromElement((Button) sender)).Content;
			if (string.IsNullOrWhiteSpace(email))
				return;

			bsy.IsBusy = true;
			var bgw = new BackgroundWorker();
			bgw.DoWork += (o, ev) => {
				try {
					Session.wunderClient.unshareList(_list, email);
				}
				catch (Exception exc) {
					ev.Result = exc;
				}
			};
			bgw.RunWorkerCompleted += (o, ev) => {
				if (ev.Result == null)
					loadSharedWith();
				else
					bsy.IsBusy = false; //TODO: Show exception
			};
			bgw.RunWorkerAsync();
		}

		private void UnshareCompletely_Click(object sender, RoutedEventArgs e) {
			bsy.IsBusy = true;
			var bgw = new BackgroundWorker();
			bgw.DoWork += (o, ev) => {
				              try {
					              Session.wunderClient.unshareListCompletely(_list);
					              _list.Shared = 0;
								  Session.wunderClient.Synchronize();
				              }
				              catch (Exception exc) {
					              ev.Result = exc;
				              }
			              };
			bgw.RunWorkerCompleted += (o, ev) => {
				                          bsy.IsBusy = false;
										  //TODO: Show exception
			                          };
			bgw.RunWorkerAsync();
		}

		private void Share_Click(object sender, RoutedEventArgs e) {
			bsy.IsBusy = true;
			var bgw = new BackgroundWorker();
			bgw.DoWork += (o, ev) => {
				              try {
					              _list.Shared = 1;
					              Session.wunderClient.Synchronize();
				              }
				              catch (Exception exc) {
					              ev.Result = exc;
				              }
			              };
			bgw.RunWorkerCompleted += (o, ev) => {
				                          bsy.IsBusy = false;
										  //TODO: Handle and show exception
			                          };
			bgw.RunWorkerAsync();
		}
		
		private void WatermarkTextBox_KeyUp(object sender, KeyEventArgs e) {
			var s = sender as TextBox;
			if (s != null && e.Key == Key.Return && !string.IsNullOrWhiteSpace(s.Text) && s.Text.isEmail()) {
				bsy.IsBusy = true;
				var bgw = new BackgroundWorker();
				bgw.DoWork += (o, ev) => {
					try {
						Session.wunderClient.shareListWith(_list, ev.Argument.ToString());
					}
					catch(Exception exc) {
						ev.Result = exc;
					}
				};
				bgw.RunWorkerCompleted += (o, ev) => {
					if (ev.Result == null)
						loadSharedWith();
					else
						bsy.IsBusy = false; //TODO: Show exception
				};
				bgw.RunWorkerAsync(s.Text);
				s.Text = string.Empty;
			}
		}
	}
}