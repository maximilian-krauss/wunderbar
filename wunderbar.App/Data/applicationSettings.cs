using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using wunderbar.App.Core;
using wunderbar.Api.Extensions;
using System.Xml.Serialization;
using System.IO;
using System.Reflection;

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

		static applicationSettings() {
			_token = Assembly.GetExecutingAssembly().GetName().GetPublicKey();
		}
		public applicationSettings() {
			_enableAutoSync = true;
			_autoSyncInterval = 5;
			_showDueTasksInTrayIcon = true;
		}

		/// <summary>Gets or Sets the eMail-Address for Wunderlist.</summary>
		public string eMail { get { return _email; } set { _email = value; onPropertyChanged("eMail"); } }

		/// <summary>Gets or Sets the Wunderlist Password.</summary>
		public string Password { get { return _password; } set { _password = value; onPropertyChanged("Password"); } }

		public bool enableAutoSync { get { return _enableAutoSync; } set { _enableAutoSync = value; onPropertyChanged("enableAutoSync"); } }

		public int autoSyncInterval { get { return _autoSyncInterval; } set { _autoSyncInterval = value; onPropertyChanged("autoSyncInterval"); } }

		public bool showDueTasksInTrayIcon { get { return _showDueTasksInTrayIcon; } set { _showDueTasksInTrayIcon = value; onPropertyChanged("showDueTasksInTrayIcon"); } }

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
