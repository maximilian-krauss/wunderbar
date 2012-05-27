using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace wunderbar.Api.dataContracts {
	[DataContract]
	public sealed class syncItem {
		[DataMember(Name = "id")]
		public int Id { get; set; }
	}
}
