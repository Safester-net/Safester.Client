using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Safester.Views;
using Safester.Models;
using Safester.CryptoLibrary.Api;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Plugin.Multilingual;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace Safester
{
    public partial class App : Application
    {
        public static User CurrentUser { get; set; }
        public static SettingsInfo UserSettings { get; set; }

        public static Decryptor KeyDecryptor;
        public static Encryptor KeyEncryptor;

        // Local Temp Data
        public static ObservableCollection<Recipient> Recipients { get; set; }
        public static ObservableCollection<DraftMessage> DraftMessages { get; set; }

        public App()
        {
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("dummy");

            InitializeComponent();
            AppResources.Culture = CrossMultilingual.Current.DeviceCultureInfo;

            bool armor = false;
            bool withIntegrityCheck = true;
            KeyEncryptor = new Encryptor(armor, withIntegrityCheck);

            CurrentUser = new User();

            Recipients = Utils.Utils.LoadDataFromFile<ObservableCollection<Recipient>>(Utils.Utils.KEY_FILE_RECIPIENTS);
            if (Recipients == null)
                Recipients = new ObservableCollection<Recipient>();

            UserSettings = Utils.Utils.LoadDataFromFile<SettingsInfo>(Utils.Utils.KEY_FILE_USERSETTINGS);
            if (UserSettings == null)
                UserSettings = new SettingsInfo();

            DraftMessages = Utils.Utils.LoadDataFromFile<ObservableCollection<DraftMessage>>(Utils.Utils.KEY_FILE_DRAFTMESSAGES);
            if (DraftMessages == null)
                DraftMessages = new ObservableCollection<DraftMessage>();

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
