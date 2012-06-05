using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace wunderbar.Api.Extensions {
	public static class DateTimeExtensions {

		public static DateTime FromUnixTimeStamp(this DateTime dt, long? timestamp) {
			if (timestamp == null)
				timestamp = 0;

			var result = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds((long)timestamp).ToLocalTime();
			return result;
		}

		public static double ToUnixTimeStamp(this DateTime dt) {
			var baseDate = new DateTime(1970,1,1,0,0,0,DateTimeKind.Utc);
			var localDate = new DateTime(dt.Year, dt.Month, dt.Day, 0, 0, 0, DateTimeKind.Local);
			var diff = localDate.ToUniversalTime() - baseDate;
			return diff.TotalSeconds;
		}

		/// <summary>Converts the specified DateTime to its relative date.</summary>
		/// <param name="dateTime">The DateTime to convert.</param>
		/// <returns>A string value based on the relative date
		/// of the datetime as compared to the current date.</returns>
		public static string ToRelativeDate(this DateTime dateTime) {
			//var timeSpan = DateTime.Now - dateTime.Date;

			var timeSpan = dateTime.Subtract(DateTime.Now.Date);

			if (timeSpan.Days == 0)
				return "Today";

			if (timeSpan.Days > 0 && timeSpan.Days < 30)
				return timeSpan.Days == 1 ? "Tomorrow" : "in " + Math.Abs(timeSpan.Days) + " days";
			if (timeSpan.Days < 0 && timeSpan.Days > -30)
				return timeSpan.Days == -1 ? "Yesterday" : Math.Abs(timeSpan.Days) + " days ago";

			return dateTime.ToLongDateString();
		}

	}
}