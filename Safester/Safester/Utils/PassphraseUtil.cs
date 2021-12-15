using System;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace Safester.Utils
{
    public class PassphraseUtil
    {
        public static String salt = "ThiS*IsSAlt4loGin$";
        public static int PASSPHRASE_HASH_ITERATIONS = 3;

        /**
         * Get password for connection using hash
         * @param login         the login
         * @param passphrase    the passphrase
         *
         * @return  the password to use for connection
         * @throws NoSuchAlgorithmException
         * @throws NoSuchProviderException
         */
        public static String ComputeHashAndSaltedPassphrase(String login, String password)
        {
            try
            {
                SHA1 sha1 = SHA1.Create();

                // Always convert passphrase to HTML so stah the getBytes() will produce
                // the same on all platforms: Windows, Mac OS X, Linux
                String passphraseStr = HttpUtility.HtmlEncode(password);
                byte[] bArray = Encoding.UTF8.GetBytes(passphraseStr);
                passphraseStr = GetHexHash(sha1, bArray);

                if (passphraseStr.Length > 20)
                    passphraseStr = passphraseStr.Substring(0, 20);
                passphraseStr = passphraseStr.ToLower();

                //Apply salt and hash iterations
                String loginsalt = login + salt;
                byte[] bPassphraseSaltCompute = Utils.Combine(Encoding.UTF8.GetBytes(passphraseStr), Encoding.UTF8.GetBytes(loginsalt));
                String connectionPassword = "";
                for (int i = 0; i < PASSPHRASE_HASH_ITERATIONS; i++)
                {
                    connectionPassword = GetHexHash(sha1, bPassphraseSaltCompute);
                    bPassphraseSaltCompute = Encoding.UTF8.GetBytes(connectionPassword);
                }

                if (connectionPassword.Length > 20)
                    connectionPassword = connectionPassword.Substring(0, 20); // half of hash

                connectionPassword = connectionPassword.ToLower(); // All tests in lowercase

                return connectionPassword;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return string.Empty;
        }

        private static string GetHexHash(SHA1 sha, byte[] ba)
        {
            byte[] computeHash = sha.ComputeHash(ba);
            return Utils.ByteArrayToString(computeHash);
        }
    }
}