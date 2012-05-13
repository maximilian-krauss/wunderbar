using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace wunderbar.Api.Responses {
	[DataContract]
	public class baseResponse {

		[DataMember(Name = "code")]
		public int statusCode { get; set; }

		[DataMember(Name = "user_id")]
		public int userId { get; set; }

	}
}
