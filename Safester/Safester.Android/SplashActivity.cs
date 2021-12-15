
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace Safester.Droid
{
    [Activity(Label = "Safester", MainLauncher = true, Icon = "@drawable/icon", Theme = "@style/MainTheme")]
    public class SplashActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.Splash);
        }

        protected override void OnResume()
        {
            base.OnResume();

            startMainActivity();
        }

        private async void startMainActivity()
        {
            await System.Threading.Tasks.Task.Delay(3000);

            StartActivity(new Intent(this, typeof(MainActivity)));
            Finish();
        }
    }
}
