using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace wunderbar.App.Core {
	internal abstract class baseController:IDisposable{

		protected baseController(applicationSession session) {
			Session = session;
		}

		public applicationSession Session { get; set; }


		#region IDisposable Members

		public virtual void Dispose() {
		}

		#endregion
	}
}
