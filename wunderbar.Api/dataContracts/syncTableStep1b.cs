using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace wunderbar.Api.dataContracts {

	[DataContract]
	public sealed class syncTableStep1b {

		public syncTableStep1b() {
			newLists = new List<listType>();
			newTasks = new List<taskType>();
			syncedLists = new List<int>();
			requiredTasks = new List<int>();
			requiredLists = new List<int>();
		}

		[DataMember(Name = "new_lists")]
		public List<listType> newLists { get; set; }

		[DataMember(Name = "new_tasks")]
		public List<taskType> newTasks { get; set; }

		[DataMember(Name = "synced_lists")]
		public List<int> syncedLists { get; set; }

		[DataMember(Name = "required_tasks")]
		public List<int> requiredTasks { get; set; }

		[DataMember(Name = "required_lists")]
		public List<int> requiredLists { get; set; }

	}
}
