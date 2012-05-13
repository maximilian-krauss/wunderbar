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
			syncedLists = new List<listType>();
			requiredTasks = new List<taskType>();
			requiredLists = new List<listType>();
		}

		[DataMember(Name = "new_lists")]
		public List<listType> newLists { get; set; }

		[DataMember(Name = "new_tasks")]
		public List<taskType> newTasks { get; set; }

		[DataMember(Name = "synced_lists")]
		public List<listType> syncedLists { get; set; }

		[DataMember(Name = "required_tasks")]
		public List<taskType> requiredTasks { get; set; }

		[DataMember(Name = "required_lists")]
		public List<listType> requiredLists { get; set; }

	}
}
