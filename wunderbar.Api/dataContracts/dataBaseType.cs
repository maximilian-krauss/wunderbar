using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace wunderbar.Api.dataContracts {

	[DataContract]
	public abstract class dataBaseType {

		[DataMember(Name = "deleted")]
		public int Deleted { get; set; }

		[DataMember(Name = "name")]
		public string Name { get; set; }

		[DataMember(Name = "online_id")]
		public int Id { get; set; }

		[DataMember(Name = "version")]
		public int Version { get; set; }

		[DataMember(Name = "position")]
		public int Position { get; set; }

		[DataMember(Name = "user_id")]
		public int userId { get; set; }

	}
}
