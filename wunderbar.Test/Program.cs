﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using wunderbar.Api;
using wunderbar.Api.dataContracts;
using System.IO;

namespace wunderbar.Test {
	class Program {
		static void Main(string[] args) {

			var client = new wunderClient();
			client.localStorageDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "wunderbar");

			Console.WriteLine("client.Login");
			if(!client.Login(testCredentials.eMail,testCredentials.Password))
				Console.WriteLine("Login failed!");

			
			//Console.WriteLine(client.Login() ? "Success" : "Fail");

			//Grab all Tasks and Lists
			Console.WriteLine("client.Synchronize");
			client.Synchronize();

			//Add a Testtask
			/*client.Tasks.Add(new taskType {
				listId = 106437,
				userId = 29253,
				Deleted = 0,
				Done = 0,
				Important = 1,
				Name = "Hi there little boggers!",
				Id = 0,
				Version = 0,
				Note = ""
			});*/

			//Modify Task
			var task = client.Tasks.First(t => t.Done == 0 && t.Deleted == 0);
			task.Name = task.Name + " Muh!";
			task.Version++;

			//Synchronize changes
			client.Synchronize();

			Console.ReadKey();
		}
	}
}