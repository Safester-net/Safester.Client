using Safester.Models;
using System;
using System.Collections.Generic;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Safester.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MenuPage : ContentPage
    {
        List<HomeMenuItem> menuItems;
        public MenuPage()
        {
            InitializeComponent();

            if (Device.RuntimePlatform == Device.Android)
            {
                menuItems = new List<HomeMenuItem>
                {
                    new HomeMenuItem {Id = MenuItemType.Compose, Title=AppResources.Compose, Image="compose.png" },
                    new HomeMenuItem {Id = MenuItemType.Inbox, Title=AppResources.Inbox, Image="inbox.png" },
                    new HomeMenuItem {Id = MenuItemType.Sent, Title=AppResources.Sent, Image="inbox_out.png" },
                    new HomeMenuItem {Id = MenuItemType.Drafts, Title=AppResources.Drafts, Image="document.png" },
                    //new HomeMenuItem {Id = MenuItemType.Trash, Title="Trash", Image="recycle.png" },
                    new HomeMenuItem {Id = MenuItemType.Contacts, Title=AppResources.Contacts, Image="business.png" },
                    new HomeMenuItem {Id = MenuItemType.Settings, Title=AppResources.Settings, Image="settings.png" },
                    new HomeMenuItem {Id = MenuItemType.TwoFactorSettings, Title=AppResources.TwoFactorAuthentication, Image="authentication.png" },
                    new HomeMenuItem {Id = MenuItemType.ChangePass, Title=AppResources.ChangePass, Image="password.png" },
                    new HomeMenuItem {Id = MenuItemType.About, Title=AppResources.About, Image="information.png" },
                    new HomeMenuItem {Id = MenuItemType.Logout, Title=AppResources.Logout, Image="log_out.png" }
                };
            }
            else
            {
                menuItems = new List<HomeMenuItem>
                {
                    new HomeMenuItem {Id = MenuItemType.Compose, Title=AppResources.Compose, Image="compose.png" },
                    new HomeMenuItem {Id = MenuItemType.Inbox, Title=AppResources.Inbox, Image="inbox.png" },
                    new HomeMenuItem {Id = MenuItemType.Sent, Title=AppResources.Sent, Image="inbox_out.png" },
                    new HomeMenuItem {Id = MenuItemType.Drafts, Title=AppResources.Drafts, Image="document.png" },
                    new HomeMenuItem {Id = MenuItemType.Contacts, Title=AppResources.Contacts, Image="business.png" },
                    new HomeMenuItem {Id = MenuItemType.Settings, Title=AppResources.Settings, Image="settings.png" },
                    new HomeMenuItem {Id = MenuItemType.Logout, Title=AppResources.Logout, Image="log_out.png" }
                };
            }

            ListViewMenu.ItemsSource = menuItems;

            ListViewMenu.SelectedItem = menuItems[1];
            ListViewMenu.ItemSelected += async (sender, e) =>
            {
                if (e.SelectedItem == null)
                    return;

                var item = (HomeMenuItem)e.SelectedItem;

                await MainPage.MainMasterPage.NavigateFromMenu(item);

                if (item.Id == MenuItemType.Logout ||
                    item.Id == MenuItemType.Compose)
                    ListViewMenu.SelectedItem = null;
            };
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            labelName.Text = App.UserSettings.name;
            labelEmail.Text = App.CurrentUser.UserEmail;
        }
    }
}