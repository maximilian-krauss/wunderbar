using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using wunderbar.Api.dataContracts;

namespace wunderbar.App.Data {
	public sealed class taskModel : baseModel {
		private readonly taskType _task;

		public taskModel(taskType task) {
			_task = task;
		}
		public taskModel() {
			_task = new taskType();
		}

		public taskType Source { get { return _task; } }

		public string Name {
			get { return _task.Name; }
			set {
				_task.Name = value;
				onPropertyChanged("Name");
			}
		}

	}
}