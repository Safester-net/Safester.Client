using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Acr.UserDialogs;
using Safester.Controls;
using Safester.CryptoLibrary.Api;
using Safester.Models;
using Safester.Network;
using Safester.Services;
using Safester.Utils;
using Xamarin.Forms;

namespace Safester.Views
{
    public partial class UsersPage : ContentPage
    {
        public static ObservableCollection<User> Users { get; set; }
        private SettingsService _settingsService { get; set; }

        public UsersPage()
        {
            InitializeComponent();

            Users = new ObservableCollection<User>();

            _settingsService = DependencyService.Get<SettingsService>();

            if (App.ConnectedUsers != null && App.ConnectedUsers.Count > 0)
            {
                foreach (var item in App.ConnectedUsers)
                {
                    if (item.UserEmail.Equals(App.CurrentUser.UserEmail) == false)
                        Users.Add(item);
                }
            }

            UsersListView.ItemsSource = Users;

            BackgroundColor = ThemeHelper.GetReadMailBGColor();

            btnDeleteAccount.Text = AppResources.UserDeleteAcc + " " + App.CurrentUser.UserEmail;
            btnDeleteAccount.Clicked += BtnDeleteAccount_Clicked;
        }

        private async void BtnDeleteAccount_Clicked(object sender, EventArgs e)
        {
            var msg = AppResources.DeleteAccount + " \"" + App.CurrentUser.UserEmail + "\"";
            bool result = await CustomAlertPage.Show(AppResources.Warning, msg, AppResources.Yes, AppResources.Cancel);
            if (result)
            {
                try
                {
                    ShowLoading(true);
                    ApiManager.SharedInstance().DeleteAccount(App.CurrentUser.UserEmail, new String(App.CurrentUser.UserPassword), "", (success, message) => {
                        if (success == true)
                        {
                            ShowLoading(false);
                            var _settingsService = DependencyService.Get<SettingsService>();
                            _settingsService.SaveSettings("rememberuser", "0");

                            CustomAlertPage.Show(string.Empty, AppResources.AccountDeleted, AppResources.OK);

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
                                Navigation.PushAsync(new TwoFactorPage(App.CurrentUser.UserEmail, new String(App.CurrentUser.UserPassword), false, false));
                            }
                            else
                            {
                                CustomAlertPage.Show(AppResources.Error, message, AppResources.OK);
                            }
                        }
                    });
                    ShowLoading(false);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }

        async void OnItemSelected(object sender, SelectedItemChangedEventArgs args)
        {
            var item = args.SelectedItem as User;
            if (item == null)
                return;

            // Manually deselect item.
            UsersListView.SelectedItem = null;

            ShowLoading(true);
            ApiManager.SharedInstance().Login(item.UserEmail, new String(item.UserPassword), "", (success, message) =>
            {
                if (success == true)
                {
                    App.CurrentUser.UserEmail = item.UserEmail;
                    App.CurrentUser.UserPassword = item.UserPassword;

                    ApiManager.SharedInstance().GetPrivateKey(App.CurrentUser.UserEmail, App.CurrentUser.Token, (suc, keyInfo) => {
                        ShowLoading(false);
                        if (suc)
                        {
                            App.CurrentUser.PrivateKey = keyInfo.privateKey;
                            App.KeyDecryptor = new Decryptor(App.CurrentUser.PrivateKey, App.CurrentUser.UserPassword);

                            _settingsService.SaveSettings("useremail", item.UserEmail);
                            _settingsService.SaveSettings("password_encrypted", "1");
                            _settingsService.SaveSettings("password", Utils.Utils.GetEncryptedPassphrase(new String(item.UserPassword)));

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
                        CustomAlertPage.Show("", string.Format(alertMsg, item.UserEmail), AppResources.OK);
                    }
                    else if (message.StartsWith(Errors.LOGIN_ACCOUNT_INVALID2FA, StringComparison.OrdinalIgnoreCase))
                    {
                        Navigation.PushAsync(new TwoFactorPage(item.UserEmail, new String(item.UserPassword), true));
                    }
                    else
                    {
                        CustomAlertPage.Show(AppResources.Error, message, AppResources.OK);
                    }
                }
            });
        }

        void Connect_Clicked(object sender, System.EventArgs e)
        {
            Navigation.PushAsync(new ConnectUserPage());
        }

        private void ShowLoading(bool isShowing)
        {
            if (isShowing)
                UserDialogs.Instance.Loading(AppResources.Pleasewait, null, null, true);
            else
                UserDialogs.Instance.Loading().Hide();
        }

        async void OnDelete(object sender, System.EventArgs e)
        {
            var mi = ((MenuItem)sender);
            var data = (mi.CommandParameter as User);

            var message = AppResources.DeleteAccount + " \"" + data.UserEmail + "\"";
            bool result = await CustomAlertPage.Show(AppResources.Warning, message, AppResources.Yes, AppResources.Cancel);
            if (result)
            {
                try
                {
                    App.ConnectedUsers.Remove(data);
                    Users.Remove(data);

                    UsersListView.ItemsSource = null;
                    UsersListView.ItemsSource = Users;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }
    }
}
