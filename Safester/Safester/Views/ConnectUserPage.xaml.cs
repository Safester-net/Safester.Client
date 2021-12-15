using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Safester.Controls;
using Safester.Models;
using Safester.Services;
using Safester.Utils;
using Xamarin.Forms;

namespace Safester.Views
{
    public partial class ConnectUserPage : ContentPage
    {
        public static ObservableCollection<User> Users { get; set; }

        public ConnectUserPage()
        {
            InitializeComponent();

            Users = new ObservableCollection<User>();
            if (App.LocalUsers != null && App.LocalUsers.Count > 0)
            {
                foreach (var item in App.LocalUsers)
                {
                    bool isExisting = false;
                    if (App.ConnectedUsers != null && App.ConnectedUsers.Count > 0)
                    {
                        if (App.ConnectedUsers.Any(x => x.UserEmail.Equals(item.UserEmail)))
                            isExisting = true;
                    }

                    if (item.UserEmail.Equals(App.CurrentUser.UserEmail))
                        isExisting = true;

                    if (isExisting == false)
                        Users.Add(item);
                }
            }

            UsersListView.ItemsSource = Users;

            BackgroundColor = ThemeHelper.GetReadMailBGColor();
        }

        async void OnItemSelected(object sender, SelectedItemChangedEventArgs args)
        {
            var item = args.SelectedItem as User;
            if (item == null)
                return;

            // Manually deselect item.
            UsersListView.SelectedItem = null;

            if (item.UserEmail.Equals(App.CurrentUser.UserEmail, StringComparison.OrdinalIgnoreCase)) // current logged in user
                return;

            string msg = AppResources.ChangeAccount + "\"" + item.UserEmail + "\"?";
            bool result = await CustomAlertPage.Show(AppResources.Warning, msg, AppResources.Yes, AppResources.Cancel);
            if (result)
            {
                LoginPage.CurrentUserEmail = item.UserEmail;
                LoginPage.NeedsUpdating = true;
                LoginPage.CurrentUserPassword = "";
                Navigation.PushAsync(new LoginPage(true));
            }
        }

        void AddAccount_Clicked(object sender, System.EventArgs e)
        {
            LoginPage.CurrentUserEmail = "";
            LoginPage.CurrentUserPassword = "";
            LoginPage.NeedsUpdating = true;
            Navigation.PushAsync(new LoginPage(true));
        }

        async void OnDelete(object sender, System.EventArgs e)
        {
            var mi = ((MenuItem)sender);
            var data = (mi.CommandParameter as User);

            var message = AppResources.DeleteAccount + " \"" + data.UserEmail + "\"";
            bool result = await CustomAlertPage.Show(AppResources.Warning, message, AppResources.Yes, AppResources.Cancel);
            if (result)
            {
                try
                {
                    App.LocalUsers.Remove(data);
                    Users.Remove(data);
                    Utils.Utils.SaveDataToFile(App.LocalUsers, Utils.Utils.KEY_FILE_USERS);

                    UsersListView.ItemsSource = null;
                    UsersListView.ItemsSource = Users;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }
    }
}
