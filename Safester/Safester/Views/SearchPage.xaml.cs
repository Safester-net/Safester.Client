using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Web;
using Acr.UserDialogs;
using Safester.Models;
using Safester.Network;
using Safester.Utils;
using Safester.ViewModels;
using Xamarin.Forms;

namespace Safester.Views
{
    public partial class SearchPage : ContentPage
    {
        MenuItemType _pageType { get; set; }

        public SearchPage(MenuItemType itemType)
        {
            InitializeComponent();
            _pageType = itemType;

            pickerFolder.Items.Add(AppResources.Inbox);
            pickerFolder.Items.Add(AppResources.Sent);
            if (itemType == MenuItemType.Sent)
                pickerFolder.SelectedIndex = 1;
            else
                pickerFolder.SelectedIndex = 0;

            suggestBoxTo.DataSource = App.Recipients;
            suggestBoxFrom.DataSource = App.Recipients;

            dateStart.Date = DateTime.Today.AddMonths(-1);
            dateEnd.Date = DateTime.Today;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            ChangeTheme();
        }

        async void Search_Clicked(object sender, System.EventArgs e)
        {
            int directoryId = 1;
            if (pickerFolder.SelectedIndex == 1)
                directoryId = 2;

            long dtStart = Utils.Utils.GetUnixEpochTimeStamp(dateStart.Date);
            long dtEnd = Utils.Utils.GetUnixEpochTimeStamp(new DateTime(dateEnd.Date.Year, dateEnd.Date.Month, dateEnd.Date.Day, 23, 59, 59));

            string senderSearch = string.Empty;
            if (suggestBoxFrom.SelectedItem != null)
            {
                if (suggestBoxFrom.SelectedItem is Recipient)
                {
                    var selectedItem = suggestBoxFrom.SelectedItem as Recipient;
                    if (selectedItem != null)
                        senderSearch = selectedItem.recipientEmailAddr;
                }
            }
            else if (string.IsNullOrEmpty(suggestBoxFrom.Text) == false)
            {
                senderSearch = suggestBoxFrom.Text;
            }
            senderSearch = HttpUtility.HtmlEncode(senderSearch);

            string recipientSearch = string.Empty;
            if (suggestBoxTo.SelectedItem != null)
            {
                if (suggestBoxTo.SelectedItem is Recipient)
                {
                    var selectedItem = suggestBoxTo.SelectedItem as Recipient;
                    if (selectedItem != null)
                        recipientSearch = selectedItem.recipientEmailAddr;
                }
            }
            else if (string.IsNullOrEmpty(suggestBoxTo.Text) == false)
            {
                recipientSearch = suggestBoxTo.Text;
            }
            recipientSearch = HttpUtility.HtmlEncode(recipientSearch);

            var searchText = entrySearchText.Text;
            if (string.IsNullOrEmpty(searchText))
                searchText = string.Empty;

            ShowLoading(true);
            var result = await ApiManager.SharedInstance().SearchMessageFull(App.CurrentUser.UserEmail, App.CurrentUser.Token, searchText, 
                senderSearch, recipientSearch, dtStart, dtEnd, directoryId);
            List<Message> searchMessageList = new List<Message>();

            if (result == null || result.Count == 0)
                ItemsListView.IsVisible = false;
            else
            {
                ItemsListView.IsVisible = true;
                foreach (var item in result)
                {
                    try
                    {
                        item.subject = HttpUtility.HtmlDecode(Utils.Utils.DecryptMessageData(App.KeyDecryptor, item.subject, true));

                        bool hasText = false;
                        if (string.IsNullOrEmpty(searchText) == true)
                            hasText = true;
                        else
                        {
                            if (item.subject.ToLower().Contains(searchText.ToLower()))
                                hasText = true;

                            if (hasText == false)
                            {
                                string decryptedBody = Utils.Utils.DecryptMessageData(App.KeyDecryptor, item.body, false);
                                decryptedBody = HttpUtility.HtmlDecode(decryptedBody);
                                decryptedBody = Utils.Utils.GetRemovedHtmlString(decryptedBody);

                                if (decryptedBody.ToLower().Contains(searchText.ToLower()))
                                    hasText = true;
                            }
                        }

                        if (hasText == false)
                            continue;

                        if (_pageType != MenuItemType.Inbox)
                        {
                            item.IsRead = true;

                            if (item.recipients != null && item.recipients.Count > 0)
                                item.SenderOrRecipient = item.recipients[0].displayName;
                        }
                        else
                        {
                            item.SenderOrRecipient = item.senderName;
                        }

                        item.SenderOrRecipient = HttpUtility.HtmlDecode(item.SenderOrRecipient);

                        searchMessageList.Add(item);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }

                ItemsListView.ItemsSource = searchMessageList;
            }

            ShowLoading(false);
        }

        async void OnItemSelected(object sender, SelectedItemChangedEventArgs args)
        {
            var item = args.SelectedItem as Message;
            if (item == null)
                return;

            await Navigation.PushAsync(new ItemDetailPage(new ItemDetailViewModel(item, pickerFolder.SelectedIndex == 0 ? MenuItemType.Inbox : MenuItemType.Sent)));

            // Manually deselect item.
            ItemsListView.SelectedItem = null;
        }

        private void ShowLoading(bool isShowing)
        {
            if (isShowing)
                UserDialogs.Instance.Loading(AppResources.Pleasewait, null, null, true);
            else
                UserDialogs.Instance.Loading().Hide();
        }

        private void ChangeTheme()
        {
            BackgroundColor = ThemeHelper.GetReadMailBGColor();

            lblFind.TextColor = ThemeHelper.GetReadMailLabelColor();

            lblSearchFrom.TextColor = ThemeHelper.GetReadMailLabelColor();
            suggestBoxFrom.TextColor = ThemeHelper.GetThemeTextColor();

            lblSearchTo.TextColor = ThemeHelper.GetReadMailLabelColor();
            suggestBoxTo.TextColor = ThemeHelper.GetThemeTextColor();

            lblFolder.TextColor = ThemeHelper.GetReadMailLabelColor();
            lblBetween.TextColor = ThemeHelper.GetReadMailLabelColor();
            lblAnd.TextColor = ThemeHelper.GetReadMailLabelColor();

            pickerFolder.TextColor = ThemeHelper.GetThemeTextColor();
            dateStart.TextColor = ThemeHelper.GetThemeTextColor();
            dateEnd.TextColor = ThemeHelper.GetThemeTextColor();

            suggestBoxTo.BorderColor = ThemeHelper.GetSearchEntryBorderColor();
            suggestBoxFrom.BorderColor = ThemeHelper.GetSearchEntryBorderColor();

            suggestBoxTo.DropDownBackgroundColor = ThemeHelper.GetReadMailBGColor();
            suggestBoxFrom.DropDownBackgroundColor = ThemeHelper.GetReadMailBGColor();

            pickerFolder.BackgroundColor = ThemeHelper.GetReadMailBGColor();
            dateStart.BackgroundColor = ThemeHelper.GetReadMailBGColor();
            dateEnd.BackgroundColor = ThemeHelper.GetReadMailBGColor();

            //entrySearchText.BackgroundColor = ThemeHelper.GetReadMailBGColor();
            entrySearchText.TextColor = ThemeHelper.GetThemeTextColor();
        }
    }
}
