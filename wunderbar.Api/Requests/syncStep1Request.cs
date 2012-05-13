using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using wunderbar.Api.httpClientAttributes;
using System.Runtime.Serialization;
using wunderbar.Api.dataContracts;

namespace wunderbar.Api.Requests {
	public class syncStep1Request : baseRequest {

		public syncStep1Request() {
			syncTable = new syncTableStep1a();
		}

		[httpClientIgnorePropertyAttribute]
		public override string actionToken {
			get { return "/1.2.0"; }
		}

		[DataMember(Name = "step")]
		public int Step {
			get { return 1; }
		}

		[DataMember(Name = "sync_table")]
		[httpClientTransformValue(httpClientValueTransformations.FUON, true)]
		public syncTableStep1a syncTable { get; set; }
	}
}