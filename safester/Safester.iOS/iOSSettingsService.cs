using System;
using Xamarin.Forms;
using AVFoundation;
using Foundation;
using System.IO;
using Safester.iOS;
using Safester.Services;
using Safester.Utils;
using UIKit;

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
            return "2.2.4"; //NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleVersion").ToString();
        }

        public void AskContactsPermission(Action ContactsGrantedAction)
        {
            ContactsGrantedAction?.Invoke();
        }

        public void CloseApplication()
        {
            System.Threading.Thread.CurrentThread.Abort();
        }

        public void ChangeTheme(ThemeStyle style)
        {
            
        }
    }
}

