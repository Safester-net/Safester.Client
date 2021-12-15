using System;
namespace Safester.Utils
{
    public static class Errors
    {
        public static string NO_ERROR = "ok";
        public static string API_ERROR = "ko";

        // Register API
        public static string REGISTER_ACCOUNT_EXISTS = "error_account_already_exists";
        public static string REGISTER_EMAIL_INVALID = "error_invalid_email_address";
        public static string REGISTER_COUPON_INVALID = "error_invalid_coupon";

        // Login API
        public static string LOGIN_ACCOUNT_PENDING = "error_account_pending_validation";
        public static string LOGIN_ACCOUNT_INVALID2FA = "error_invalid_2facode";

        public static bool IsErrorExist(string response)
        {
            if (!string.IsNullOrEmpty(response) && response.StartsWith(Errors.API_ERROR, StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        }

        public static bool IsApiSuccess(string response)
        {
            if (!string.IsNullOrEmpty(response) && response.StartsWith(Errors.NO_ERROR, StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        }
    }
}
