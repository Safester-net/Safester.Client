using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using Safester.Models;
using Safester.Views;
using Safester.ViewModels;
using System.Collections;
using Safester.Services;
using Acr.UserDialogs;
using Safester.Utils;
using Safester.Controls;

namespace Safester.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ItemsPage : ContentPage
    {
        public MenuItemType ItemType { get; set; }
        public static bool NeedForceReload { get; set; }

        private ItemsViewModel viewModel;

        public ItemsPage(MenuItemType itemType, string title, bool isPageInboxForStarred = true)
        {
            InitializeComponent();

            BindingContext = viewModel = new ItemsViewModel();
            ItemType = itemType;
            viewModel.DirectoryId = ItemType;
            viewModel.Title = title;
            viewModel.IsStarredForInbox = isPageInboxForStarred;

            ItemsListView.ItemAppearing += InfiniteListView_ItemAppearing;

            viewModel.LoadingAction = (isShowing) =>
            {
                if (isShowing)
                    UserDialogs.Instance.Loading(AppResources.Pleasewait, null, null, true);
                else
                    UserDialogs.Instance.Loading().Hide();
            };

            viewModel.LoadingFinished = () =>
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    ItemsListView.IsRefreshing = false; 
                });
            };
        }

        async void OnItemSelected(object sender, SelectedItemChangedEventArgs args)
        {
            var item = args.SelectedItem as Message;
            if (item == null)
                return;

            viewModel.MarkMessageAsRead(item);

            var currentPageType = ItemType;
            if (ItemType == MenuItemType.Starred)
                currentPageType = (viewModel.IsStarredForInbox ? MenuItemType.Inbox : MenuItemType.Sent);

            await Navigation.PushAsync(new ItemDetailPage(new ItemDetailViewModel(item, currentPageType)));

            // Manually deselect item.
            ItemsListView.SelectedItem = null;
        }

        void InfiniteListView_ItemAppearing(object sender, ItemVisibilityEventArgs e)
        {
            try
            {
                var items = ItemsListView.ItemsSource as IList;

                var _settingsService = DependencyService.Get<SettingsService>();
                var messagesPerScroll = _settingsService.LoadSettings("messages_per_scroll");
                int limit = Utils.Utils.GetCountPerScroll(messagesPerScroll);

                if (items != null && e.Item == items[items.Count - 1] && items.Count >= limit)
                {
                    if (viewModel.LoadMoreCommand != null)
                        viewModel.LoadMoreCommand.Execute(null);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        async void ComposeItem_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new NewItemPage());
        }

        async void SearchItem_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new SearchPage(ItemType));
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            BackgroundColor = ThemeHelper.GetListPageBGColor();

            if (viewModel.Items.Count == 0 || NeedForceReload)
                viewModel.LoadItemsCommand.Execute(null);

            NeedForceReload = false;
        }

        async void OnStarClicked(object sender, System.EventArgs e)
        {
            if (ItemType != MenuItemType.Inbox && ItemType != MenuItemType.Sent && ItemType != MenuItemType.Starred)
                return;

            var mi = ((Button)sender);

            UserDialogs.Instance.Loading(AppResources.Pleasewait, null, null, true);
            var result = await viewModel.MarkStarCommand(mi.CommandParameter as Message);
            UserDialogs.Instance.Loading().Hide();

            if (result == false)
            {
                await CustomAlertPage.Show(AppResources.Warning, AppResources.TryAgain, AppResources.OK);
            }
        }

        async void OnUnread(object sender, System.EventArgs e)
        {
            if (ItemType != MenuItemType.Inbox)
                return;

            var mi = ((MenuItem)sender);

            UserDialogs.Instance.Loading(AppResources.Pleasewait, null, null, true);
            var result = await viewModel.MarkUnreadCommand(mi.CommandParameter as Message);
            UserDialogs.Instance.Loading().Hide();

            if (result == true)
            {
                viewModel.LoadItemsCommand.Execute(null);
            }
            else
            {
                await CustomAlertPage.Show(AppResources.Warning, AppResources.TryAgain, AppResources.OK);
            }
        }

        async void OnDelete(object sender, System.EventArgs e)
        {
            bool result = await CustomAlertPage.Show(ALERTTYPE.Picker, AppResources.Warning, AppResources.DeleteMail, AppResources.Yes, AppResources.Cancel, new string[] { AppResources.DeleteForEveryone, AppResources.DeleteForMe });
            if (result)
            {
                viewModel.deleteOption = CustomAlertPage.GetSelectedIndex();
                var mi = ((MenuItem)sender);

                UserDialogs.Instance.Loading(AppResources.Pleasewait, null, null, true);
                result = await viewModel.DeleteItemsCommand((int)(mi.CommandParameter as Message).messageId);
                UserDialogs.Instance.Loading().Hide();

                if (result == true)
                {
                    viewModel.LoadItemsCommand.Execute(null);
                }
                else
                {
                    await CustomAlertPage.Show(AppResources.Warning, AppResources.TryAgain, AppResources.OK);
                }
            }
        }
    }
}