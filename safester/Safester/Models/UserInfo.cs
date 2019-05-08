using System;

namespace Safester.Models
{	
	public class UserInfo : BaseResult
    {
		public string token { get; set; }
        public int product { get; set; }
    }

    public class User
    {
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public char[] UserPassword { get; set; }
        public string PassPhrase { get; set; }
        public string Token { get; set; }
        public string PrivateKey { get; set; }
    }
}