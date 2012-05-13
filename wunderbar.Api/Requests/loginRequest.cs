using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using wunderbar.Api.httpClientAttributes;

namespace wunderbar.Api.Requests {
	public class loginRequest : baseRequest {

		[httpClientIgnorePropertyAttribute]
		public override string actionToken {
			get { return "/login"; }
		}
	}
}
