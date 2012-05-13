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

		private bool _loggedIn;

		public wunderClient() {
			Credentials = new digestCredentials();
			_httpClient = new httpClient();
			Lists = new List<listType>();
			Tasks = new List<taskType>();

			_loggedIn = false;
		}

		public digestCredentials Credentials { get; set; }

		public List<listType> Lists { get; private set; }

		public List<taskType> Tasks { get; private set; }

		public bool Login() {
			var result = _httpClient.httpPost<loginRequest, baseResponse>(new loginRequest {
			                                                                               	eMail = Credentials.eMail,
			                                                                               	Password = Credentials.Password
			                                                                               });
			_loggedIn = (result.statusCode == statusCodes.LOGIN_SUCCESS);
			return _loggedIn;
		}

		public void Synchronize() {

			/*
				Synchronization Step 1
			 */
			var step1Request = new syncStep1Request {
			                                        	eMail = Credentials.eMail,
														Password = Credentials.Password
			                                        };
			step1Request.syncTable.Lists.AddRange(Lists.Where(l => l.Id > 0));
			step1Request.syncTable.Tasks.AddRange(Tasks.Where(t => t.Id > 0));
			step1Request.syncTable.newLists.AddRange(Lists.Where(l => l.Id <= 0));

			var step1Result = _httpClient.httpPost<syncStep1Request, syncStep1Response>(step1Request);
			if (step1Result.statusCode != statusCodes.SYNC_SUCCESS)
				throw new synchronizationException(step1Request.Step, step1Result.statusCode);

			//Add new Lists and Tasks
			//TODO: The data in new* can also existing in this lists, check if exists and then update instead of insert
			if(step1Result.syncTable.newLists != null)
				Lists.AddRange(step1Result.syncTable.newLists);

			if (step1Result.syncTable.newTasks != null)
					Tasks.AddRange(step1Result.syncTable.newTasks);
			//TODO: Update existing Lists with the data from synced_lists


			/*
				Synchronization Step 2
			 */
			var step2Request = new syncStep2Request {
			                                        	eMail = Credentials.eMail,
			                                        	Password = Credentials.Password
			                                        };
			step2Request.syncTable.newTasks.AddRange(Tasks.Where(t => t.Id <= 0));

			if(step1Result.syncTable.requiredTasks != null)
				step2Request.syncTable.requiredTasks.AddRange(
						from task in Tasks 
						where step1Result.syncTable.requiredTasks.Any(requiredTask => requiredTask.Id == task.Id)
						select task
					);

			if(step1Result.syncTable.requiredLists != null)
				step2Request.syncTable.requiredLists.AddRange(
						from list in Lists
						where step1Result.syncTable.requiredLists.Any(requiredList => requiredList.Id == list.Id)
						select list
					);

			var step2Result = _httpClient.httpPost<syncStep2Request, syncStep2Response>(step2Request);
			if (step2Result.statusCode != statusCodes.SYNC_SUCCESS)
				throw new synchronizationException(step2Request.Step, step2Result.statusCode);

		}

		#region IDisposable Members

		public void Dispose() {
			throw new NotImplementedException();
		}

		#endregion
	}
}