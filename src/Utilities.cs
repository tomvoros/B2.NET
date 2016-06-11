﻿using System;
using System.Diagnostics;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using B2Net.Models;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.IO;

namespace B2Net {
	public static class Utilities {
		/// <summary>
		/// Create the B2 Authorization header. Base64 encoded accountId:applicationKey.
		/// </summary>
		public static string CreateAuthorizationHeader(string accountId, string applicationKey) {
			var authHeader = "Basic ";
			var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes(accountId + ":" + applicationKey));
			return authHeader + credentials;
		}
		
		public static async Task CheckForErrors(HttpResponseMessage response) {
			if (!response.IsSuccessStatusCode) {
				string content = await response.Content.ReadAsStringAsync();

				B2Error b2Error;
				try {
					b2Error = JsonConvert.DeserializeObject<B2Error>(content);
				} catch (Exception ex) {
					throw new Exception("Seralization of the response failed. See inner exception for response contents and serialization error.", ex);
				}
				if (b2Error != null) {
					throw new Exception($"Status: {b2Error.Status}, Code: {b2Error.Code}, Message: {b2Error.Message}");
				}

                // TODO: What if b2Error is null? We already consumed the content. Will this be a problem?
			}
		}

		public static string GetSHA1Hash(byte[] fileData) {
			using (SHA1CryptoServiceProvider sha1 = new SHA1CryptoServiceProvider()) {
				return HexStringFromBytes(sha1.ComputeHash(fileData));
			}
		}

        public static string GetSHA1Hash(Stream fileStream) {
            using (SHA1CryptoServiceProvider sha1 = new SHA1CryptoServiceProvider()) {
				return HexStringFromBytes(sha1.ComputeHash(fileStream));
			}
        }

        public static string DetermineBucketId(B2Options options, string bucketId) {
			// Check for a persistant bucket
			if (!options.PersistBucket && string.IsNullOrEmpty(bucketId)) {
				throw new ArgumentNullException(nameof(bucketId));
			}
			
			// Are we persisting buckets? If so use the one from settings
			return options.PersistBucket ? options.BucketId : bucketId;
		}

		private static string HexStringFromBytes(byte[] bytes) {
			var sb = new StringBuilder();
			foreach (byte b in bytes) {
				var hex = b.ToString("x2");
				sb.Append(hex);
			}
			return sb.ToString();
		}

		internal class B2Error {
				public string Code { get; set; }
				public string Message { get; set; }
				public string Status { get; set; }
		}
	}

	public static class B2StringExtension {
		public static string b2UrlEncode(this string str) {
			if (str == "/") {
				return str;
			}
			return Uri.EscapeDataString(str);
		}

		public static string b2UrlDecode(this string str) {
			if (str == "+") {
				return " ";
			}
			return Uri.UnescapeDataString(str);
		}
	}
}
