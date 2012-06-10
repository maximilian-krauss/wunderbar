using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace wunderbar.Api {
	internal static class statusCodes {
		public const int REGISTER_SUCCESS = 100;
		public const int REGISTER_DUPLICATE = 101;
		public const int REGISTER_INVALID_EMAIL = 102;
		public const int REGISTER_FAILURE = 103;

		public const int LOGIN_SUCCESS = 200;
		public const int LOGIN_FAILURE = 201;
		public const int LOGIN_DENIED = 202;
		public const int LOGIN_NOT_EXIST = 203;

		public const int SYNC_SUCCESS = 300;
		public const int SYNC_FAILURE = 301;
		public const int SYNC_DENIED = 302;
		public const int SYNC_NOT_EXIST = 303;

		public const int PASSWORD_SUCCESS = 400;
		public const int PASSWORD_INVALID_EMAIL = 401;
		public const int PASSWORD_FAILURE = 402;

		public const int INVITE_SUCCESS = 500;
		public const int INVITE_INVALID_EMAIL = 501;
		public const int INVITE_FAILURE = 502;

		public const int EDIT_PROFILE_SUCCESS = 600;
		public const int EDIT_PROFILE_AUTHENTICATION_FAILED = 601;
		public const int EDIT_PROFILE_EMAIL_ALREADY_EXISTS = 602;
		public const int EDIT_PROFILE_INVALID_EMAIL_ADDRESS = 603;
		public const int EDIT_PROFILE_FAILURE = 604;

		public const int DELETE_ACCOUNT_SUCCESS = 700;
		public const int DELETE_ACCOUNT_FAILURE = 701;
		public const int DELETE_ACCOUNT_INVALID_EMAIL = 702;
		public const int DELETE_ACCOUNT_NOT_EXISTS = 703;
		public const int DELETE_ACCOUNT_DENIED = 704;

		public const int SHARE_SUCCESS = 800;
		public const int SHARE_FAILURE = 801;
		public const int SHARE_DENIED = 802;
		public const int SHARE_NOT_EXISTS = 803;
		public const int SHARE_NOT_SHARED = 804;
		public const int SHARE_OWN_EMAIL = 805;
	}
}
