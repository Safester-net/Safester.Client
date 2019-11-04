using Safester.Controls;
using Safester.Models;
using Safester.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Web;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Safester.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MenuPage : ContentPage
    {
        ObservableCollection<HomeMenuItem> menuItems;
        public MenuPage()
        {
            InitializeComponent();

            if (Device.RuntimePlatform == Device.Android)
            {
                menuItems = new ObservableCollection<HomeMenuItem>
                {
                    new HomeMenuItem {Id = MenuItemType.Users, Title=AppResources.Users, Image="keys.png" },
                    new HomeMenuItem {Id = MenuItemType.Compose, Title=AppResources.Compose, Image="compose.png" },
                    new HomeMenuItem {Id = MenuItemType.Inbox, Title=AppResources.Inbox, Image="inbox.png" },
                    new HomeMenuItem {Id = MenuItemType.Sent, Title=AppResources.Sent, Image="inbox_out.png" },
                    new HomeMenuItem {Id = MenuItemType.Search, Title=AppResources.Search, Image="find.png" },
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
                menuItems = new ObservableCollection<HomeMenuItem>
                {
                    new HomeMenuItem {Id = MenuItemType.Users, Title=AppResources.Users, Image="keys.png" },
                    new HomeMenuItem {Id = MenuItemType.Compose, Title=AppResources.Compose, Image="compose.png" },
                    new HomeMenuItem {Id = MenuItemType.Inbox, Title=AppResources.Inbox, Image="inbox.png" },
                    new HomeMenuItem {Id = MenuItemType.Sent, Title=AppResources.Sent, Image="inbox_out.png" },
                    new HomeMenuItem {Id = MenuItemType.Search, Title=AppResources.Search, Image="find.png" },
                    new HomeMenuItem {Id = MenuItemType.Drafts, Title=AppResources.Drafts, Image="document.png" },
                    new HomeMenuItem {Id = MenuItemType.Contacts, Title=AppResources.Contacts, Image="business.png" },
                    new HomeMenuItem {Id = MenuItemType.Settings, Title=AppResources.Settings, Image="settings.png" },
                    new HomeMenuItem {Id = MenuItemType.About, Title=AppResources.About, Image="information.png" },
                    new HomeMenuItem {Id = MenuItemType.Logout, Title=AppResources.Logout, Image="log_out.png" }
                };
            }

            ListViewMenu.ItemsSource = menuItems;

            ListViewMenu.SelectedItem = menuItems[2];
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

        public void ChangeTheme()
        {
            BackgroundColor = ThemeHelper.GetMenuBGColor();
            ListViewMenu.BackgroundColor = ThemeHelper.GetMenuBGColor();

            var selectedItem = ListViewMenu.SelectedItem;
            if (Device.RuntimePlatform == Device.iOS)
            {
                int idx = -1;
                if (selectedItem != null)
                    idx = menuItems.IndexOf(selectedItem as HomeMenuItem);

                ListViewMenu.ItemsSource = null;
                ListViewMenu.ItemsSource = menuItems;

                if (idx != -1)
                {
                    ListViewMenu.SelectedItem = null;
                    ListViewMenu.SelectedItem = menuItems[idx];
                }
            }
            else
            {
                if (selectedItem != null)
                {
                    ListViewMenu.SelectedItem = null;

                    if (selectedItem is HomeMenuItem)
                    {
                        if ((selectedItem as HomeMenuItem).Id != MenuItemType.Logout &&
                            (selectedItem as HomeMenuItem).Id != MenuItemType.Compose)
                            ListViewMenu.SelectedItem = selectedItem;
                    }
                }
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            labelName.Text = App.CurrentUser.UserName;
            labelEmail.Text = App.CurrentUser.UserEmail;
        }
    }
}