using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace wunderbar.Api.dataContracts {
	[DataContract]
	public sealed class syncTableStep2b {

		public syncTableStep2b() {
			syncedTasks = new List<syncItem>();
		}

		[DataMember(Name = "synced_tasks")]
		public List<syncItem> syncedTasks { get; set; }

	}
}
