using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using wunderbar.App.Core;
using wunderbar.Api.Extensions;
using System.Xml.Serialization;
using System.IO;
using System.Reflection;
using Microsoft.Win32;
using System.Windows;

namespace wunderbar.App.Data {
	public sealed class applicationSettings : baseModel, ICloneable {
		private const string _settingsFilename = "wunderbar.App.Settings.xml";
		private static readonly byte[] _token;
		private const int autoSyncMinimumValue = 5;

		private string _email;
		private string _password;
		private bool _enableAutoSync;
		private int _autoSyncInterval;
		private bool _showDueTasksInTrayIcon;
		private bool _useNtlmProxyAuthentication;
		private bool _showDueTasksOnTop;
		private bool _sortByDueDate;
		private bool _hotkeyNewTask;
		private bool _hotkeySync;
		private bool _enforceSSLSecurity;
		private double _flyoutHeight;
		private double _flyoutWidth;

		static applicationSettings() {
			_token = Assembly.GetExecutingAssembly().GetName().GetPublicKey();
		}

		//Screw you ReSharper, serialization needs an PUBLIC constructor
		public applicationSettings() {
			_enableAutoSync = true;
			_autoSyncInterval = 5;
			_showDueTasksInTrayIcon = true;
			_showDueTasksOnTop = true;
			_enforceSSLSecurity = true;
			_flyoutHeight = 350;
			_flyoutWidth = 300;
		}

		/// <summary>Gets or sets the eMail-Address for Wunderlist.</summary>
		public string eMail { get { return _email; } set { _email = value; onPropertyChanged("eMail"); } }

		/// <summary>Gets or sets the Wunderlist password.</summary>
		public string Password { get { return _password; } set { _password = value; onPropertyChanged("Password"); } }

		/// <summary>Gets or sets whether tasks and lists should synchronize in background or not.</summary>
		public bool enableAutoSync { get { return _enableAutoSync; } set { _enableAutoSync = value; onPropertyChanged("enableAutoSync"); } }

		/// <summary>Gets or sets the sync-interval in minutes.</summary>
		public int autoSyncInterval { get { return _autoSyncInterval; } set { _autoSyncInterval = value; onPropertyChanged("autoSyncInterval"); } }

		/// <summary>Gets or sets whether due tasks should appear inside the trayicon or not.</summary>
		public bool showDueTasksInTrayIcon { get { return _showDueTasksInTrayIcon; } set { _showDueTasksInTrayIcon = value; onPropertyChanged("showDueTasksInTrayIcon"); } }

		/// <summary>Gets or sets whether HTTP-Requets should use NTLM-auth or not. (Required for auth on ISA-Servers)</summary>
		public bool useNtlmProxyAuthentication { get { return _useNtlmProxyAuthentication; } set { _useNtlmProxyAuthentication = value; onPropertyChanged("useNtlmProxyAuthentication"); } }

		/// <summary>Gets or sets whether due or overdue tasks should appear on top of the contextmenu.</summary>
		public bool showDueTasksOnTop { get { return _showDueTasksOnTop; } set { _showDueTasksOnTop = value; onPropertyChanged("showDueTasksOnTop"); } }

		/// <summary>Gets or sets whether task should sorted by due date or not</summary>
		public bool sortByDueDate { get { return _sortByDueDate; } set { _sortByDueDate = value; onPropertyChanged("sortByDueDate"); } }

		/// <summary>Gets or sets whether a global hotkey for new tasks should be registered or not.</summary>
		public bool hotkeyNewTasks { get { return _hotkeyNewTask; } set { _hotkeyNewTask = value; onPropertyChanged("hotkeyNewTasks"); } }

		/// <summary>Gets or sets whether a global hotkey for syncing should be registered or not.</summary>
		public bool hotkeySync { get { return _hotkeySync; } set { _hotkeySync = value; onPropertyChanged("hotkeySync"); } }

		/// <summary>Gets or sets whether the public key of the SSL-certificate should be verified or not.</summary>
		public bool enforceSSLSecurity { get { return _enforceSSLSecurity; } set { _enforceSSLSecurity = value; onPropertyChanged("enforceSSLSecurity"); } }

		public double FlyoutWidth { get { return _flyoutWidth; } set { _flyoutWidth = value; onPropertyChanged("FlyoutWidth"); } }
		public double FlyoutHeight { get { return _flyoutHeight; } set { _flyoutHeight = value; onPropertyChanged("FlyoutHeight"); } }

		/// <summary>Gets or sets whether wunderbar should run on startup or not.</summary>
		[XmlIgnore]
		public bool autoRun {
			get { return (string) runHive.GetValue(Session.applicationName, string.Empty) == Assembly.GetExecutingAssembly().Location; }
			set {
				if(value)
					runHive.SetValue(Session.applicationName, Assembly.GetExecutingAssembly().Location);
				else
					runHive.DeleteValue(Session.applicationName, false);
				
				onPropertyChanged("autoRun");
			}
		}

		private RegistryKey runHive {
			get { return Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true); }
		}

		[XmlIgnore] //the Property is Private but just for being save we don't want to serialize the applicationSession
		private applicationSession Session { get; set; }


		internal static applicationSettings Load(applicationSession session) {
			string settingsPath = Path.Combine(session.applicationDataStorageDirectory, _settingsFilename);
			if (!File.Exists(settingsPath))
				return new applicationSettings {Session = session};

			using (var reader = new StreamReader(settingsPath,Encoding.UTF8)) {
				var serializer = new XmlSerializer(typeof (applicationSettings));
				var instance = (applicationSettings) serializer.Deserialize(reader);
				instance.Session = session;
				instance.Password = instance.Password.aesDecrypt(_token);
				if (instance.autoSyncInterval < autoSyncMinimumValue)
					instance.autoSyncInterval = autoSyncMinimumValue;

				return instance;
			}
		}
		public void Save() {
			if (!Directory.Exists(Session.applicationDataStorageDirectory))
				Directory.CreateDirectory(Session.applicationDataStorageDirectory);

			using (var writer = new StreamWriter(Path.Combine(Session.applicationDataStorageDirectory, _settingsFilename), false, Encoding.UTF8)) {
				var serializer = new XmlSerializer(typeof (applicationSettings));
				var clone = Clone() as applicationSettings;
				clone.Password = clone.Password.aesEncrypt(_token);

				serializer.Serialize(writer, clone);
			}
		}


		#region ICloneable Members

		public object Clone() {
			return MemberwiseClone();
		}

		#endregion
	}
}
