using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Safester.Views;
using Safester.Models;
using Safester.CryptoLibrary.Api;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Plugin.Multilingual;
using Safester.Services;
using Safester.Utils;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace Safester
{
    public partial class App : Application
    {
        public static User CurrentUser { get; set; }
        public static SettingsInfo UserSettings { get; set; }

        public static Decryptor KeyDecryptor;
        public static Encryptor KeyEncryptor;

        public static string CurrentLanguage { get; set; }

        public static ObservableCollection<Recipient> Recipients { get; set; }
        public static ObservableCollection<DraftMessage> DraftMessages { get; set; }

        public static ObservableCollection<User> LocalUsers { get; set; }
        public static ObservableCollection<User> ConnectedUsers { get; set; }

        public App()
        {
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("ODU3NjVAMzEzNzJlMzEyZTMwZ1hwRDBmQkVIRWszNE5STkNGUmQ4RkJRdVA0U1J6aFp0aFlFUUNGci9IZz0=");

            InitializeComponent();

            bool armor = false;
            bool withIntegrityCheck = true;
            KeyEncryptor = new Encryptor(armor, withIntegrityCheck);

            CurrentUser = new User();
            LocalUsers = Utils.Utils.LoadDataFromFile<ObservableCollection<User>>(Utils.Utils.KEY_FILE_USERS);
            if (LocalUsers == null)
                LocalUsers = new ObservableCollection<User>();

            App.Recipients = Utils.Utils.LoadDataFromFile<ObservableCollection<Recipient>>(Utils.Utils.KEY_FILE_RECIPIENTS);
            if (App.Recipients == null)
                App.Recipients = new ObservableCollection<Recipient>();

            var settingsService = DependencyService.Get<SettingsService>();
            var language = settingsService.LoadSettings("app_language");
            if (string.IsNullOrEmpty(language))
            {
                if (CrossMultilingual.Current.DeviceCultureInfo.TwoLetterISOLanguageName == "fr")
                {
                    language = "fr";
                }
                else
                {
                    language = "en";
                }
            }

            CurrentLanguage = language;
            var currentCulture = new System.Globalization.CultureInfo(language);
            CrossMultilingual.Current.CurrentCultureInfo = currentCulture;
            AppResources.Culture = currentCulture;
            settingsService.SaveSettings("app_language", language);

            var themeStyle = settingsService.LoadSettings("app_theme");
            if (string.IsNullOrEmpty(themeStyle))
            {
                themeStyle = "0";
            }

            if (themeStyle.Equals("0"))
                ThemeHelper.ChangeTheme(ThemeStyle.STANDARD_THEME);
            else
                ThemeHelper.ChangeTheme(ThemeStyle.DARK_THEME);

            MainPage = new NavigationPage(new LoginPage());
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
