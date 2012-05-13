using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using wunderbar.Api;

namespace wunderbar.Test {
	class Program {
		static void Main(string[] args) {

			var client = new wunderClient {
			                              	Credentials = new digestCredentials {eMail = testCredentials.eMail, Password = testCredentials.Password}
			                              };
			
			//Console.WriteLine("Test Login:");
			//Console.WriteLine(client.Login() ? "Success" : "Fail");

			Console.WriteLine("Sync");
			client.Synchronize();

			Console.ReadKey();
		}
	}
}