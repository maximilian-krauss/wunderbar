using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using wunderbar.Api.Extensions;
using wunderbar.Api.httpClientAttributes;

namespace wunderbar.Api.dataContracts {

	[DataContract]
	public sealed class taskType : dataBaseType {
		private long _date;
		private int _done;
		private long _doneDate;
		private int _important;
		private int _listId;
		private string _note;
		private int _push;
		private int _pushTS;

		[DataMember(Name = "date")]
		public long Date { get { return _date; } set { _date = value; onPropertyChanged("Date"); } }

		[DataMember(Name = "done")]
		public int Done { get { return _done; } set { _done = value; onPropertyChanged("Done"); } }

		[DataMember(Name = "done_date")]
		public long doneDate { get { return _doneDate; } set { _doneDate = value; onPropertyChanged("doneDate"); } }

		[DataMember(Name = "important")]
		public int Important { get { return _important; } set { _important = value; onPropertyChanged("Important"); } }

		[DataMember(Name = "list_id")]
		public int listId { get { return _listId; } set { _listId = value; onPropertyChanged("listId"); } }

		[DataMember(Name = "note")]
		public string Note { get { return _note; } set { _note = value; onPropertyChanged("Note"); } }

		[DataMember(Name = "push")]
		public int Push { get { return _push; } set { _push = value; onPropertyChanged("Push"); } }

		[DataMember(Name = "push_ts")]
		public int pushTS { get { return _pushTS; } set { _pushTS = value; onPropertyChanged("pushTS"); } } //TODO: Find out what that property does

		[IgnoreDataMember]
		[httpClientIgnoreProperty]
		public DateTime dueDate {
			get { return DateTime.Now.FromUnixTimeStamp(Date); }
			set { Date = (long) value.ToUnixTimeStamp(); }
		}

		[IgnoreDataMember]
		[httpClientIgnoreProperty]
		public bool isOverdue {
			get { return (Date > 0 && Done == 0 && dueDate.Date < DateTime.Now.Date); }
		}

		/// <summary>Postpones this task for one day.</summary>
		public void Postpone() {
			if (Date > 0) {
				Date = (long) DateTime.Now.AddDays(1).Date.ToUnixTimeStamp();
			}
		}

		/// <summary>Sets the flag "done".</summary>
		public void markAsDone() {
			Done = 1;
		}

	}
}
