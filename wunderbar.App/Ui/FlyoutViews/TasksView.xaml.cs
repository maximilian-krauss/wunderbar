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
			get { return "Tasks"; }
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
			lstTasks.ItemsSource =
				Session.wunderClient.Tasks.Where(t => t.Deleted == 0 && t.Done == 0 && t.listId == _list.Id).OrderBy(t => t.Important).ThenBy(t => t.Position);
		}

		private void TextBox_KeyUp(object sender, KeyEventArgs e) {
			var s = sender as TextBox;
			if (s != null && e.Key == Key.Return && !string.IsNullOrWhiteSpace(s.Text)) {
				Session.wunderClient.Tasks.addOrUpdateTask(ParseTask(s.Text));
				s.Text = string.Empty;
				UpdateBinding();
			}
		}
		private taskType ParseTask(string taskText) {
			var task = new taskType {
			                        	listId = _list.Id,
			                        	Id = new Random(Environment.TickCount).Next(-99999, -100)
			                        };

			if (taskText.StartsWith("*") || taskText.StartsWith("!")) {
				task.Important = 1;
				taskText = taskText.Substring(1).Trim();
			}

			var knownDates = new Dictionary<string, DateTime>
			                 {{"today", DateTime.Now.Date}, {"tomorrow", DateTime.Now.AddDays(1).Date}};
			if (taskText.Contains(" ")) {
				var lastWord = taskText.Substring(taskText.LastIndexOf(' ') + 1).ToLowerInvariant();
				if (knownDates.ContainsKey(lastWord)) {
					task.dueDate = knownDates[lastWord];
					taskText = taskText.Substring(0, taskText.LastIndexOf(' ')).Trim();
				}
			}

			task.Name = taskText;
			return task;
		}
	}
}