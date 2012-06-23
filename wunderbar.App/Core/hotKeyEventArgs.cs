using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/*
	LICENSE
	====================================================
	WPF HotKey Stuff
	(c) Henning Dieterichs (http://www.codeproject.com/Tips/274003/Global-Hotkeys-in-WPF#)
	Licensed under the terms and conditions of the Code Project Open License (CPOL) (http://www.codeproject.com/info/cpol10.aspx)
 */

namespace wunderbar.App.Core {
	public class hotKeyEventArgs : EventArgs {
		public hotKey HotKey { get; private set; }

		public hotKeyEventArgs(hotKey hotKey) {
			HotKey = hotKey;
		}
	}
}
