using System;
using System.Collections.Generic;
using Safester.Controls;
using Safester.Models;
using Safester.Utils;
using Xamarin.Forms;

namespace Safester.Views
{
    public partial class DraftItemsPage : ContentPage
    {
        public DraftItemsPage()
        {
            InitializeComponent();
        }

        async void OnItemSelected(object sender, SelectedItemChangedEventArgs args)
        {
            var item = args.SelectedItem as DraftMessage;
            if (item == null)
                return;

            var itemsPage = new NewItemPage(item);
            await Navigation.PushAsync(itemsPage);

            // Manually deselect item.
            ItemsListView.SelectedItem = null;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            LoadData();

            BackgroundColor = ThemeHelper.GetReadMailBGColor();
        }

        private void LoadData()
        {
            if (App.DraftMessages != null)
            {
                foreach (var item in App.DraftMessages)
                {
                    item.ShowToRecipients = string.Empty;
                    if (item.ToRecipients != null && item.ToRecipients.Count > 0)
                        item.ShowToRecipients = string.Join(";", item.ToRecipients);
                }
            }

            ItemsListView.ItemsSource = App.DraftMessages;
        }

        async void ComposeItem_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new NewItemPage());
        }

        async void OnDelete(object sender, System.EventArgs e)
        {
            bool result = await CustomAlertPage.Show(AppResources.Warning, AppResources.DeleteMail, AppResources.Yes, AppResources.Cancel);
            if (result)
            {
                var mi = ((MenuItem)sender);
                var draftData = mi.CommandParameter as DraftMessage;

                int idx = App.DraftMessages.IndexOf(draftData);
                App.DraftMessages.RemoveAt(idx);
                Utils.Utils.SaveDataToFile(App.DraftMessages, Utils.Utils.KEY_FILE_DRAFTMESSAGES, true);

                LoadData();
            }
        }
    }
}
