using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Windows;
using wunderbar.Api;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using wunderbar.App.Data;
using wunderbar.Api.dataContracts;
using wunderbar.App.Ui.Dialogs;

namespace wunderbar.App.Core {
	internal sealed class applicationSession : IDisposable {
		private const string _portableIdentifier = "portable";
		
		public applicationSession(Window owner) {
			mainWindow = owner;
			Settings = applicationSettings.Load(this);
			wunderClient = new wunderClient {
			                                	localStorageDirectory = applicationDataStorageDirectory
			                                };

			//This should initialized at last...
			trayController = new trayController(this);
		}

		public event EventHandler trayContextUpdateRequired;

		/// <summary>Accesses the Wunderlist-Client for Tasks and Lists.</summary>
		public wunderClient wunderClient { get; private set; }

		/// <summary>Gets Access to the TrayIcon for Updating the NotifyIcon and displaying some Textbubbles.</summary>
		internal trayController trayController { get; private set; }

		/// <summary>Returns the MainWindow-Dummy which handles the TrayIconstuff.</summary>
		public Window mainWindow { get; private set; }

		/// <summary>Gets access to all Applicationspecific Settings.</summary>
		public applicationSettings Settings { get; private set; }

		/// <summary>Returns the friendly Applicationname.</summary>
		public string applicationName { get { return "wunderbar"; } }

		/// <summary>Returns the current Applicationversion.</summary>
		public Version applicationVersion { get { return Assembly.GetExecutingAssembly().GetName().Version; } }

		/// <summary>Returns an Option that indicates which Items from the ContextMenu should be displayed.</summary>
		internal trayContextTypes trayContextType{
			get {
				if (!wunderClient.loggedIn)
					return trayContextTypes.loginRequired;
				
				return trayContextTypes.signedIn;
			}
		}

		/// <summary>Returns the Directory which is used for storing all Application related data. This can be either the Users AppData Folder or the Applicationpath if the App should run in Portable Mode.</summary>
		public string applicationDataStorageDirectory {
			get {
				string baseDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
				if (File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _portableIdentifier)))
					baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

				return Path.Combine(baseDirectory, applicationName, applicationVersion.Major.ToString(CultureInfo.InvariantCulture));
			}
		}

		public void runApplication() {
			if (!string.IsNullOrWhiteSpace(Settings.eMail) && !string.IsNullOrWhiteSpace(Settings.Password))
				Login(new digestCredentials {eMail = Settings.eMail, Password = Settings.Password});
		}

		/// <summary>Closes and Disposes all Applicationmodules.</summary>
		public void closeApplication() {
			Settings.Save();
			mainWindow.Close();
		}

		/// <summary>Displays the Logindialog and tries to Login to Wunderlist.</summary>
		public void Login() {
			var login = new loginDialog();
			login.ShowDialog();
			if (login.DialogResult.HasValue && login.DialogResult.Value)
				Login(login.Credentials);
		}

		/// <summary>Tries to Login to Wunderlist with the Credentials provided by the credentials-param.</summary>
		public void Login(digestCredentials credentials) {
			
			//Perform login
			trayController.startAnimation();
			var loginTask = Task.Factory.StartNew(() => {
			                                      	Thread.Sleep(5000);
			                                      	if (!wunderClient.Login(credentials.eMail, credentials.Password))
			                                      		throw new Exception("Login failed");
			                                      });
			try {
				loginTask.Wait();
			}
			catch (Exception exc) {
				//TODO: Notify and Log error
				onTrayContextUpdateRequired(EventArgs.Empty);
				trayController.stopAnimation();
			}

			//Save Credentials
			Settings.eMail = credentials.eMail;
			Settings.Password = credentials.Password;

			//Execute synchronize. If the Login was not successfull, synchronizazon won't do anything (except stopping the animation)
			Synchronize();
		}

		/// <summary>Clears cached Tasks and Lists and removes any saved Credentials.</summary>
		public void Logout() {
			if(wunderClient.loggedIn)
				wunderClient.removeLocalStorage();

			Settings.Password = string.Empty;
			Settings.eMail = string.Empty;
			Settings.Save();
			onTrayContextUpdateRequired(EventArgs.Empty);
		}

		/// <summary>Performs the whole Synchronization from Tasks and Lists.</summary>
		public void Synchronize() {
			try {
				if (!wunderClient.loggedIn)
					return;

				trayController.startAnimation();

				//Synchronize Tasks and Lists
				var syncTask = Task.Factory.StartNew(wunderClient.Synchronize);
				try {
					syncTask.Wait();
				}
				catch (Exception exc) {
					//TODO: Notify and Log error
					onTrayContextUpdateRequired(EventArgs.Empty);
				}
			}
			finally { //Make sure that the Trayanimation will always stop
				trayController.stopAnimation();
				onTrayContextUpdateRequired(EventArgs.Empty);
			}
		}

		public void showTask(int listId) {
			showTask(new taskType {listId = listId, Name = "Enter a Name for the new Task"});
		}
		public void showTask(taskType task) {
			var dialog = new taskDialog {ListsItemSource = wunderClient.Lists, DataContext = task};
			dialog.ShowDialog();

			//If this Task already exists on the Server, we have to increase the Version.
			if (task.Id > 0)
				task.Version++;

			onTrayContextUpdateRequired(EventArgs.Empty);
		}

		#region Event Invocations

		public void onTrayContextUpdateRequired(EventArgs e) {
			EventHandler handler = trayContextUpdateRequired;
			if (handler != null) handler(this, e);
		}

		#endregion

		#region IDisposable Members

		public void Dispose() {
			trayController.Dispose();
			wunderClient.Dispose();
		}

		#endregion
	}
}