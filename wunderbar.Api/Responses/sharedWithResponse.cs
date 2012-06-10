using System.Collections.Generic;
using System.Runtime.Serialization;

namespace wunderbar.Api.Responses {
	[DataContract]
	public sealed class sharedWithResponse : baseResponse {

		[DataMember(Name = "emails")]
		public List<string> eMails { get; set; }
	}
}
