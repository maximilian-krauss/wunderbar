using System.Text;
using System.Security.Cryptography;

namespace wunderbar.Api.Extensions {
	public static class StringExtensions {

		public static string generateMD5Hash(this string value) {
			var textBytes = Encoding.Default.GetBytes(value);
			var cryptHandler = new MD5CryptoServiceProvider();
			var hash = cryptHandler.ComputeHash(textBytes);
			var ret = "";
			foreach (var a in hash) {
				if (a < 16)
					ret += "0" + a.ToString("x");
				else
					ret += a.ToString("x");
			}
			return ret;
		}

	}
}
