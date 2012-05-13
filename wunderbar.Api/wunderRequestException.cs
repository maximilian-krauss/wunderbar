using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace wunderbar.Api {
	public class wunderRequestException : Exception {
		public wunderRequestException(int statusCode, string message) :base(message) {
			this.statusCode = statusCode;
		}

		public int statusCode { get; private set; }
	}
}
