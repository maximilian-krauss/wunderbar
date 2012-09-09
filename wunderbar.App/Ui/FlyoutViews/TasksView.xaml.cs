using System;
using System.Windows.Controls;
using System.Windows.Input;
using wunderbar.Api.dataContracts;
using wunderbar.App.Core;
using System.Linq;
using System.Collections.Generic;

namespace wunderbar.App.Ui.FlyoutViews {
	public partial class TasksView : IView {
		private listType _list;

		public TasksView() {
			InitializeComponent();
			lstTasks.MouseUp += (o, e) => {
				if (ShowView != null && lstTasks.SelectedItem != null && (lstTasks.SelectedItem as taskType != null))
					ShowView(this, new ShowViewEventArgs(new TaskView(), lstTasks.SelectedItem as taskType));
			};
		}

		#region IView Members

		public event EventHandler<ShowViewEventArgs> ShowView;

		public string Title {
			get { return _list != null ? _list.Name : "Tasks"; }
		}

		public applicationSession Session { get; set; }

		public void ViewLoaded(object args) {
			_list = args as listType;
			UpdateBinding();
		}

		public bool SupportsBack {
			get { return true; }
		}

		public void GoBack() {
			if(ShowView!=null)
				ShowView(this, new ShowViewEventArgs(new ListsView()));
		}

		public string ActionName {
			get { return "do something..."; }
		}

		public Action Action { get; set; }

		#endregion

		private void UpdateBinding() {
			var tasks = Session.wunderClient.Tasks.Where(t => t.Deleted == 0 && t.Done == 0 && t.listId == _list.Id).OrderByDescending(t => t.Important).ThenBy(t => t.Position);

			if (Session.Settings.sortByDueDate)
				tasks = tasks.OrderByDescending(t => t.Important).ThenByDescending(t => t.dueDate);

			lstTasks.ItemsSource = tasks;
		}

		private void TextBox_KeyUp(object sender, KeyEventArgs e) {
			var s = sender as TextBox;
			if (s != null && e.Key == Key.Return && !string.IsNullOrWhiteSpace(s.Text)) {
				Session.wunderClient.Tasks.addOrUpdateTask(Session.createTaskFromString(s.Text, _list.Id));
				s.Text = string.Empty;
				UpdateBinding();
			}
		}
	}
}