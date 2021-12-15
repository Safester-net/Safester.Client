using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using Safester.Custom.Effects;
using Safester.Utils;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Safester.Controls
{
    public enum ALERTTYPE
    {
        Simple = 0,
        Picker = 1,
        Information = 2,
    }

    public partial class CustomAlertPage : PopupPage
    {
        private static TaskCompletionSource<bool> _taskCompletion;
        private static CustomAlertPage _instance;

        public static async Task<bool> Show(string title, string description, string okstr, string cancelstr = "", string inputDescription = "", string inputDefault = "", string inputExpected = "")
        {
            _taskCompletion = new TaskCompletionSource<bool>();

            var alertPage = new CustomAlertPage(ALERTTYPE.Simple, title, description, okstr, cancelstr, inputDescription, inputDefault, inputExpected);
            _instance = alertPage;

            await PopupNavigation.Instance.PushAsync(alertPage);

            return await _taskCompletion.Task;
        }

        public static async Task<bool> Show(ALERTTYPE type, string title, string description, string okstr, string cancelstr, object param)
        {
            _taskCompletion = new TaskCompletionSource<bool>();

            var alertPage = new CustomAlertPage(type, title, description, okstr, cancelstr, "", "", "", param);
            _instance = alertPage;

            await PopupNavigation.Instance.PushAsync(alertPage);

            return await _taskCompletion.Task;
        }

        public static int GetSelectedIndex()
        {
            if (_instance != null)
                return _instance.IndexOfOption;

            return 0;
        }

        private string _inputExpected { get; set; }
        private ALERTTYPE _type { get; set; }

        public int IndexOfOption = 0;

        public CustomAlertPage(ALERTTYPE type, string title, string description, string okstr, string cancelstr = "", string inputDescription = "", string inputDefault = "", string inputExpected = "", object param = null)
        {
            InitializeComponent();

            _type = type;
            lblTitle.Text = title;
            lblDescription.Text = description;
            lblClose.Text = okstr.ToUpper();
            lblCancel.Text = cancelstr.ToUpper();

            if (string.IsNullOrEmpty(title))
                lblTitle.IsVisible = false;

            if (string.IsNullOrEmpty(cancelstr))
                cancelLayout.IsVisible = false;

            switch (type)
            {
                case ALERTTYPE.Simple:
                    if (string.IsNullOrEmpty(inputDescription) == false)
                    {
                        inputLayout.IsVisible = true;
                        lblInputDescription.Text = inputDescription;
                        entryInput.Text = inputDefault;
                        _inputExpected = inputExpected;
                    }
                    break;
                case ALERTTYPE.Information:
                    informationLayout.IsVisible = true;
                    lblInformation.Text = param != null ? param as string : "";
                    break;
                case ALERTTYPE.Picker:
                    pickerLayout.IsVisible = true;
                    if (param != null)
                    {
                        pickerOptions.ItemsSource = param as string[];
                        pickerOptions.SelectedIndex = 0;
                        IndexOfOption = 0;

                        pickerOptions.SelectedIndexChanged += (sender, args) =>
                        {
                            IndexOfOption = pickerOptions.SelectedIndex;
                        };
                    }

                    break;
                default:
                    break;
            }

            TapGestureRecognizer closeGesture = new TapGestureRecognizer();
            closeGesture.Tapped += BtnClose_Clicked;
            closeLayout.GestureRecognizers.Add(closeGesture);

            TapGestureRecognizer cancelGesture = new TapGestureRecognizer();
            cancelGesture.Tapped += BtnCancel_Clicked;
            cancelLayout.GestureRecognizers.Add(cancelGesture);

            TapGestureRecognizer copyGesture = new TapGestureRecognizer();
            copyGesture.Tapped += CopyBtn_Clicked;
            ImgCopy.GestureRecognizers.Add(copyGesture);

            lblCopyHint.Text = AppResources.CopyHint;
            TapGestureRecognizer copyHintGesture = new TapGestureRecognizer();
            copyHintGesture.Tapped += CopyBtn_Clicked;
            lblCopyHint.GestureRecognizers.Add(copyHintGesture);

            ChangeTheme();

            lblInformation.Effects.Add(new ShowHidePassEffect());
            lblInformation.Focused += LblInformation_Focused;
        }

        private void LblInformation_Focused(object sender, FocusEventArgs e)
        {
            lblInformation.Focus();
            lblInformation.Unfocus();
        }

        private void ChangeTheme()
        {
            if (ThemeHelper.CurrentTheme == ThemeStyle.DARK_THEME)
            {
                mainLayout.BackgroundColor = (Color)App.Current.Resources["DarkPanelBackgroundColor"];
                lblTitle.TextColor = (Color)App.Current.Resources["LightBackgroundColor"];
                lblDescription.TextColor = (Color)App.Current.Resources["DarkMessageTextColor"];
                lblInputDescription.TextColor = (Color)App.Current.Resources["DarkMessageTextColor"];
                entryInput.TextColor = (Color)App.Current.Resources["DarkMessageTextColor"];
                lblClose.TextColor = (Color)App.Current.Resources["DarkOptionTextColor"];
                lblCancel.TextColor = (Color)App.Current.Resources["DarkOptionTextColor"];
                lblCopyHint.TextColor = (Color)App.Current.Resources["DarkOptionTextColor"];
            }
            else
            {
                mainLayout.BackgroundColor = (Color)App.Current.Resources["LightBackgroundColor"];
                lblTitle.TextColor = Color.Black;
                lblDescription.TextColor = Color.Black;
                lblInputDescription.TextColor = Color.Black;
                entryInput.TextColor = Color.Black;
                lblClose.TextColor = (Color)App.Current.Resources["PrimaryDark"];
                lblCancel.TextColor = (Color)App.Current.Resources["PrimaryDark"];
                lblCopyHint.TextColor = (Color)App.Current.Resources["PrimaryDark"];
            }

            inputLine.Color = ThemeHelper.GetSearchEntryBorderColor();
        }

        private async void BtnClose_Clicked(object sender, EventArgs e)
        {
            await PopupNavigation.Instance.PopAllAsync();

            if (string.IsNullOrEmpty(_inputExpected) == false)
            {
                if (string.IsNullOrEmpty(entryInput.Text) == false)
                {
                    if (entryInput.Text.ToLower().Equals(_inputExpected.ToLower()))
                        _taskCompletion?.TrySetResult(true);
                    else
                        _taskCompletion?.TrySetResult(false);
                }
                else
                {
                    _taskCompletion?.TrySetResult(false);
                }
            }
            else
            {
                _taskCompletion?.TrySetResult(true);
            }
        }

        private async void BtnCancel_Clicked(object sender, EventArgs e)
        {
            await PopupNavigation.Instance.PopAllAsync();

            _taskCompletion?.TrySetResult(false);
        }

        private async void CopyBtn_Clicked(object sender, EventArgs e)
        {
            try
            {
                await Clipboard.SetTextAsync(lblInformation.Text);

                await DisplayAlert("", AppResources.ClipboardPassPhraseSuccess, AppResources.OK);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
