using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.ComponentModel;
using wunderbar.Api.httpClientAttributes;

namespace wunderbar.Api.dataContracts {

	[DataContract]
	public abstract class dataBaseType : INotifyPropertyChanged {

		private int _deleted;
		private string _name;
		private int _id;
		private int _version;
		private int _position;
		private int _userId;
		private bool _onUpdate;
		private bool _trackChanges;

		[DataMember(Name = "deleted")]
		public int Deleted { get { return _deleted; } set { _deleted = value; onPropertyChanged("Deleted"); } }

		[DataMember(Name = "name")]
		public string Name { get { return _name; } set { _name = value; onPropertyChanged("Name"); } }

		[DataMember(Name = "online_id")]
		public int Id { get { return _id; } set { _id = value; onPropertyChanged("Id"); } }

		[DataMember(Name = "version")]
		public int Version { get { return _version; } set { _version = value; onPropertyChanged("Version"); } }

		[DataMember(Name = "position")]
		public int Position { get { return _position; } set { _position = value; onPropertyChanged("Position");} }

		[DataMember(Name = "user_id")]
		public int userId { get { return _userId; } set { _userId = value; onPropertyChanged("userId");} }

		#region INotifyPropertyChanged Members

		public event PropertyChangedEventHandler PropertyChanged;

		protected void onPropertyChanged(string propertyName) {

			//Increase the Version of the Object if a Value changes
			if (propertyName != "Version" && !_onUpdate && _trackChanges )
				_version++;

			var handler = PropertyChanged;
			if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
		}

		#endregion

		[IgnoreDataMember]
		[httpClientIgnorePropertyAttribute]
		public bool trackChanges { get { return _trackChanges; } set { _trackChanges = value; } }

		/// <summary>Call this method to prevent the Version from being Updated by changing Properties.</summary>
		public void beginUpdate() {
			_onUpdate = true;
		}

		/// <summary>Call this Method after beginUpdate() and Changed Properties to allow the incremention of the Version.</summary>
		public void endUpdate() {
			_onUpdate = false;
		}

	}
}