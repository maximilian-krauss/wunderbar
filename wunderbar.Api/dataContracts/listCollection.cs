using System.Collections.Generic;
using System.Linq;

namespace wunderbar.Api.dataContracts {
	public sealed class listCollection : List<listType> {
		public void addOrUpdateList(listType list) {
			list.trackChanges = true;
			if (this.Any(l => l.Id == list.Id)) {
				var existingList = this.First(l => l.Id == list.Id);
				existingList.beginUpdate();
				existingList.Deleted = list.Deleted;
				existingList.Inbox = list.Inbox;
				existingList.Name = list.Name;
				existingList.Position = list.Position;
				existingList.Shared = list.Shared;
				existingList.Version = list.Version;
				existingList.endUpdate();
			}
			else 
				Add(list);
			
		}
	}
}
