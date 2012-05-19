using System.Collections.Generic;
using System.Linq;

namespace wunderbar.Api.dataContracts {
	public class taskCollection : List<taskType> {
		public void addOrUpdateTask(taskType task) {
			task.trackChanges = true;
			if (this.Any(t => t.Id == task.Id)) {
				var existingTask = this.First(t => t.Id == task.Id);
				existingTask.Date = task.Date;
				existingTask.Deleted = task.Deleted;
				existingTask.Done = task.Done;
				existingTask.Important = task.Important;
				existingTask.Name = task.Name;
				existingTask.Note = task.Note;
				existingTask.Position = task.Position;
				existingTask.Push = task.Push;
				existingTask.Version = task.Version;
				existingTask.doneDate = task.doneDate;
				existingTask.listId = task.listId;
				existingTask.pushTS = task.pushTS;
			}
			else
				Add(task);
		}
	}
}
