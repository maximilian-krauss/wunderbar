using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

/*
	LICENSE
	====================================================
	WPF HotKey Stuff
	(c) Henning Dieterichs (http://www.codeproject.com/Tips/274003/Global-Hotkeys-in-WPF#)
	Licensed under the terms and conditions of the Code Project Open License (CPOL) (http://www.codeproject.com/info/cpol10.aspx)
 */

namespace wunderbar.App.Core {
	[Serializable]
	public class hotKeyAlreadyRegisteredException : Exception {
		public hotKey HotKey { get; private set; }
		public hotKeyAlreadyRegisteredException(string message, hotKey hotKey) : base(message) { HotKey = hotKey; }
		public hotKeyAlreadyRegisteredException(string message, hotKey hotKey, Exception inner) : base(message, inner) { HotKey = hotKey; }
		protected hotKeyAlreadyRegisteredException(
		  SerializationInfo info,
		  StreamingContext context)
			: base(info, context) { }
	}
}
