using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using wunderbar.Api.httpClientAttributes;

namespace wunderbar.Api.Requests {
	
	[DataContract]
	public abstract class baseRequest {

		[httpClientIgnorePropertyAttribute]
		public virtual string baseUrl { get { return "https://sync.wunderlist.net"; } }

		[httpClientIgnorePropertyAttribute]
		public abstract string actionToken { get; }

		[DataMember(Name = "email")]
		public string eMail { get; set; }

		[DataMember(Name = "password")]
		[httpClientTransformValue(httpClientValueTransformations.MD5Hash)]
		public string Password { get; set; }


	}
}
