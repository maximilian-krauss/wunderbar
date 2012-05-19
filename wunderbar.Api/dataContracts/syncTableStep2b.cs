using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace wunderbar.Api.dataContracts {
	[DataContract]
	public sealed class syncTableStep2b {

		public syncTableStep2b() {
			syncedTasks = new List<int>();
		}

		[DataMember(Name = "synced_tasks")]
		public List<int> syncedTasks { get; set; }

	}
}
