using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using wunderbar.Api.httpClientAttributes;

namespace wunderbar.Api.Requests {
	public sealed class unshareCompletelyRequest : baseRequest {
		[httpClientIgnoreProperty]
		public override string actionToken {
			get { return "/share/1.1.2/remove"; }
		}

		[DataMember(Name = "list_id")]
		public int listId { get; set; }
	}
}
