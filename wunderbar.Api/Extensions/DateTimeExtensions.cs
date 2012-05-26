using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace wunderbar.Api.Extensions {
	public static class DateTimeExtensions {

		public static DateTime FromUnixTimeStamp(this DateTime dt, long timestamp) {
			DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Local);
			origin = origin.AddHours(TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).Hours);
				//This is important because the value provided by wunderlist is an UTC Date
			return origin.AddSeconds((long) timestamp);
		}

		public static double ToUnixTimeStamp(this DateTime dt) {
			DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
			TimeSpan diff = dt - origin;
			return Math.Floor((diff.TotalSeconds - TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).Seconds));
		}

		/// <summary>Converts the specified DateTime to its relative date.</summary>
		/// <param name="dateTime">The DateTime to convert.</param>
		/// <returns>A string value based on the relative date
		/// of the datetime as compared to the current date.</returns>
		public static string ToRelativeDate(this DateTime dateTime) {
			//var timeSpan = DateTime.Now - dateTime.Date;

			var timeSpan = dateTime.Subtract(DateTime.Now);

			if (timeSpan.Days == 0)
				return "Today";

			if (timeSpan.Days > 0 && timeSpan.Days < 30)
				return timeSpan.Days == 1 ? "Tomorrow" : "in " + timeSpan.Days + " days";
			if (timeSpan.Days < 0 && timeSpan.Days > -30)
				return timeSpan.Days == -1 ? "Yesterday" : timeSpan.Days + " days ago";

			/*if (timeSpan.Days > 0) {
				// span is less than or equal to 30 days (1 month), measure in days.
				if (timeSpan <= TimeSpan.FromDays(30)) {
					return timeSpan.Days > 1
					       	? timeSpan.Days + " days ago"
					       	: "Yesterday";
				}
				// span is less than or equal to 365 days (1 year), measure in months.
				if (timeSpan <= TimeSpan.FromDays(365)) {
					return timeSpan.Days > 30
					       	? timeSpan.Days/30 + " months ago"
					       	: "a month ago";
				}
			}
			else {
				// span is less than or equal to 30 days (1 month), measure in days.
				if (timeSpan <= TimeSpan.FromDays(-30)) {
					return timeSpan.Days > 1
							? "in " + timeSpan.Days + " days"
							: "Tomorrow";
				}
			}*/
			return dateTime.ToLongDateString();
		}

	}
}
