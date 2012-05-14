using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace wunderbar.Api.dataContracts {
	
	[DataContract]
	public sealed class syncTableStep2a {

		public syncTableStep2a() {
			newTasks = new List<taskType>();
			requiredTasks = new List<taskType>();
			requiredLists = new List<listType>();
		}

		[DataMember(Name = "new_tasks")]
		public List<taskType> newTasks { get; set; }

		[DataMember(Name = "required_tasks")]
		public List<taskType> requiredTasks { get; set; }

		[DataMember(Name = "required_lists")]
		public List<listType> requiredLists { get; set; }
	}
}
