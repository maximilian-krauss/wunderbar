using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using wunderbar.Api.Requests;
using wunderbar.Api.Responses;
using System.Reflection;
using wunderbar.Api.httpClientAttributes;
using wunderbar.Api.Extensions;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Net;
using System.Runtime.Serialization;
using System.Web;

namespace wunderbar.Api {
	internal sealed class httpClient {

		public TResponse httpPost<TRequest, TResponse>(TRequest request)
			where TRequest : baseRequest
			where TResponse : baseResponse {

			var requestParams = buildRequestParam(request);
			requestParams = requestParams.Substring(0, requestParams.Length - 1); //TODO: Fix this shit
			var requestData = Encoding.Default.GetBytes(requestParams);

			var httpRequest = (HttpWebRequest) WebRequest.Create(request.baseUrl + request.actionToken); //TODO: Method to securely concat these two URL-Parts
			httpRequest.Method = WebRequestMethods.Http.Post;
			httpRequest.ContentType = "application/x-www-form-urlencoded";
			httpRequest.Accept = "application/json";
			httpRequest.ContentLength = requestData.Length;
			httpRequest.GetRequestStream().Write(requestData, 0, requestData.Length);

			var httpResponse = (HttpWebResponse) httpRequest.GetResponse();
			TResponse response;
			using (var responseStream = httpResponse.GetResponseStream()) {
				var responseSerializer = new DataContractJsonSerializer(typeof (TResponse));
				response = (TResponse) responseSerializer.ReadObject(responseStream);
			}
			httpResponse.Close();
			return response;
		}

		private string buildRequestParam<TRequest>(TRequest request) {
			
			//Create Params from Properties
			var requestKVParams = new Dictionary<string, string>();
			var requestParams = new List<string>();
			foreach (var pInfo in typeof(TRequest).GetProperties()) {
				//Skip if no needed
				if(getAttribute<httpClientIgnorePropertyAttribute>(pInfo) != null)
					continue;

				//Value needs Transformation?
				var transformAttribute = getAttribute<httpClientTransformValueAttribute>(pInfo);
				if (transformAttribute != null && transformAttribute.handlesNameAndValue)
					requestParams.Add(transformValue(pInfo, pInfo.GetValue(request, null), transformAttribute.transformTo));
				else {
					string propertyValue = transformAttribute != null
					                       	? transformValue(pInfo, pInfo.GetValue(request, null), transformAttribute.transformTo)
					                       	: pInfo.GetValue(request, null).ToString();
					requestKVParams.Add(getPropertyName(pInfo), propertyValue);
				}
			}

			//Build String
			var sbParams = new StringBuilder();
			foreach (var kvPair in requestKVParams)
				sbParams.AppendFormat("{0}={1}&", kvPair.Key, HttpUtility.UrlEncode( kvPair.Value));

			if (requestParams.Count > 0) {
				foreach (string param in requestParams)
					sbParams.Append(param);
			}
			
			return sbParams.ToString();
		}

		private TAttribute getAttribute<TAttribute>(PropertyInfo property) where TAttribute : Attribute {
			var attributes = property.GetCustomAttributes(typeof (TAttribute), true);
			return attributes.Length > 0 ? (TAttribute) attributes[0] : null;
		}
		private string getPropertyName(PropertyInfo property) {
			var nameAttribute = getAttribute<DataMemberAttribute>(property);
			return nameAttribute == null || string.IsNullOrEmpty(nameAttribute.Name) ? property.Name : nameAttribute.Name;
		}

		private string transformValue(PropertyInfo property, object value, httpClientValueTransformations transformTo) {
			switch (transformTo) {
				case httpClientValueTransformations.MD5Hash:
					return value.ToString().generateMD5Hash();
				case httpClientValueTransformations.JSON:
					using (var msJson = new MemoryStream()) {
						var serializer = new DataContractJsonSerializer(value.GetType());
						serializer.WriteObject(msJson, value);
						return Encoding.UTF8.GetString(msJson.ToArray());
					}
				case httpClientValueTransformations.FUON:
					return transformFUON(property, value);
				default:
					return value.ToString();
			}
		}

		private string transformFUON(PropertyInfo property, object value) {

			/*
				WARNING! We're what you'd call experts. Do not try any of these things at home!
				--------------------------------------------------------------------------------
				Desired Format: param-name[list-name][index][property-name] = [value]&
			 
				TODO: Comment and Clean
			 */
			string fuonRoot = getPropertyName(property);
			var result = new StringBuilder();

			//Get each List from the Container
			foreach (var table in value.GetType().GetProperties()) {
				var instance = table.GetValue(value, null);
				string fuonTablePath = string.Format("{0}[{1}]", fuonRoot, getPropertyName(table));
				int itemCount = (int)instance.GetType().GetProperty("Count").GetValue(instance, null);
				if (itemCount > 0) {
					for (int i = 0; i < itemCount; i++) {

						var tablePropertyEntry = instance.GetType().GetProperty("Item").GetValue(instance, new object[] {i});
						foreach (var tableProperty in tablePropertyEntry.GetType().GetProperties()) {

							//Should this Property skipped?
							if (getAttribute<httpClientIgnorePropertyAttribute>(tableProperty) != null)
								continue;

							string key = HttpUtility.UrlEncode(string.Format("{0}[{1}][{2}]", new object[] {
							                                                                               	fuonTablePath,
							                                                                               	i,
							                                                                               	getPropertyName(tableProperty)
							                                                                               }));
							result.Append(string.Format("{0}={1}&", new object[] {
							                                              	key,
							                                              	HttpUtility.UrlEncode(tableProperty.GetValue(tablePropertyEntry, null).ToString())
							                                              }));
						}
					}
				}
			}
			return result.ToString();
		}

	}
}