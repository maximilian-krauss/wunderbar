using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace wunderbar.Api.Extensions {
	public static class DateTimeExtensions {

		public static DateTime FromUnixTimeStamp (this DateTime dt, long timestamp) {
			DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Local);
			origin = origin.AddHours(TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).Hours); //This is important because the value provided by wunderlist is an UTC Date
			return origin.AddSeconds((long)timestamp);
		}

		public static double ToUnixTimeStamp(this DateTime dt) {
			DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
			TimeSpan diff = dt - origin;
			return Math.Floor((diff.TotalSeconds - TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).Seconds));
		}

	}
}
