using Acr.UserDialogs;
using Safester.Controls;
using Safester.Models;
using Safester.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Safester.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage : MasterDetailPage
    {
        public static MainPage MainMasterPage { get; set; }

        public MainPage()
        {
            InitializeComponent();

            MainMasterPage = this;

            NavigationPage.SetHasNavigationBar(this, false);

            MasterBehavior = MasterBehavior.Popover;
            Detail = new NavigationPage(new ItemsPage(MenuItemType.Inbox, AppResources.Inbox));

            ChangeMenuTheme();
        }

        public void ChangeMenuTheme()
        {
            (Master as MenuPage).ChangeTheme();
        }

        public async Task NavigateFromMenu(HomeMenuItem item)
        {
            IsPresented = false;
            bool isPageInbox = true;

            switch (item.Id)
            {
                case MenuItemType.Compose:
                    await Navigation.PushAsync(new NewItemPage());
                    break;
                case MenuItemType.Inbox:
                    Detail = new NavigationPage(new ItemsPage(MenuItemType.Inbox, AppResources.Inbox));
                    break;
                case MenuItemType.Sent:
                    Detail = new NavigationPage(new ItemsPage(MenuItemType.Sent, AppResources.Sent));
                    break;
                case MenuItemType.Starred:
                    if (Detail != null && Detail is NavigationPage)
                    {
                        var currentPage = Detail as NavigationPage;
                        if (currentPage.CurrentPage is ItemsPage)
                            isPageInbox = (currentPage.CurrentPage as ItemsPage).ItemType != MenuItemType.Sent;
                    }

                    Detail = new NavigationPage(new ItemsPage(MenuItemType.Starred, AppResources.Starred, isPageInbox));
                    break;
                case MenuItemType.Search:
                    await Navigation.PushAsync(new SearchPage(MenuItemType.Inbox));
                    break;
                case MenuItemType.Drafts:
                    Detail = new NavigationPage(new DraftItemsPage());
                    break;
                case MenuItemType.Contacts:
                    ImportContacts();
                    break;
                case MenuItemType.Settings:
                    Detail = new NavigationPage(new SettingsPage());
                    break;
                case MenuItemType.TwoFactorSettings:
                    Detail = new NavigationPage(new TwoFactorSettingsPage());
                    break;
                case MenuItemType.About:
                    Detail = new NavigationPage(new AboutPage());
                    break;
                case MenuItemType.Users:
                    Detail = new NavigationPage(new UsersPage());
                    break;
                case MenuItemType.Trash:
                    await CustomAlertPage.Show(AppResources.Warning, AppResources.AvailableSoon, AppResources.OK);
                    break;
                case MenuItemType.ChangePass:
                    await CustomAlertPage.Show(AppResources.ChangePass, AppResources.ChangePassHint, AppResources.OK);
                    break;
                case MenuItemType.Logout:
                    bool result = await CustomAlertPage.Show("", AppResources.ConfirmLogout, AppResources.Yes, AppResources.No);
                    if (result)
                    {
                        var _settingsService = DependencyService.Get<SettingsService>();
                        _settingsService.SaveSettings("rememberuser", "0");

                        LoginPage.CurrentUserEmail = App.CurrentUser.UserEmail;
                        LoginPage.CurrentUserPassword = String.Empty;
                        LoginPage.NeedsUpdating = true;

                        App.ConnectedUsers = new System.Collections.ObjectModel.ObservableCollection<User>();

                        App.Current.MainPage = new NavigationPage(new LoginPage());
                    }
                    break;
            }

            if (Device.RuntimePlatform == Device.Android)
                await Task.Delay(100);                
        }

        private async void ImportContacts()
        {
            bool result = await CustomAlertPage.Show("Safester", AppResources.ImportContacts, AppResources.OK, AppResources.Cancel);
            if (result)
            {
                var _settingsService = DependencyService.Get<SettingsService>();
                _settingsService.AskContactsPermission(ImportContactsWithPermission);
            }
        }

        private void ImportContactsWithPermission()
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                UserDialogs.Instance.Loading(AppResources.ImportContactsHint, null, null, true);

                await Utils.Utils.GetPhoneContacts();

                UserDialogs.Instance.Loading().Hide();
                await CustomAlertPage.Show("Safester", AppResources.ImportContactsSuccess, AppResources.OK);
            });
        }
    }
}