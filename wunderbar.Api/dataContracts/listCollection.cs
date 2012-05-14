using System.Collections.Generic;
using System.Linq;

namespace wunderbar.Api.dataContracts {
	public sealed class listCollection : List<listType> {
		public void addOrUpdateList(listType list) {
			if (this.Any(l => l.Id == list.Id)) {
				var existingList = this.First(l => l.Id == list.Id);
				existingList.Deleted = list.Deleted;
				existingList.Inbox = list.Inbox;
				existingList.Name = list.Name;
				existingList.Position = list.Position;
				existingList.Shared = list.Shared;
				existingList.Version = list.Version;
			}
			else 
				Add(list);
			
		}
	}
}
