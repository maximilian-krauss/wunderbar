using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace wunderbar.Api.dataContracts {

	[DataContract]
	public sealed class listType : dataBaseType {
		
		//TODO: Implement INotifyPropertyChanged

		public listType() {
			Inbox = 0;
			Shared = 0;
		}

		[DataMember(Name = "inbox")]
		public int Inbox { get; set; }

		[DataMember(Name = "shared")]
		public int Shared { get; set; }

	}
}
