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
using Safester.Utils;
using Android.Util;

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
            return "2.2.4";
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

        public void CloseApplication()
        {
            var activity = CrossCurrentActivity.Current.Activity;
            activity.FinishAffinity();
        }

        public void ChangeTheme(ThemeStyle style)
        {
            if (CrossCurrentActivity.Current.Activity is MainActivity)
            {
                if (style == ThemeStyle.STANDARD_THEME)
                {
                    CrossCurrentActivity.Current.Activity.SetTheme(Resource.Style.MainTheme);
                    ChangeAccentColor(Color.FromHex("#0356b3"));
                }
                else
                {
                    CrossCurrentActivity.Current.Activity.SetTheme(Resource.Style.MainTheme_Dark);
                    ChangeAccentColor(Color.FromHex("#4197FE"));
                }                    
            }
        }

        private void ChangeAccentColor(Color color)
        {
            var themeAccentColor = new TypedValue();
            CrossCurrentActivity.Current.Activity.Theme.ResolveAttribute(Resource.Attribute.colorAccent, themeAccentColor, true);
            var droidAccentColor = new Android.Graphics.Color(themeAccentColor.Data);

            var accentColorProp = typeof(Xamarin.Forms.Color).GetProperty(nameof(Xamarin.Forms.Color.Accent), System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            var xamarinAccentColor = new Xamarin.Forms.Color(droidAccentColor.R / 255.0, droidAccentColor.G / 255.0, droidAccentColor.B / 255.0, droidAccentColor.A / 255.0);
            accentColorProp.SetValue(null, xamarinAccentColor, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static, null, null, System.Globalization.CultureInfo.CurrentCulture);
        }
    }
}

