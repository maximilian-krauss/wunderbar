using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using wunderbar.Api.dataContracts;

namespace wunderbar.Api.Responses {
	[DataContract]
	public class syncStep2Response : baseResponse {

		public syncStep2Response() {
			syncTable = new syncTableStep2b();
		}

		[DataMember(Name = "sync_table")]
		public syncTableStep2b syncTable { get; set; }

	}
}
