using System;

namespace wunderbar.Api {
	public class wunderException : Exception {
		public wunderException(string message, Exception inner) : base(message, inner) {
		}
		public wunderException(string message) : this(message, null) {
		}
		public wunderException() : this(string.Empty,null) {
		}
	}
}
