using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using NLog;
using NLog.Config;
using NLog.Targets;
using wunderbar.Api;
using wunderbar.Api.dataContracts;
using wunderbar.Api.Extensions;
using wunderbar.App.Data;
using wunderbar.App.Ui.Dialogs;
using System.Windows.Interop;
using wunderbar.App.Ui.FlyoutViews;

namespace wunderbar.App.Core {
	public sealed class applicationSession : IDisposable {
		private const string _portableIdentifier = "portable";
		private const string _newTaskText = "<Enter a name for your task>";
		private const int _randomMax = -10000;
		private const int _randomMin = -99999;
		private flyoutWindow _flyoutWindow;

		private LoggingConfiguration _loggingConfiguration;
		private readonly Random _random;

		//Hotkeys
		private hotKey _htkAddTask;
		private hotKey _htkSync;
		
		public applicationSession(Window owner) {
			mainWindow = owner;
			initializeLogger();
			_random = new Random(Environment.TickCount);

			//Trigger Exceptionhandler
			Application.Current.Dispatcher.UnhandledException += Dispatcher_UnhandledException;
			AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

			Settings = applicationSettings.Load(this);
			wunderClient = new wunderClient {
			                                	localStorageDirectory = applicationDataStorageDirectory,
												enforceSSLSecurity = Settings.enforceSSLSecurity
			                                };
			wunderClient.httpRequestCreated += wunderClient_httpRequestCreated;

			//This should initialized at last...
			syncController = new syncController(this);
			trayController = new trayController(this);

			//Refresh hotkey settings if their properties changed
			Settings.PropertyChanged += (o, e) => {
			                            	if (e.PropertyName == "hotkeyNewTasks" || e.PropertyName == "hotkeySync")
			                            		setupHotKeys();
			                            };
		}

		public event EventHandler trayContextUpdateRequired;

		/// <summary>Accesses the Wunderlist-Client for Tasks and Lists.</summary>
		public wunderClient wunderClient { get; private set; }

		/// <summary>Gets Access to the TrayIcon for Updating the NotifyIcon and displaying some Textbubbles.</summary>
		internal trayController trayController { get; private set; }

		internal syncController syncController { get; private set; }

		/// <summary>Returns the MainWindow-Dummy which handles the TrayIconstuff.</summary>
		public Window mainWindow { get; private set; }

		/// <summary>Gets access to all Applicationspecific Settings.</summary>
		public applicationSettings Settings { get; private set; }

		/// <summary>Returns the friendly Applicationname.</summary>
		public string applicationName { get { return "wunderbar"; } }

		/// <summary>Returns the current Applicationversion.</summary>
		public Version applicationVersion { get { return Assembly.GetExecutingAssembly().GetName().Version; } }

		public string displayVersion { get { return string.Format("{0} beta 6", applicationVersion); } }

		/// <summary>Returns the last error which occoured while logging in or syncing.</summary>
		public Exception lastError { get; private set; }

		public Logger Logger { get; private set; }

		/// <summary>Gets access to the global hotkey manager.</summary>
		public hotKeyHost hotKeys { get; private set; }

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

			//Show the use what he need to do when he openend the application
			if (!wunderClient.loggedIn && string.IsNullOrWhiteSpace(Settings.eMail) && string.IsNullOrWhiteSpace(Settings.Password)) {
				trayController.notifyInformation("Hi there! Looks like you didn't log in yet.\r\nJust click on this icon to open the logindialog.","wunderbar waits for you!");
			}

			//Because the hotKeyHost needs an active windowhandle it is important to initialize this when the (hidden) mainwindow is fully loaded and called this method
			setupHotKeys();
		}

		/// <summary>Closes and Disposes all Applicationmodules.</summary>
		public void closeApplication() {
			Settings.Save();
			mainWindow.Close();
		}

		/// <summary>Displays the Logindialog and tries to Login to Wunderlist.</summary>
		public void Login() {
			showFlyout(new LoginView(), null);
		}

		/// <summary>Tries to Login to Wunderlist with the Credentials provided by the credentials-param.</summary>
		public void Login(digestCredentials credentials) {
			
			//Perform login
			lastError = null;
			trayController.startAnimation();
			var loginTask = Task.Factory.StartNew(() => {
			                                      	if (!wunderClient.Login(credentials.eMail, credentials.Password))
			                                      		throw new Exception("Login failed. Username and or Password are invalid.");
			                                      });
			try {
				loginTask.Wait();
			}
			catch (Exception exc) {
				//TODO: Too redundant, see catch-block in Sychronize()
				Logger.WarnException("Login failed.", exc);
				lastError = exc.LowestException();
				onTrayContextUpdateRequired(EventArgs.Empty);
				trayController.stopAnimation();
				trayController.notifyError(string.Format("There was an error while logging into Wunderlist:\r\n{0}", lastError.Message));
			}

			//Execute synchronize.
			if (wunderClient.loggedIn) {
				//Save Credentials
				Settings.eMail = credentials.eMail;
				Settings.Password = credentials.Password;

				Synchronize();
			}
		}

		/// <summary>Clears cached Tasks and Lists and removes any saved Credentials.</summary>
		public void Logout() {
			if(wunderClient.loggedIn)
				wunderClient.Logout();

			Settings.Password = string.Empty;
			Settings.eMail = string.Empty;
			Settings.Save();
			onTrayContextUpdateRequired(EventArgs.Empty);
		}

		/// <summary>Performs the whole Synchronization from Tasks and Lists.</summary>
		public void Synchronize() {
			lastError = null;
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
					lastError = exc.LowestException();
					onTrayContextUpdateRequired(EventArgs.Empty);
					trayController.notifyError(string.Format("There was an problem while syncing your tasks and lists:\r\n{0}",
					                                         lastError.Message));
				}
			}
			finally { //Make sure that the Trayanimation will always stop
				trayController.stopAnimation();
				onTrayContextUpdateRequired(EventArgs.Empty);
			}
		}

		public void showTask(int listId) {
			showTask(new taskType {
									/*
									 Because this is a new task, we have to set an custom id.
									 And to seperate the newly created tasks from the synced one,
									 we set the Id to a negative value.
									 */
									Id = _random.Next(_randomMin, _randomMax),
			                      	listId = listId,
									Name = _newTaskText,
									userId = wunderClient.userId
			                      });
		}
		public void showTask(taskType task) {
			showFlyout(new TaskView(), task);
		}

		public void showFlyout() {
			showFlyout(null,null);
		}
		public void showFlyout(IView view, object argument) {
			if (_flyoutWindow != null) {
				_flyoutWindow.BringIntoView();
				_flyoutWindow.Activate();
				return;
			}
			_flyoutWindow = new flyoutWindow(this, view, argument);
			_flyoutWindow.Closed += (o, e) => {
			                        	_flyoutWindow = null;
			                        	onTrayContextUpdateRequired(EventArgs.Empty);
			                        };
			//Make sure the window is displayed
			_flyoutWindow.Show();
			_flyoutWindow.BringIntoView();
			_flyoutWindow.Activate();
		}

		public taskType createTaskFromString(string taskString) {
			return createTaskFromString(taskString, wunderClient.Lists.Inbox.Id);
		}
		public taskType createTaskFromString(string taskString, int listId) {
			if (string.IsNullOrEmpty(taskString))
				return null;

			var task = new taskType {
				listId = listId,
				Id = new Random(Environment.TickCount).Next(-99999, -100),
				userId = wunderClient.userId
			};

			//check if the task should be prioritized
			if (taskString.StartsWith("*") || taskString.StartsWith("!")) {
				task.Important = 1;
				taskString = taskString.Substring(1).Trim();
			}

			//try to identify target list with a hashtag the user maybe provided
			var hashtags = taskString.Split(new[] {" "}, StringSplitOptions.RemoveEmptyEntries).Where(w => w.StartsWith("#"));
			foreach (var tag in hashtags) {
				var list = wunderClient.Lists.FirstOrDefault(l => l.Name.ToLowerInvariant() == tag.Replace("#", ""));
				if (list == null) continue;
				
				task.listId = list.Id;
				taskString = taskString
					.Replace(tag, "")
					.Trim();
				break;
			}

			var knownDates = new Dictionary<string, DateTime> { { "today", DateTime.Now.Date }, { "tomorrow", DateTime.Now.AddDays(1).Date } };
			if (taskString.Contains(" ")) {
				var lastWord = taskString.Substring(taskString.LastIndexOf(' ') + 1).ToLowerInvariant();
				if (knownDates.ContainsKey(lastWord)) {
					task.dueDate = knownDates[lastWord];
					taskString = taskString.Substring(0, taskString.LastIndexOf(' ')).Trim();
				}
			}

			task.Name = taskString;
			return task;
		}

		void wunderClient_httpRequestCreated(object sender, httpRequestCreatedEventArgs e) {
			if (Settings.useNtlmProxyAuthentication && e.Request.Proxy != null)
				e.Request.Proxy.Credentials = CredentialCache.DefaultNetworkCredentials;
		}

		private void initializeLogger() {
			_loggingConfiguration = new LoggingConfiguration();

			var fTarget = new FileTarget {
											Header = "" ,
			                             	Layout = "[${longdate}] [${logger}] [${level}]: ${message} ${exception} ${stacktrace}",
											FileName = // Do not use string.Format, it will crash!
			                             		Path.Combine(applicationDataStorageDirectory, "Logs", applicationName + "_${shortdate}.log")
			                             };
			_loggingConfiguration.AddTarget("file", fTarget);

			var mainRule = new LoggingRule("*", LogLevel.Debug, fTarget);
			_loggingConfiguration.LoggingRules.Add(mainRule);

			LogManager.Configuration = _loggingConfiguration;
			Logger = LogManager.GetLogger("Base");
		}

		private void setupHotKeys() {
			//Initialize the Host if this is the first time we call this method
			if (hotKeys == null)
				hotKeys = new hotKeyHost((HwndSource)PresentationSource.FromVisual(Application.Current.MainWindow));

			//Hotkeys
			if (_htkAddTask == null) {
				_htkAddTask = new hotKey(Key.T, ModifierKeys.Control | ModifierKeys.Alt);
				_htkAddTask.HotKeyPressed += (o, e) => {
				                             	if (wunderClient.loggedIn)
				                             		showTask(wunderClient.Lists.Inbox.Id);
				                             };
				try {
					hotKeys.AddHotKey(_htkAddTask);
				}
				catch (hotKeyAlreadyRegisteredException exc) {
					Logger.WarnException("Unable to register hotkey \"CTRL+ALT+T\".", exc);
				}
			}
			if (_htkSync == null) {
				_htkSync = new hotKey(Key.S, ModifierKeys.Control | ModifierKeys.Alt);
				_htkSync.HotKeyPressed += (o, e) => Synchronize();
				try {
					hotKeys.AddHotKey(_htkSync);
				}
				catch (hotKeyAlreadyRegisteredException exc) {
					Logger.WarnException("Unable to register hotkey \"CTRL+ALT+S\".", exc);
				}
			}
			_htkAddTask.Enabled = Settings.hotkeyNewTasks;
			_htkSync.Enabled = Settings.hotkeySync;
		}

		void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e) {
			showExceptionDialog((Exception)e.ExceptionObject);
		}
		void Dispatcher_UnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e) {
			showExceptionDialog(e.Exception);
			e.Handled = true;
		}
		void showExceptionDialog(Exception exc) {
			Logger.FatalException("Unhandled exception cought!", exc);
			var dialog = new exceptionDialog { DataContext = exc };
			dialog.ShowDialog();
			if (mainWindow != null)
				mainWindow.Close();
		}

		#region Event Invocations

		public void onTrayContextUpdateRequired(EventArgs e) {
			EventHandler handler = trayContextUpdateRequired;
			if (handler != null) handler(this, e);
		}

		#endregion

		#region IDisposable Members

		public void Dispose() {
			//Important as hell to dispose all the hotkey stuff
			hotKeys.Dispose();
			trayController.Dispose();
			wunderClient.Dispose();
		}

		#endregion
	}
}
