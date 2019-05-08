using System;
using Xamarin.Forms;
using AVFoundation;
using Foundation;
using System.IO;
using Safester.iOS;
using Safester.Services;

[assembly: Xamarin.Forms.Dependency(typeof(iOSSettingsService))]
namespace Safester.iOS
{
	public class iOSSettingsService : SettingsService
	{
		public void SaveSettings(String key, String value)
		{
			NSUserDefaults.StandardUserDefaults.SetString(value, key);

			NSUserDefaults.StandardUserDefaults.Synchronize();
		}

		public String LoadSettings(String key)
		{
			String strResult = NSUserDefaults.StandardUserDefaults.StringForKey(key);
            if (string.IsNullOrEmpty(strResult))
                strResult = string.Empty;

			return strResult;
		}

        public string GetAppVersionName()
        {
            return "1.2.3"; //NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleVersion").ToString();
        }

        public void AskContactsPermission(Action ContactsGrantedAction)
        {
            ContactsGrantedAction?.Invoke();
        }
    }
}

