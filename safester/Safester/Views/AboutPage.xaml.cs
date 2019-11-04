using System;
using Acr.UserDialogs;
using Safester.Controls;
using Safester.Utils;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Safester.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AboutPage : ContentPage
    {
        public AboutPage()
        {
            InitializeComponent();

            TapGestureRecognizer urlGesture = new TapGestureRecognizer();
            urlGesture.Tapped += (sender, e) =>
            {
                Device.OpenUri(new Uri("https://www.safester.net"));
            };
            lblUrl.GestureRecognizers.Add(urlGesture);

            TapGestureRecognizer emailGesture = new TapGestureRecognizer();
            emailGesture.Tapped += async (sender, e) =>
            {
                if (Utils.Utils.SendEmail("contact@safester.net") == false)
                {
                    CustomAlertPage.Show("", AppResources.ALERT_NOEMAILAPP, AppResources.OK);
                }
            };
            lblEmail.GestureRecognizers.Add(emailGesture);

            if (Device.RuntimePlatform == Device.iOS)
            {
                lblUrl.IsVisible = false;
            }

            BackgroundColor = ThemeHelper.GetLoginBGColor();
            imgLogo.Source = ImageSource.FromFile(ThemeHelper.GetLoginLogoName());
        }
    }
}