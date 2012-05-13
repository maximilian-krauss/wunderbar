using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace wunderbar.Api {
	public sealed class synchronizationException : wunderRequestException {
		public synchronizationException(int step, int status)
			: base(status, string.Format("Synchronization failed on Step {0} with Statuscode {1}.", step, status)) {
			Step = step;
		}

		public int Step { get; set; }
	}
}
