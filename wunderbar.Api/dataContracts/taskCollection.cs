using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using wunderbar.Api.httpClientAttributes;
using System;

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
			else {
				var maxPos = this.Where(t => t.listId == task.listId).OrderByDescending(t => t.Position).FirstOrDefault();
				if (task.Position == 0 && maxPos != null)
					task.Position = maxPos.Position + 1;
					
				Add(task);
			}
		}

		/// <summary>Returns a List with overdue tasks.</summary>
		[IgnoreDataMember]
		[httpClientIgnoreProperty]
		public IEnumerable<taskType> dueTasks {
			get {
				return this.Where(t => t.Deleted == 0 &&
				                       t.Done == 0 &&
									   t.Date > 0 &&
									   t.dueDate.Date <= DateTime.Now.Date).OrderBy(t => t.Important);
			}
		}

	
	
	
	
	}
}
