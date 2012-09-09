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
		private digestCredentials _credentials;

		private const string _lsTasksFilename = "Tasks.json";
		private const string _lsListsFilename = "Lists.json";

		private bool _loggedIn;

		public event EventHandler<httpRequestCreatedEventArgs> httpRequestCreated;

		public wunderClient() {
			_credentials = new digestCredentials();
			_httpClient = new httpClient();
			_httpClient.httpRequestCreated += (o, e) => onHttpRequestCreated(e);
			Lists = new listCollection();
			Tasks = new taskCollection();
			userId = -1;

			_loggedIn = false;
		}

		private void onHttpRequestCreated(httpRequestCreatedEventArgs e) {
			EventHandler<httpRequestCreatedEventArgs> handler = httpRequestCreated;
			if (handler != null) handler(this, e);
		}

		/// <summary>Returns all Lists from the Wunderlist-account.</summary>
		public listCollection Lists { get; private set; }

		/// <summary>Returns all Tasks from the Wunderlist-account.</summary>
		public taskCollection Tasks { get; private set; }

		/// <summary>Gets or sets the directory in which the tasks and lists should be cached.</summary>
		public string localStorageDirectory { get; set; }

		/// <summary>Gets or sets whether the public key of the SSL-certificate should be verified or not.</summary>
		public bool enforceSSLSecurity { get { return _httpClient.enforceSSLSecurity; } set { _httpClient.enforceSSLSecurity = value; } }

		/// <summary>Returns if the User is logged in.</summary>
		public bool loggedIn { get { return _loggedIn; } }

		/// <summary>Returns the serverside userid for the currently logged in user.</summary>
		public int userId { get; private set; }

		/// <summary>Tries to Login to Wunderlist.</summary>
		/// <param name="email">Your e-mailaddress.</param>
		/// <param name="password">Your password.</param>
		/// <returns>Returns true if the login was successfull, otherwise false.</returns>
		public bool Login(string email, string password) {
			_credentials.eMail = email;
			_credentials.Password = password;

			var result = _httpClient.httpPost<loginRequest, baseResponse>(new loginRequest {
			                                                                               	eMail = _credentials.eMail,
			                                                                               	Password = _credentials.Password
			                                                                               });
			_loggedIn = (result.statusCode == statusCodes.LOGIN_SUCCESS);
			if (_loggedIn) {
				readLocalStorage();
				userId = result.userId;
			}

			return _loggedIn;
		}

		/// <summary>Clears the local storage from the currently logged in user.</summary>
		public void Logout() {
			if(!_loggedIn)
				throw new wunderException("You need to log-in before you can purge any data.");

			if(Directory.Exists(localStoragePath))
				Directory.Delete(localStoragePath, true);

			_loggedIn = false;
			userId = -1;
			_credentials = new digestCredentials();
			Tasks.Clear();
			Lists.Clear();
		}

		/// <summary>Synchronizes the localstorage with the wunderlist-servers.</summary>
		public void Synchronize() {

			if (!_loggedIn)
				throw new wunderException("You need to call Login(email, password) first before you can start synchronizing.");

			/*
				Synchronization step 1
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

			//Add new lists and tasks
			if (step1Result.syncTable != null && step1Result.syncTable.newLists != null)
				step1Result.syncTable.newLists.ForEach(l => Lists.addOrUpdateList(l));

			if (step1Result.syncTable != null && step1Result.syncTable.newTasks != null)
				step1Result.syncTable.newTasks.ForEach(t => Tasks.addOrUpdateTask(t));

			if (step1Result.syncTable != null && step1Result.syncTable.syncedLists != null &&
			    step1Result.syncTable.syncedLists.Count > 0)
				for (int i = 0; i < step1Result.syncTable.syncedLists.Count; i++)
					step1Request.syncTable.newLists[i].Id = step1Result.syncTable.syncedLists[i].Id;


			/*
				Synchronization step 2
			 */
			var step2Request = new syncStep2Request {
			                                        	eMail = _credentials.eMail,
			                                        	Password = _credentials.Password
			                                        };
			step2Request.syncTable.newTasks.AddRange(Tasks.Where(t => t.Id <= 0));
			step2Request.syncTable.newTasks.ForEach(t => t.Id = 0); //To successfully sync, set id to 0

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

			//Seems like everything worked, save the tasks and lists locally
			writeLocalStorage();
		}

		/// <summary>Returns a list of email addresses with which the provided list is shared.</summary>
		public List<string> listSharedWith(listType list) {
			if(!_loggedIn)
				throw new wunderException("Cannot fetch addresses: User isn't logged in.");
			if(list.Shared==0)
				throw new wunderException("Cannot fetch addresses: List is not shared.");

			var request = new sharedWithRequest {
			                                    	eMail = _credentials.eMail,
													Password = _credentials.Password,
													listId = list.Id
			                                    };
			var response = _httpClient.httpPost<sharedWithRequest, sharedWithResponse>(request);
			if (response.statusCode != statusCodes.SHARE_SUCCESS)
				throw new wunderRequestException(response.statusCode, "Server error while fetching shared addresses");

			return response.eMails;
		}
		
		public void shareListWith(listType list, string email) {
			var request = new shareWithRequest {
				eMail = _credentials.eMail,
				Password = _credentials.Password,
				listId = list.Id,
				Add = email
			};
			var response = _httpClient.httpPost<shareWithRequest, baseResponse>(request);
			if(response.statusCode != statusCodes.SHARE_SUCCESS)
				throw new wunderRequestException(response.statusCode,"Failed to share list");

		}

		public void unshareList(listType list, string email) {
			var request = new unshareRequest {
				eMail = _credentials.eMail,
				Password = _credentials.Password,
				listId = list.Id,
				Delete = email
			};
			var response = _httpClient.httpPost<unshareRequest, baseResponse>(request);
			if(response.statusCode != statusCodes.SHARE_SUCCESS)
				throw new wunderRequestException(response.statusCode, string.Format("Failed to unshare list with {0}", email));
		}

		public void unshareListCompletely(listType list) {
			var request = new unshareCompletelyRequest {
				                                           eMail = _credentials.eMail,
				                                           Password = _credentials.Password,
				                                           listId = list.Id
			                                           };
			var response = _httpClient.httpPost<unshareCompletelyRequest, baseResponse>(request);
			if(response.statusCode != statusCodes.SHARE_SUCCESS)
				throw new wunderRequestException(response.statusCode, string.Format("Failed to unshare list"));
		}

		/// <summary>Writes Lists and Tasks to the LocalStorage in the FileSystem.</summary>
		private void writeLocalStorage() {

			//If there is no StorageLocation we can't load anything...
			if (string.IsNullOrWhiteSpace(localStorageDirectory))
				return;

			//Not logged in, no data to write
			if (!_loggedIn)
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
			Tasks.ForEach(t => {
			              	t.trackChanges = true;
			              	if (t.Date == null)
			              		t.Date = 0;
			              });
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
			//Save unsynced changes
			writeLocalStorage();

			Tasks.Clear();
			Lists.Clear();

			_credentials.Password = string.Empty;
		}

		#endregion
	}
}