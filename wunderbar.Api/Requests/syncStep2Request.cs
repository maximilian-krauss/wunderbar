using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using wunderbar.Api.dataContracts;
using wunderbar.Api.httpClientAttributes;

namespace wunderbar.Api.Requests {

	[DataContract]
	public sealed class syncStep2Request : baseRequest {

		public syncStep2Request() {
			syncTable = new syncTableStep2a();
		}

		[httpClientIgnoreProperty]
		public override string actionToken {
			get { return "/1.2.0"; }
		}

		[DataMember(Name = "step")]
		public int Step { get { return 2; } }

		[DataMember(Name = "sync_table")]
		[httpClientTransformValue(httpClientValueTransformations.FUON, true)]
		public syncTableStep2a syncTable { get; set; }

	}
}
