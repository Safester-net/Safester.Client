using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Plugin.CurrentActivity;
using Acr.UserDialogs;
using Android.Support.V4.Content;
using Android.Support.V4.App;
using Safester.Droid.Renderers;

namespace Safester.Droid
{
    [Activity(Label = "Safester", Icon = "@drawable/icon", Theme = "@style/MainTheme", ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        public Action    ContactPermissionGranted { get; set; }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);

            HtmlLabelRenderer.Initialize();
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);

            CrossCurrentActivity.Current.Init(this, savedInstanceState);

            UserDialogs.Init(() => this);
            Rg.Plugins.Popup.Popup.Init(this, savedInstanceState);

            Window.SetSoftInputMode(Android.Views.SoftInput.AdjustResize);
            AndroidBug5497WorkaroundForXamarinAndroid.assistActivity(this);

            LoadApplication(new App());
        }

        const int TAG_CODE_PERMISSIONS = 1;
        protected override void OnResume()
        {
            base.OnResume();

            if (ContextCompat.CheckSelfPermission(this, Android.Manifest.Permission.WriteExternalStorage) != Permission.Granted ||
                ContextCompat.CheckSelfPermission(this, Android.Manifest.Permission.ReadExternalStorage) != Permission.Granted)
                ActivityCompat.RequestPermissions(this, new String[] {
                Android.Manifest.Permission.WriteExternalStorage, Android.Manifest.Permission.ReadExternalStorage }, TAG_CODE_PERMISSIONS);
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            switch (requestCode)
            {
                case TAG_CODE_PERMISSIONS:
                    {
                        if (permissions != null && permissions[0] == Android.Manifest.Permission.ReadContacts &&
                            grantResults != null && grantResults[0] == Permission.Granted)
                            ContactPermissionGranted?.Invoke();
                    }
                    break;
                default:
                    break;
            }

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}