using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using wunderbar.Api.dataContracts;
using wunderbar.App.Core;
using System.Linq;

namespace wunderbar.App.Ui.FlyoutViews {
	public partial class TaskView : UserControl, IView {
		private taskType _task;

		public TaskView() {
			InitializeComponent();
		}

		private void dateTriggerButton_Click(object sender, RoutedEventArgs e) {
			ppDate.IsOpen = true;
		}
		private void btnNoDate_Click(object sender, RoutedEventArgs e) {
			_task.Date = 0;
		}
		private void Calendar_SelectedDatesChanged(object sender, SelectionChangedEventArgs e) {
			var calendar = sender as Calendar;
			if (calendar != null)
				((calendar.Parent as Border).Parent as Popup).IsOpen = false;
		}

		#region IView Members

		public event EventHandler<ShowViewEventArgs> ShowView;

		public string Title {
			get { return "Task"; }
		}

		public applicationSession Session { get; set; }

		public void ViewLoaded(object args) {
			_task = args as taskType;
			cboLists.ItemsSource = Session.wunderClient.Lists;
			DataContext = _task;
		}

		public bool SupportsBack {
			get { return true; }
		}

		public void GoBack() {
			if(ShowView != null)
				ShowView(this, new ShowViewEventArgs(new TasksView(), Session.wunderClient.Lists.FirstOrDefault(l => l.Id == _task.listId)));
		}

		public string ActionName {
			get { return "None"; }
		}

		public Action Action { get; set; }

		#endregion

		private void CompleteTask_Click(object sender, RoutedEventArgs e) {
			_task.markAsDone();
			GoBack();
		}

		private void DeleteTask_Click(object sender, RoutedEventArgs e) {
			_task.Deleted = 1;
			GoBack();
		}
	}
}
