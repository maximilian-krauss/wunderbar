using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Windows.Threading;

namespace wunderbar.App.Core {
	internal sealed class syncController : baseController {
		private readonly Timer _tmrSync;

		public syncController(applicationSession session):base(session) {
			session.Settings.PropertyChanged += Settings_PropertyChanged;
			_tmrSync = new Timer();
			_tmrSync.Elapsed += _tmrSync_Elapsed;
			setupSyncTimer();
			GC.KeepAlive(_tmrSync);
		}

		void _tmrSync_Elapsed(object sender, ElapsedEventArgs e) {
			//Only synchronize if logged in
			if (Session.wunderClient.loggedIn)
				Session.mainWindow.Dispatcher.Invoke(DispatcherPriority.Background,
				                                     new Action(() => Session.Synchronize()));
		}

		private void  setupSyncTimer() {
			//TODO: Remove if working
			_tmrSync.Interval = (Session.Settings.autoSyncInterval * 60 * 1000);
			_tmrSync.Enabled = Session.Settings.enableAutoSync;
		}

		void Settings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
			//Lets get notified when the Interval changed
			if (e.PropertyName == "enableAutoSync" || e.PropertyName == "autoSyncInterval")
				setupSyncTimer();
		}
	}
}