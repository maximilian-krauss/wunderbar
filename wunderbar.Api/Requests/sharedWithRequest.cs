using wunderbar.Api.httpClientAttributes;
using System.Runtime.Serialization;

namespace wunderbar.Api.Requests {
	public sealed class sharedWithRequest : baseRequest {
		
		[httpClientIgnorePropertyAttribute]
		public override string actionToken {
			get { return "/share/1.1.2/emails"; }
		}

		[DataMember(Name = "list_id")]
		public int listId { get; set; }
	}
}
