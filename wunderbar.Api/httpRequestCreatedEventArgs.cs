using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace wunderbar.Api {
	public sealed class httpRequestCreatedEventArgs : EventArgs {

		internal httpRequestCreatedEventArgs(HttpWebRequest request) {
			Request = request;
		}

		public HttpWebRequest Request { get; private set; }

	}
}
