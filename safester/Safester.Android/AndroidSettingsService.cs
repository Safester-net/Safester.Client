using System;
using Xamarin.Forms;
using System.IO;
using Android.Content;
using Plugin.CurrentActivity;
using Safester.Services;
using Safester.Droid;
using Android.Support.V4.Content;
using Android.Support.V4.App;
using Android.Content.PM;

[assembly: Xamarin.Forms.Dependency(typeof(AndroidSettingsService))]
namespace Safester.Droid
{
	public class AndroidSettingsService : SettingsService
	{
		public void SaveSettings(string key, string value)
		{
			//store
			var prefs = Android.App.Application.Context.GetSharedPreferences(key, FileCreationMode.Private);
			var prefEditor = prefs.Edit();
			prefEditor.PutString(key, value);
			prefEditor.Commit();
		}

		public string LoadSettings(string key)
		{
			//retreive 
			var prefs = Android.App.Application.Context.GetSharedPreferences(key, FileCreationMode.Private);
			var value = prefs.GetString(key, "");

			return value;
		}

		public string GetAppVersionName()
		{
            return "1.3.5";
		}

        public void AskContactsPermission(Action ContactsGrantedAction)
        {
            if (ContextCompat.CheckSelfPermission(Android.App.Application.Context, Android.Manifest.Permission.ReadContacts) != Permission.Granted)
            {
                if (CrossCurrentActivity.Current.Activity is MainActivity)
                    (CrossCurrentActivity.Current.Activity as MainActivity).ContactPermissionGranted = ContactsGrantedAction;

                ActivityCompat.RequestPermissions(CrossCurrentActivity.Current.Activity, new String[] {
                Android.Manifest.Permission.ReadContacts}, 1);
            }
            else
            {
                ContactsGrantedAction?.Invoke();
            }
        }
	}
}

