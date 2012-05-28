using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Net;
using System.Runtime.Serialization;
using wunderbar.Api.Extensions;
using wunderbar.Api.Requests;
using wunderbar.Api.Responses;
using System.Runtime.Serialization.Json;
using wunderbar.Api.dataContracts;

namespace wunderbar.Api {
	public sealed class wunderClient : IDisposable {
		private readonly httpClient _httpClient;
		private readonly digestCredentials _credentials;

		private const string _lsTasksFilename = "Tasks.json";
		private const string _lsListsFilename = "Lists.json";

		private bool _loggedIn;

		public wunderClient() {
			_credentials = new digestCredentials();
			_httpClient = new httpClient();
			Lists = new listCollection();
			Tasks = new taskCollection();

			_loggedIn = false;
		}

		/// <summary>Returns all Lists from the Wunderlist-Account.</summary>
		public listCollection Lists { get; private set; }

		/// <summary>Returns all Tasks from the Wunderlist-Account.</summary>
		public taskCollection Tasks { get; private set; }

		/// <summary>Gets or Sets the Directory in which the Tasks and Lists should be cached.</summary>
		public string localStorageDirectory { get; set; }

		/// <summary>Returns if the User is logged in.</summary>
		public bool loggedIn { get { return _loggedIn; } }

		/// <summary>Tries to Login to Wunderlist.</summary>
		/// <param name="email">Your E-Mailaddress.</param>
		/// <param name="password">Your Password.</param>
		/// <returns>Returns true if the Login was successfull, otherwise false.</returns>
		public bool Login(string email, string password) {
			_credentials.eMail = email;
			_credentials.Password = password;

			var result = _httpClient.httpPost<loginRequest, baseResponse>(new loginRequest {
			                                                                               	eMail = _credentials.eMail,
			                                                                               	Password = _credentials.Password
			                                                                               });
			_loggedIn = (result.statusCode == statusCodes.LOGIN_SUCCESS);
			if(_loggedIn)
				readLocalStorage();

			return _loggedIn;
		}

		/// <summary>Clears the local Storage from the currently logged in User.</summary>
		public void removeLocalStorage() {
			if(!_loggedIn)
				throw new InvalidOperationException("You need to Log-In before you can purge any Data.");

			if(Directory.Exists(localStoragePath))
				Directory.Delete(localStoragePath, true);

			_loggedIn = false;
		}

		/// <summary>Synchronizes the LocalStorage with the Wunderlist-Servers.</summary>
		public void Synchronize() {

			if (!_loggedIn)
				throw new InvalidOperationException(
					"You need to call Login(email, password) first before you can Start Synchronizing.");

			/*
				Synchronization Step 1
			 */
			var step1Request = new syncStep1Request {
			                                        	eMail = _credentials.eMail,
			                                        	Password = _credentials.Password
			                                        };
			step1Request.syncTable.Lists.AddRange(Lists.Where(l => l.Id > 0));
			step1Request.syncTable.Tasks.AddRange(Tasks.Where(t => t.Id > 0));
			step1Request.syncTable.newLists.AddRange(Lists.Where(l => l.Id == 0));

			var step1Result = _httpClient.httpPost<syncStep1Request, syncStep1Response>(step1Request);
			if (step1Result.statusCode != statusCodes.SYNC_SUCCESS)
				throw new synchronizationException(step1Request.Step, step1Result.statusCode);

			//Add new Lists and Tasks
			if (step1Result.syncTable != null && step1Result.syncTable.newLists != null)
				step1Result.syncTable.newLists.ForEach(l => Lists.addOrUpdateList(l));

			if (step1Result.syncTable != null && step1Result.syncTable.newTasks != null)
				step1Result.syncTable.newTasks.ForEach(t => Tasks.addOrUpdateTask(t));

			if (step1Result.syncTable != null && step1Result.syncTable.syncedLists != null &&
			    step1Result.syncTable.syncedLists.Count > 0)
				for (int i = 0; i < step1Result.syncTable.syncedLists.Count; i++)
					step1Request.syncTable.newLists[i].Id = step1Result.syncTable.syncedLists[i].Id;


			/*
				Synchronization Step 2
			 */
			var step2Request = new syncStep2Request {
			                                        	eMail = _credentials.eMail,
			                                        	Password = _credentials.Password
			                                        };
			step2Request.syncTable.newTasks.AddRange(Tasks.Where(t => t.Id == 0));

			if (step1Result.syncTable != null && step1Result.syncTable.requiredTasks != null)
				step2Request.syncTable.requiredTasks.AddRange(
					from task in Tasks
					where step1Result.syncTable.requiredTasks.Any(requiredTask => requiredTask == task.Id)
					select task
					);

			if (step1Result.syncTable != null && step1Result.syncTable.requiredLists != null)
				step2Request.syncTable.requiredLists.AddRange(
					from list in Lists
					where step1Result.syncTable.requiredLists.Any(requiredList => requiredList == list.Id)
					select list
					);

			var step2Result = _httpClient.httpPost<syncStep2Request, syncStep2Response>(step2Request);
			if (step2Result.statusCode != statusCodes.SYNC_SUCCESS)
				throw new synchronizationException(step2Request.Step, step2Result.statusCode);

			if (step2Result.syncTable != null && step2Result.syncTable.syncedTasks != null &&
			    step2Result.syncTable.syncedTasks.Count > 0)
				for (int i = 0; i < step2Result.syncTable.syncedTasks.Count; i++)
					step2Request.syncTable.newTasks[i].Id = step2Result.syncTable.syncedTasks[i].Id;

			//Seems like everything worked, save the Tasks and Lists locally
			writeLocalStorage();
		}

		/// <summary>Writes Lists and Tasks to the LocalStorage in the FileSystem.</summary>
		private void writeLocalStorage() {

			//If there is no StorageLocation we can't load anything...
			if (string.IsNullOrWhiteSpace(localStorageDirectory))
				return;

			string lsDirectory = localStoragePath;
			if (!Directory.Exists(lsDirectory))
				Directory.CreateDirectory(lsDirectory);

			serializeJson<taskCollection>(Path.Combine(lsDirectory, _lsTasksFilename), Tasks);
			serializeJson<listCollection>(Path.Combine(lsDirectory, _lsListsFilename), Lists);
		}

		/// <summary>Reads previously saved Lists and Tasks.</summary>
		private void readLocalStorage() {

			//No Storagelocation given
			if (string.IsNullOrWhiteSpace(localStorageDirectory))
				return;

			string lsDirectory = localStoragePath;
			if (!Directory.Exists(lsDirectory))
				return;

			string lsTasksPath = Path.Combine(lsDirectory, _lsTasksFilename);
			string lsListsPath = Path.Combine(lsDirectory, _lsListsFilename);

			if (File.Exists(lsTasksPath))
				Tasks = deserializeJson<taskCollection>(lsTasksPath);
			if (File.Exists(lsListsPath))
				Lists = deserializeJson<listCollection>(lsListsPath);

			//Activate Changetracking
			Tasks.ForEach(t => t.trackChanges = true);
			Lists.ForEach(l => l.trackChanges = true);

		}

		private string localStoragePath {
			get { return Path.Combine(localStorageDirectory, _credentials.eMail.ToLowerInvariant().generateMD5Hash()); }
		}

		private void serializeJson<T>(string path, T graph) {
			using (var fStream = new FileStream(path, FileMode.Create)) {
				var serializer = new DataContractJsonSerializer(typeof (T));
				serializer.WriteObject(fStream, graph);
			}
		}

		private T deserializeJson<T>(string path) {
			using (var fStream = File.OpenRead(path)) {
				var serializer = new DataContractJsonSerializer(typeof (T));
				return (T) serializer.ReadObject(fStream);
			}
		}

		#region IDisposable Members

		public void Dispose() {
			_credentials.Password = string.Empty;
		}

		#endregion
	}
}