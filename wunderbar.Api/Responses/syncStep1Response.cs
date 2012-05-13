using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using wunderbar.Api.dataContracts;
using System.Runtime.Serialization;

namespace wunderbar.Api.Responses {
	[DataContract]
	public sealed class syncStep1Response : baseResponse {

		public syncStep1Response() {
			syncTable = new syncTableStep1b();
		}

		[DataMember(Name = "sync_table")]
		public syncTableStep1b syncTable { get; set; }

	}
}