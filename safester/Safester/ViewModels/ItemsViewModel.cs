using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;

using Xamarin.Forms;

using Safester.Models;
using Safester.Views;
using Safester.Network;
using Safester.Services;
using System.Web;

namespace Safester.ViewModels
{
    public class ItemsViewModel : BaseViewModel
    {
        public ObservableCollection<Message> Items { get; set; }
        public Command LoadItemsCommand { get; set; }
        public Command LoadMoreCommand { get; set; }
        public Command<int> DeleteCommand { get; set; }

        public MenuItemType DirectoryId { get; set; }

        public Action<bool> LoadingAction { get; set; }
        public Action LoadingFinished { get; set; }

        private int limit { get; set; }
        private int offset { get; set; }

        public ItemsViewModel()
        {
            Items = new ObservableCollection<Message>();

            var _settingsService = DependencyService.Get<SettingsService>();
            var messagesPerScroll = _settingsService.LoadSettings("messages_per_scroll");
            limit = Utils.Utils.GetCountPerScroll(messagesPerScroll);

            LoadItemsCommand = new Command(async () => await ExecuteLoadItemsCommand());
            LoadMoreCommand = new Command(async () => await ExecuteLoadMoreItemsCommand());
            DeleteCommand = new Command<int>(async (id) => await DeleteItemsCommand(id));
        }

        async Task ExecuteLoadItemsCommand()
        {
            if (IsBusy)
                return;

            IsBusy = true;
            LoadingAction?.Invoke(true);
            try
            {
                Items.Clear();
                offset = 0;
                ApiManager.SharedInstance().ListMessages(App.CurrentUser.UserName, App.CurrentUser.Token, (int)DirectoryId, limit, offset, (success, result) =>
                {
                    IsBusy = false;
                    LoadingAction?.Invoke(false);

                    if (success && result != null)
                    {
                        foreach (var item in result.messages)
                        {
                            AddMessageToTheList(item);
                        }

                        LoadingFinished?.Invoke();
                        OnPropertyChanged("Items");
                        offset = Items.Count;
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        async Task ExecuteLoadMoreItemsCommand()
        {
            if (IsBusy)
                return;

            IsBusy = true;
            LoadingAction?.Invoke(true);

            try
            {
                ApiManager.SharedInstance().ListMessages(App.CurrentUser.UserName, App.CurrentUser.Token, (int)DirectoryId, limit, offset, (success, result) =>
                {
                    IsBusy = false;
                    LoadingAction?.Invoke(false);
                    if (success && result != null)
                    {
                        foreach (var item in result.messages)
                        {
                            AddMessageToTheList(item);
                        }

                        LoadingFinished?.Invoke();
                        OnPropertyChanged("Items");
                        offset = Items.Count;
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        public async Task<bool> DeleteItemsCommand(int id)
        {
            bool result = false;
            try
            {
                result = await ApiManager.SharedInstance().DeleteMessage(App.CurrentUser.UserName, App.CurrentUser.Token, id, (int)DirectoryId);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            return result;
        }

        public void MarkMessageAsRead(Message item)
        {
            if (Items == null)
                return;

            var index = Items.IndexOf(item);
            if (index == -1)
                return;

            Items[index].IsRead = true;
        }

        private void AddMessageToTheList(Message item)
        {
            item.subject = HttpUtility.HtmlDecode(Utils.Utils.DecryptMessageData(App.KeyDecryptor, item.subject, true));
            if (DirectoryId != MenuItemType.Inbox)
                item.IsRead = true;

            Items.Add(item);
        }
    }
}