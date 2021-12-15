using System;
using System.Collections.Generic;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using Safester.Services;
using Xamarin.Forms;

namespace Safester.Views
{
    public partial class SignatureInputPage : PopupPage
    {
        public SignatureInputPage()
        {
            InitializeComponent();

            var _settingsService = DependencyService.Get<SettingsService>();
            entrySignature.Text = _settingsService.LoadSettings("mobile_signature");
        }

        void Save_Clicked(object sender, System.EventArgs e)
        {
            var _settingsService = DependencyService.Get<SettingsService>();
            _settingsService.SaveSettings("mobile_signature", string.IsNullOrEmpty(entrySignature.Text) ? "" : entrySignature.Text);
            PopupNavigation.Instance.PopAsync();
        }

        void Cancel_Clicked(object sender, System.EventArgs e)
        {
            PopupNavigation.Instance.PopAsync();
        }
    }
}
