using System;
using System.Collections.Generic;
using System.Web;
using Acr.UserDialogs;
using Plugin.Multilingual;
using Rg.Plugins.Popup.Services;
using Safester.Controls;
using Safester.Models;
using Safester.Services;
using Safester.Utils;
using Safester.ViewModels;
using Xamarin.Forms;

namespace Safester.Views
{
    public partial class SettingsPage : ContentPage
    {
        private SettingsViewModel viewModel { get; set; }
        private string _curLanguage = string.Empty;
        private bool hasValidCoupon = false;

        public SettingsPage()
        {
            InitializeComponent();

            viewModel = new SettingsViewModel();

            pickerLanguage.Items.Add("French");
            pickerLanguage.Items.Add("English");

            if (string.IsNullOrEmpty(App.CurrentLanguage) == false && App.CurrentLanguage.Equals("fr", StringComparison.OrdinalIgnoreCase))
            {
                _curLanguage = "fr";
                pickerLanguage.SelectedIndex = 0;
            }                
            else
            {
                _curLanguage = "en";
                pickerLanguage.SelectedIndex = 1;
            }

            pickerLanguage.SelectedIndexChanged += PickerLanguage_SelectedIndexChanged;

            if (ThemeHelper.CurrentTheme == ThemeStyle.STANDARD_THEME)
                switchDarkMode.IsToggled = false;
            else
                switchDarkMode.IsToggled = true;

            switchDarkMode.Toggled += SwitchDarkMode_Toggled;

            btnSignature.Clicked += (sender, e) =>
            {
                var popupPage = new SignatureInputPage();
                PopupNavigation.Instance.PushAsync(popupPage);
            };

            btnSaveCoupon.Clicked += async (sender, e) =>
            {
                if (string.IsNullOrEmpty(entryCoupon.Text))
                {
                    CustomAlertPage.Show(AppResources.Warning, "Please input coupon first.", AppResources.OK, AppResources.Cancel);
                    return;
                }

                if (hasValidCoupon)
                {
                    var result = await CustomAlertPage.Show(AppResources.Warning, AppResources.ModifyCoupon, AppResources.OK, AppResources.Cancel);
                    if (result == false)
                        return;
                }

                ShowLoading(true);
                viewModel.StoreCouponCommand.Execute(entryCoupon.Text);
            };

            switchSignature.Toggled += (sender, e) =>
            {
                if (switchSignature.IsToggled)
                    btnSignature.IsEnabled = true;
                else
                    btnSignature.IsEnabled = false;
            };
        }

        private void SwitchDarkMode_Toggled(object sender, ToggledEventArgs e)
        {
            try
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    bool isDarkMode = switchDarkMode.IsToggled;

                    var settingsService = DependencyService.Get<SettingsService>();
                    settingsService.SaveSettings("app_theme", isDarkMode ? "1" : "0");

                    if (!isDarkMode)
                    {
                        ThemeHelper.ChangeTheme(ThemeStyle.STANDARD_THEME);
                    }
                    else
                    {
                        ThemeHelper.ChangeTheme(ThemeStyle.DARK_THEME);
                    }

                    ChangeTheme();
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private async void PickerLanguage_SelectedIndexChanged(object sender, EventArgs e)
        {
            var selectedLang = string.Empty;
            if (pickerLanguage.SelectedIndex == 0)
                selectedLang = "fr";
            else
                selectedLang = "en";

            if (selectedLang.Equals(_curLanguage, StringComparison.OrdinalIgnoreCase) == false)
            {
                var result = await CustomAlertPage.Show(AppResources.Warning, "The App will be closed in order to update default language in use.", AppResources.OK, AppResources.Cancel);

                if (result)
                {
                    var settingsService = DependencyService.Get<SettingsService>();
                    settingsService.SaveSettings("app_language", selectedLang);

                    settingsService.CloseApplication();
                }
                else
                {
                    if (string.IsNullOrEmpty(_curLanguage) == false && _curLanguage.Equals("fr", StringComparison.OrdinalIgnoreCase))
                    {
                        pickerLanguage.SelectedIndex = 0;
                    }
                    else
                    {
                        pickerLanguage.SelectedIndex = 1;
                    }
                }
            }
        }

        void Save_Clicked(object sender, EventArgs e)
        {
            UpdateUI(false);

            ShowLoading(true);

            viewModel.ResponseAction = (success) =>
            {
                ShowLoading(false);
                if (success == false)
                    CustomAlertPage.Show(AppResources.Error, AppResources.ErrorOccured, AppResources.OK);
                else
                {
                    CustomAlertPage.Show(AppResources.Success, AppResources.SettingsSaved, AppResources.OK);
                    Utils.Utils.SaveDataToFile(App.UserSettings, Utils.Utils.KEY_FILE_USERSETTINGS, true);
                }
            };

            viewModel.SaveSettingsCommand.Execute(null);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            UpdateUI(true);
            ShowLoading(true);

            viewModel.ResponseAction = (success) =>
            {
                ShowLoading(false);
                if (success == false)
                    CustomAlertPage.Show(AppResources.Error, AppResources.ErrorOccured, AppResources.OK);
                else
                {
                    UpdateUI(true);
                    Utils.Utils.SaveDataToFile(App.UserSettings, Utils.Utils.KEY_FILE_USERSETTINGS, true);
                }
            };

            viewModel.ResponseCouponAction = (getcoupon, success, coupon) =>
            {
                ShowLoading(false);
                if (success == false)
                {
                    if (getcoupon == false)
                        CustomAlertPage.Show(AppResources.Error, AppResources.InvalidCoupon, AppResources.OK);
                }
                else
                {
                    hasValidCoupon = false;
                    entryCoupon.Text = string.Empty;

                    if (string.IsNullOrEmpty(coupon) == false)
                    {
                        if (coupon.Equals("null", StringComparison.OrdinalIgnoreCase))
                        {
                            if (getcoupon == false)
                                CustomAlertPage.Show(AppResources.Error, AppResources.InvalidCoupon, AppResources.OK);

                            entryCoupon.Text = string.Empty;
                        }
                        else
                        {
                            hasValidCoupon = true;
                            entryCoupon.Text = coupon;
                        }
                    }
                }
            };

            viewModel.LoadSettingsCommand.Execute(null);
            viewModel.LoadCouponCommand.Execute(null);
        }

        private void UpdateUI(bool loading)
        {
            var _settingsService = DependencyService.Get<SettingsService>();
            if (loading)
            {
                entryName.Text = HttpUtility.HtmlDecode(App.UserSettings.name);
                entryProduct.Text = Utils.Utils.GetProductName(App.UserSettings.product);
                entryCrypto.Text = App.UserSettings.cryptographyInfo;
                entryMailBoxSize.Text = Utils.Utils.GetSizeString(App.UserSettings.mailboxSize);
                entryEmail.Text = App.UserSettings.notificationEmail;
                switchNotification.IsToggled = App.UserSettings.notificationOn;

                var messagesPerScroll = _settingsService.LoadSettings("messages_per_scroll");
                entryCountPerScroll.Text = Utils.Utils.GetCountPerScroll(messagesPerScroll).ToString();

                var enableSignature = _settingsService.LoadSettings("enable_mobile_signature");
                if (string.IsNullOrEmpty(enableSignature) == false && enableSignature.Equals("1"))
                    switchSignature.IsToggled = true;
                else
                    switchSignature.IsToggled = false;

                var enableAllowSending = _settingsService.LoadSettings("enable_allow_sending");
                if (string.IsNullOrEmpty(enableAllowSending) == false && enableAllowSending.Equals("1"))
                    switchConfirmSending.IsToggled = true;
                else
                    switchConfirmSending.IsToggled = false;

                //entrySignature.Text = _settingsService.LoadSettings("mobile_signature");
            }
            else
            {
                App.UserSettings.name = HttpUtility.HtmlEncode(entryName.Text);
                App.UserSettings.notificationEmail = entryEmail.Text;
                App.UserSettings.notificationOn = switchNotification.IsToggled;
                //_settingsService.SaveSettings("mobile_signature", string.IsNullOrEmpty(entrySignature.Text) ? "" : entrySignature.Text);
                _settingsService.SaveSettings("messages_per_scroll", string.IsNullOrEmpty(entryCountPerScroll.Text) ? "" : entryCountPerScroll.Text);

                _settingsService.SaveSettings("enable_mobile_signature", switchSignature.IsToggled ? "1" : "0");

                _settingsService.SaveSettings("enable_allow_sending", switchConfirmSending.IsToggled ? "1" : "0");
            }

            if (switchSignature.IsToggled)
                btnSignature.IsEnabled = true;
            else
                btnSignature.IsEnabled = false;

            ChangeTheme();
        }

        private void ShowLoading(bool isShowing)
        {
            if (isShowing)
                UserDialogs.Instance.Loading(AppResources.Pleasewait, null, null, true);
            else
                UserDialogs.Instance.Loading().Hide();
        }

        private async void ResetUser_Clicked(object sender, System.EventArgs e)
        {
            var result = await CustomAlertPage.Show(AppResources.Warning, AppResources.ConfirmResetUsers, AppResources.OK, AppResources.Cancel);

            if (result == true)
            {
                App.ConnectedUsers = new System.Collections.ObjectModel.ObservableCollection<User>();
                App.LocalUsers = new System.Collections.ObjectModel.ObservableCollection<User>();

                Utils.Utils.SaveDataToFile(App.LocalUsers, Utils.Utils.KEY_FILE_USERS);
            }
        }

        private void ChangeTheme()
        {
            BackgroundColor = ThemeHelper.GetListPageBGColor();

            lblName.TextColor = ThemeHelper.GetSettingsLabelColor();
            lblProduct.TextColor = ThemeHelper.GetSettingsLabelColor();
            lblCrypto.TextColor = ThemeHelper.GetSettingsLabelColor();
            lblBoxSize.TextColor = ThemeHelper.GetSettingsLabelColor();
            lblNotification.TextColor = ThemeHelper.GetSettingsLabelColor();
            lblAllowNotification.TextColor = ThemeHelper.GetSettingsLabelColor();
            lblMessagePerScroll.TextColor = ThemeHelper.GetSettingsLabelColor();

            lblConfirmSending.TextColor = ThemeHelper.GetSettingsLabelColor();
            lblAllowConfirmSending.TextColor = ThemeHelper.GetSettingsLabelColor();

            lblEmail.TextColor = ThemeHelper.GetSettingsLabelColor();
            lblAddSignature.TextColor = ThemeHelper.GetSettingsLabelColor();
            lblSignature.TextColor = ThemeHelper.GetSettingsLabelColor();
            lblLanguage.TextColor = ThemeHelper.GetSettingsLabelColor();
            lblTheme.TextColor = ThemeHelper.GetSettingsLabelColor();
            lblResetUser.TextColor = ThemeHelper.GetSettingsLabelColor();
            lblThemeName.TextColor = ThemeHelper.GetSettingsLabelColor();
            lblCoupon.TextColor = ThemeHelper.GetSettingsLabelColor();

            pickerLanguage.TextColor = ThemeHelper.GetSettingsEntryColor();
            //pickerTheme.TextColor = ThemeHelper.GetSettingsEntryColor();

            entryName.TextColor = ThemeHelper.GetSettingsEntryColor();
            entryProduct.TextColor = ThemeHelper.GetSettingsEntryColor();
            entryCrypto.TextColor = ThemeHelper.GetSettingsEntryColor();
            entryMailBoxSize.TextColor = ThemeHelper.GetSettingsEntryColor();
            entryEmail.TextColor = ThemeHelper.GetSettingsEntryColor();
            entryCountPerScroll.TextColor = ThemeHelper.GetSettingsEntryColor();
            entryCoupon.TextColor = ThemeHelper.GetSettingsEntryColor();

            pickerLanguage.BackgroundColor = ThemeHelper.GetReadMailBGColor();

            entryName.BackgroundColor = ThemeHelper.GetReadMailBGColor();
            entryMailBoxSize.BackgroundColor = ThemeHelper.GetReadMailBGColor();
            entryEmail.BackgroundColor = ThemeHelper.GetReadMailBGColor();
            entryProduct.BackgroundColor = ThemeHelper.GetReadMailBGColor();
            entryCrypto.BackgroundColor = ThemeHelper.GetReadMailBGColor();
            entryCountPerScroll.BackgroundColor = ThemeHelper.GetReadMailBGColor();
            entryCoupon.BackgroundColor = ThemeHelper.GetReadMailBGColor();

            switchDarkMode.ColorChangedEvent?.Invoke();
            switchNotification.ColorChangedEvent?.Invoke();
            switchSignature.ColorChangedEvent?.Invoke();
            switchConfirmSending.ColorChangedEvent?.Invoke();
        }
    }
}
