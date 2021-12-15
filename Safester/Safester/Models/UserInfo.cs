using System;

namespace Safester.Models
{	
	public class UserInfo : BaseResult
    {
		public string token { get; set; }
        public int product { get; set; }
        public string name { get; set; }
    }

    public class User
    {
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public char[] UserPassword { get; set; }
        public string PassPhrase { get; set; }
        public string Token { get; set; }
        public string PrivateKey { get; set; }

        public User Clone()
        {
            User user = new User();
            if (string.IsNullOrEmpty(UserName) == false)
                user.UserName = (string)UserName.Clone();

            if (string.IsNullOrEmpty(UserEmail) == false)
                user.UserEmail = (string)UserEmail.Clone();

            if (UserPassword != null)
                user.UserPassword = (char[])UserPassword.Clone();

            if (string.IsNullOrEmpty(PassPhrase) == false)
                user.PassPhrase = (string)PassPhrase.Clone();

            if (string.IsNullOrEmpty(Token) == false)
                user.Token = (string)Token.Clone();

            if (string.IsNullOrEmpty(PrivateKey) == false)
                user.PrivateKey = (string)PrivateKey.Clone();

            return user;
        }
    }
}