using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using Safester.Utils;
using Xamarin.Forms;

namespace Safester.Controls
{
    public partial class CustomAlertPage : PopupPage
    {
        private static TaskCompletionSource<bool> _taskCompletion;
        public static async Task<bool> Show(string title, string description, string okstr, string cancelstr = "")
        {
            _taskCompletion = new TaskCompletionSource<bool>();

            var alertPage = new CustomAlertPage(title, description, okstr, cancelstr);
            await PopupNavigation.Instance.PushAsync(alertPage);

            return await _taskCompletion.Task;
        }

        public CustomAlertPage(string title, string description, string okstr, string cancelstr = "")
        {
            InitializeComponent();

            lblTitle.Text = title;
            lblDescription.Text = description;
            lblClose.Text = okstr.ToUpper();
            lblCancel.Text = cancelstr.ToUpper();

            if (string.IsNullOrEmpty(title))
                lblTitle.IsVisible = false;

            if (string.IsNullOrEmpty(cancelstr))
                cancelLayout.IsVisible = false;

            TapGestureRecognizer closeGesture = new TapGestureRecognizer();
            closeGesture.Tapped += BtnClose_Clicked;
            closeLayout.GestureRecognizers.Add(closeGesture);

            TapGestureRecognizer cancelGesture = new TapGestureRecognizer();
            cancelGesture.Tapped += BtnCancel_Clicked;
            cancelLayout.GestureRecognizers.Add(cancelGesture);

            ChangeTheme();
        }

        private void ChangeTheme()
        {
            if (ThemeHelper.CurrentTheme == ThemeStyle.DARK_THEME)
            {
                mainLayout.BackgroundColor = (Color)App.Current.Resources["DarkPanelBackgroundColor"];
                lblTitle.TextColor = (Color)App.Current.Resources["LightBackgroundColor"];
                lblDescription.TextColor = (Color)App.Current.Resources["DarkMessageTextColor"];
                lblClose.TextColor = (Color)App.Current.Resources["DarkOptionTextColor"];
                lblCancel.TextColor = (Color)App.Current.Resources["DarkOptionTextColor"];
            }
            else
            {
                mainLayout.BackgroundColor = (Color)App.Current.Resources["LightBackgroundColor"];
                lblTitle.TextColor = Color.Black;
                lblDescription.TextColor = Color.Black;
                lblClose.TextColor = (Color)App.Current.Resources["PrimaryDark"];
                lblCancel.TextColor = (Color)App.Current.Resources["PrimaryDark"];
            }
        }

        private async  void BtnClose_Clicked(object sender, EventArgs e)
        {
            _taskCompletion?.TrySetResult(true);
            await PopupNavigation.Instance.PopAllAsync();
        }

        private async void BtnCancel_Clicked(object sender, EventArgs e)
        {
            _taskCompletion?.TrySetResult(false);
            await PopupNavigation.Instance.PopAllAsync();
        }
    }
}
