using System;
using System.Collections.Generic;
using System.Diagnostics;
using Acr.UserDialogs;
using Rg.Plugins.Popup.Services;
using Safester.Controls;
using Safester.Network;
using Safester.Services;
using Safester.Utils;
using Safester.ViewModels;
using Xamarin.Forms;

namespace Safester.Views
{
    public partial class TwoFactorSettingsPage : ContentPage
    {
        bool isAuthenticated = false;
        public TwoFactorSettingsPage()
        {
            InitializeComponent();

            switchAuthentication.Toggled += (sender, e) =>
            {
                if (switchAuthentication.IsToggled != isAuthenticated)
                {
                    CustomAlertPage.Show(AppResources.Warning, AppResources.TwoFactorAlert, AppResources.OK);
                }

                switchAuthentication.IsToggled = isAuthenticated;
            };
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            LoadSettings();

            BackgroundColor = ThemeHelper.GetReadMailBGColor();
            lblTwoFactor.TextColor = ThemeHelper.GetReadMailLabelColor();
        }

        private async void LoadSettings()
        {
            ShowLoading(true);

            try
            {
                var settings = await ApiManager.SharedInstance().Get2FASettings(App.CurrentUser.UserEmail, App.CurrentUser.Token);

                ShowLoading(false);

                if (settings != null)
                {
                    isAuthenticated = settings.the2faActivationStatus;
                    switchAuthentication.IsToggled = isAuthenticated;
                    switchAuthentication.ColorChangedEvent?.Invoke();
                }
                else
                {

                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        private void ShowLoading(bool isShowing)
        {
            if (isShowing)
                UserDialogs.Instance.Loading(AppResources.Pleasewait, null, null, true);
            else
                UserDialogs.Instance.Loading().Hide();
        }
    }
}
