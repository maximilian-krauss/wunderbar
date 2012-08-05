using System;
using System.Runtime.Remoting.Contexts;
using System.Windows.Controls;
using System.Windows.Input;
using wunderbar.Api.dataContracts;
using wunderbar.App.Core;
using System.Linq;

namespace wunderbar.App.Ui.FlyoutViews {
	public partial class ListsView : UserControl, IView {
		public ListsView() {
			InitializeComponent();
			lstTasks.MouseUp += (o, e) => {
									if(ShowView != null && lstTasks.SelectedItem != null && (lstTasks.SelectedItem as listType != null))
										ShowView(this, new ShowViewEventArgs(new TasksView(), lstTasks.SelectedItem as listType));
			                    };
		}

		#region IView Members

		public event EventHandler<ShowViewEventArgs> ShowView;

		public string Title {
			get { return "Lists"; }
		}

		public applicationSession Session {
			get; set; }

		public void ViewLoaded(object args) {
			//TODO: Find a better solution for this
			var lists = Session.wunderClient.Lists.Where(l => l.Deleted == 0).OrderBy(l => l.Position);
			lists.ToList().ForEach(l => {
				l.taskCount = Session.wunderClient.Tasks.Count(t => t.listId == l.Id && t.Done == 0 && t.Deleted == 0);
				l.dueTaskCount =
					Session.wunderClient.Tasks.Count(t => t.listId == l.Id && t.Done == 0 && t.Deleted == 0 && t.Date > 0 && t.dueDate <= DateTime.Now.Date);
			});

			lstTasks.ItemsSource = lists;
		}

		public bool SupportsBack {
			get { return false; }
		}

		public void GoBack() {
			throw new NotImplementedException();
		}

		public string ActionName {
			get { return "Add new List"; }
		}

		public Action Action { get; set; }

		#endregion
	}
}
