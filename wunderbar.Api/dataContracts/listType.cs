using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace wunderbar.Api.dataContracts {

	[DataContract]
	public sealed class listType : dataBaseType {

		private int _inbox;
		private int _shared;

		public listType() {
			_inbox = 0;
			_shared = 0;
		}

		[DataMember(Name = "inbox")]
		public int Inbox { get { return _inbox; } set { _inbox = value; onPropertyChanged("Inbox"); } }

		[DataMember(Name = "shared")]
		public int Shared { get { return _shared; } set { _shared = value; onPropertyChanged("Shared"); } }

	}
}
