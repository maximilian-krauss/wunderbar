using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace wunderbar.App.Core {
	internal enum trayContextTypes {
		loginRequired,
		signedIn,
		synchronizationInProgress,
		errorOccoured
	}
}