using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace wunderbar.Api.dataContracts {

	[DataContract]
	public sealed class syncTableStep1a {

		public syncTableStep1a() {
			Lists = new List<listType>();
			Tasks = new List<taskType>();
			newLists = new List<listType>();
			
			/*newLists.Add(new listType {
			                            	Deleted = 0,
											Id = 0,
											Name = "Testlist",
											Position = 20,
											Version = 0,
											userId = 29253
			                            });*/
		}

		[DataMember(Name = "lists")]
		public List<listType> Lists { get; set; }

		[DataMember(Name = "tasks")]
		public List<taskType> Tasks { get; set; }

		[DataMember(Name = "new_lists")]
		public List<listType> newLists { get; set; }

	}
}