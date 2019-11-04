using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;
using Acr.UserDialogs;
using Safester.Controls;
using Safester.Custom.Effects;
using Safester.Network;
using Safester.Utils;
using Xamarin.Forms;

namespace Safester.Views
{
    public partial class PatatePage : ContentPage
    {
        private bool _isCreating { get; set; }

        private string _userName { get; set; }
        private string _emailAddr { get; set; }

        public PatatePage()
        {
            InitializeComponent();

            entryPassword.Effects.Add(new ShowHidePassEffect());
            entryConfirmPassword.Effects.Add(new ShowHidePassEffect());
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            BackgroundColor = ThemeHelper.GetLoginBGColor();
        }

        void Save_Clicked(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(entryName.Text) == true)
            {
                CustomAlertPage.Show(AppResources.Error, AppResources.ErrorInputUserName, AppResources.OK);
                return;
            }

            if (string.IsNullOrWhiteSpace(entryEmail.Text) == true)
            {
                CustomAlertPage.Show(AppResources.Error, AppResources.ErrorInputUserEmail, AppResources.OK);
                return;
            }

            if (entryEmail.Text.Contains(" ") == true)
            {
                CustomAlertPage.Show(AppResources.Error, AppResources.ErrorInputUserEmail1, AppResources.OK);
                return;
            }

            if (string.IsNullOrEmpty(entryPassword.Text) == true)
            {
                CustomAlertPage.Show(AppResources.Error, AppResources.ErrorInputUserPassPhrase, AppResources.OK);
                return;
            }

            if (string.IsNullOrEmpty(entryConfirmPassword.Text) == true)
            {
                CustomAlertPage.Show(AppResources.Error, AppResources.ErrorInputUserPassPhrase, AppResources.OK);
                return;
            }

            if (entryPassword.Text.Equals(entryConfirmPassword.Text) == false)
            {
                CustomAlertPage.Show(AppResources.Error, AppResources.ErrorInputUserPassPhrase1, AppResources.OK);
                return;
            }

            if (entryPassword.Text.Length < 10)
            {
                CustomAlertPage.Show(AppResources.Error, AppResources.ErrorInputUserPassPhrase2, AppResources.OK);
                return;
            }

            if (_isCreating)
                return;

            ShowLoading(true);

            _userName = HttpUtility.HtmlEncode(entryName.Text);
            _emailAddr = entryEmail.Text.ToLower();

            Task.Run(() =>
            {
                ApiManager.SharedInstance().Register(_userName, _emailAddr, entryPassword.Text, string.Empty, (success, message) =>
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        ShowLoading(false);
                        if (success == false)
                        {
                            if (message.StartsWith(Errors.REGISTER_ACCOUNT_EXISTS, StringComparison.OrdinalIgnoreCase))
                            {
                                CustomAlertPage.Show("", string.Format(AppResources.ALERT_ACCOUNT_EXIST, ""), AppResources.OK);
                            }
                            else if (message.StartsWith(Errors.REGISTER_EMAIL_INVALID, StringComparison.OrdinalIgnoreCase))
                            {
                                CustomAlertPage.Show("", string.Format(AppResources.ALERT_EMAIL_INVALID, ""), AppResources.OK);
                            }
                            else if (message.StartsWith(Errors.REGISTER_COUPON_INVALID, StringComparison.OrdinalIgnoreCase))
                            {
                                CustomAlertPage.Show("", string.Format(AppResources.ALERT_COUPON_INVALID, ""), AppResources.OK);
                            }
                            else
                            {
                                CustomAlertPage.Show("", string.Format(message, ""), AppResources.OK);
                            }
                        }
                        else
                        {
                            var alertMsg = AppResources.ALERT_REGISTER_SUCCESS.Replace("\\n", "\n");
                            CustomAlertPage.Show("", string.Format(alertMsg, _emailAddr), AppResources.OK);

                            LoginPage.CurrentUserEmail = _emailAddr;
                            LoginPage.CurrentUserPassword = String.Empty;
                            LoginPage.NeedsUpdating = true;

                            Navigation.PopAsync();
                        }
                    });
                });
            });
        }

        private void ShowLoading(bool isShowing)
        {
            _isCreating = isShowing;
            if (isShowing)
                UserDialogs.Instance.Loading(AppResources.Pleasewait, null, null, true);
            else
                UserDialogs.Instance.Loading().Hide();
        }
    }
}
