using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace wunderbar.Api.dataContracts {
	[DataContract]
	public sealed class syncTableStep2b {

		[DataMember(Name = "synced_tasks")]
		public List<taskType> syncedTasks { get; set; }

	}
}
