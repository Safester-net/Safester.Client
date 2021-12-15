using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Safester.Controls;
using Safester.CryptoLibrary.Api;
using Safester.Custom.Effects;
using Safester.Models;
using Safester.Network;
using Safester.Services;
using Safester.Utils;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Safester.Views
{
    public partial class TwoFactorPage : ContentPage
    {
        private string userName { get; set; }
        private string userPassphrase { get; set; }
        private bool rememberUser { get; set; }
        private bool loginPage { get; set; }
        private SettingsService _settingsService { get; set; }

        public TwoFactorPage(string name, string passphrase, bool remember, bool isLoginPage = true)
        {
            InitializeComponent();

            userName = name;
            userPassphrase = passphrase;
            rememberUser = remember;
            loginPage = isLoginPage;

            _settingsService = DependencyService.Get<SettingsService>();

            TapGestureRecognizer loginGesture = new TapGestureRecognizer();
            loginGesture.Tapped += StartBtn_Clicked;
            ImgLogin.GestureRecognizers.Add(loginGesture);

            TapGestureRecognizer pasteGesture = new TapGestureRecognizer();
            pasteGesture.Tapped += PasteBtn_Clicked;
            ImgPaste.GestureRecognizers.Add(pasteGesture);

            BackgroundColor = ThemeHelper.GetLoginBGColor();
        }

        async void StartBtn_Clicked(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(entryCode.Text))
            {
                await CustomAlertPage.Show(AppResources.Warning, AppResources.ErrorInputTwoFactorCode, AppResources.OK);
                return;
            }

            ShowLoading(true);
            if (loginPage)
            {
                ApiManager.SharedInstance().Login(userName, userPassphrase, entryCode.Text, (success, message) =>
                {
                    if (success == true)
                    {
                        App.CurrentUser.UserEmail = userName;
                        App.CurrentUser.UserPassword = userPassphrase.ToCharArray();

                        ApiManager.SharedInstance().GetPrivateKey(App.CurrentUser.UserEmail, App.CurrentUser.Token, (suc, keyInfo) => {
                            ShowLoading(false);
                            if (suc)
                            {
                                App.CurrentUser.PrivateKey = keyInfo.privateKey;
                                App.KeyDecryptor = new Decryptor(App.CurrentUser.PrivateKey, App.CurrentUser.UserPassword);

                                _settingsService.SaveSettings("rememberuser", rememberUser ? "1" : "0");
                                if (rememberUser)
                                {
                                    _settingsService.SaveSettings("useremail", userName);
                                    _settingsService.SaveSettings("password_encrypted", "1");
                                    _settingsService.SaveSettings("password", Utils.Utils.GetEncryptedPassphrase(userPassphrase));
                                }

                                Utils.Utils.AddOrUpdateUser(App.CurrentUser);
                                Utils.Utils.SaveDataToFile(App.LocalUsers, Utils.Utils.KEY_FILE_USERS);

                                Utils.Utils.LoadUserProfiles();
                                App.Current.MainPage = new NavigationPage(new MainPage());
                            }
                            else
                            {
                                CustomAlertPage.Show(AppResources.Error, keyInfo.errorMessage, AppResources.OK);
                            }
                        });
                    }
                    else
                    {
                        ShowLoading(false);

                        if (message.StartsWith(Errors.LOGIN_ACCOUNT_PENDING, StringComparison.OrdinalIgnoreCase))
                        {
                            var alertMsg = AppResources.ALERT_CONFIRM_EMAIL.Replace("\\n", "\n");
                            CustomAlertPage.Show("", string.Format(alertMsg, userName), AppResources.OK);
                        }
                        else if (message.StartsWith(Errors.LOGIN_ACCOUNT_INVALID2FA, StringComparison.OrdinalIgnoreCase))
                        {
                            var alertMsg = AppResources.ALERT_INVALID_2FACODE.Replace("\\n", "\n");
                            CustomAlertPage.Show("", string.Format(alertMsg, ""), AppResources.OK);
                        }
                        else
                        {
                            CustomAlertPage.Show(AppResources.Error, message, AppResources.OK);
                        }
                    }
                });
            }
            else
            {
                ApiManager.SharedInstance().DeleteAccount(userName, userPassphrase, entryCode.Text, (success, message) =>
                {
                    if (success == true)
                    {
                        ShowLoading(false);
                        var _settingsService = DependencyService.Get<SettingsService>();
                        _settingsService.SaveSettings("rememberuser", "0");

                        LoginPage.CurrentUserEmail = App.CurrentUser.UserEmail;
                        LoginPage.CurrentUserPassword = String.Empty;
                        LoginPage.NeedsUpdating = true;

                        App.ConnectedUsers = new System.Collections.ObjectModel.ObservableCollection<User>();

                        App.Current.MainPage = new NavigationPage(new LoginPage());
                    }
                    else
                    {
                        ShowLoading(false);

                        if (message.StartsWith(Errors.LOGIN_ACCOUNT_INVALID2FA, StringComparison.OrdinalIgnoreCase))
                        {
                            Navigation.PushAsync(new TwoFactorPage(App.CurrentUser.UserEmail, new String(App.CurrentUser.UserPassword), false));
                        }
                        else
                        {
                            CustomAlertPage.Show(AppResources.Error, message, AppResources.OK);
                        }
                    }
                });
            }
        }

        async void PasteBtn_Clicked(object sender, EventArgs e)
        {
            string str = await Clipboard.GetTextAsync();
            if (!string.IsNullOrEmpty(str))
            {
                entryCode.Text = str;
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
