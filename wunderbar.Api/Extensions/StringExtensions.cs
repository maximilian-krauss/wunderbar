using System.Text;
using System.Security.Cryptography;
using System.IO;
using System;
using System.Text.RegularExpressions;

namespace wunderbar.Api.Extensions {
	public static class StringExtensions {
		private static readonly byte[] _iv = new byte[] {0x19, 0x46, 0x63, 0x6f, 0x20, 0x4d, 0xff, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76};

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

		public static bool isEmail(this string text) {
			return Regex.IsMatch(text, @"^([A-Za-z0-9\+_\-\.\+])+\@([A-Za-z0-9_\-\.])+\.([A-Za-z]{2,4})$");
		}

		public static string aesEncrypt(this string value, byte[] key) {
			if (string.IsNullOrWhiteSpace(value))
				return string.Empty;

			byte[] plaintext = Encoding.UTF8.GetBytes(value);
			var pdb = new Rfc2898DeriveBytes(key, _iv, 20);
			var algo = Aes.Create();
			algo.Key = pdb.GetBytes(32);
			algo.IV = pdb.GetBytes(16);

			using (var msCrypto = new MemoryStream()) {
				using (var cryptoStream = new CryptoStream(msCrypto, algo.CreateEncryptor(), CryptoStreamMode.Write)) {
					cryptoStream.Write(plaintext, 0, plaintext.Length);
				}
				return Convert.ToBase64String(msCrypto.ToArray(), Base64FormattingOptions.None);
			}
		}

		public static string aesDecrypt(this string value, byte[] key) {
			if (string.IsNullOrWhiteSpace(value))
				return string.Empty;

			byte[] encryptedData = Convert.FromBase64String(value);
			var pdb = new Rfc2898DeriveBytes(key, _iv, 20);
			var algo = Aes.Create();
			algo.Key = pdb.GetBytes(32);
			algo.IV = pdb.GetBytes(16);

			using (var msCrypto = new MemoryStream()) {
				using (var cryptoStream = new CryptoStream(msCrypto, algo.CreateDecryptor(), CryptoStreamMode.Write)) {
					cryptoStream.Write(encryptedData, 0, encryptedData.Length);
				}
				return Encoding.UTF8.GetString(msCrypto.ToArray());
			}
		}

	}
}
