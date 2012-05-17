using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using wunderbar.App.Core;
using System.Xml.Serialization;
using System.IO;

namespace wunderbar.App.Data {
	public sealed class applicationSettings : baseModel {
		private const string _settingsFilename = "wunderbar.App.Settings.xml";

		/// <summary>Gets or Sets the eMail-Address for Wunderlist.</summary>
		public string eMail { get; set; }

		/// <summary>Gets or Sets the Wunderlist Password.</summary>
		public string Password { get; set; }

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
				return instance;
			}
		}
		public void Save() {
			if (!Directory.Exists(Session.applicationDataStorageDirectory))
				Directory.CreateDirectory(Session.applicationDataStorageDirectory);

			using (var writer = new StreamWriter(Path.Combine(Session.applicationDataStorageDirectory, _settingsFilename), false, Encoding.UTF8)) {
				var serializer = new XmlSerializer(typeof (applicationSettings));
				serializer.Serialize(writer, this);
			}
		}

	}
}
