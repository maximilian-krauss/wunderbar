using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace wunderbar.Api.dataContracts {

	[DataContract]
	public sealed class taskType : dataBaseType {

		[DataMember(Name = "date")]
		public int Date { get; set; }

		[DataMember(Name = "done")]
		public int Done { get; set; }

		[DataMember(Name = "done_date")]
		public int doneDate { get; set; }

		[DataMember(Name = "important")]
		public int Important { get; set; }

		[DataMember(Name = "list_id")]
		public int listId { get; set; }

		[DataMember(Name = "note")]
		public string Note { get; set; }

		[DataMember(Name = "push")]
		public int Push { get; set; }

		[DataMember(Name = "push_ts")] 
		public int pushTS { get; set; } //TODO: Find out what that property does

	}
}
