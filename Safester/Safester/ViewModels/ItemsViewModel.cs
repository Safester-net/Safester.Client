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
        public bool IsStarredForInbox = true;

        public int deleteOption;

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

                int messageType = (int)DirectoryId;
                if (DirectoryId == MenuItemType.Starred)
                {
                    messageType = IsStarredForInbox ? (int)MenuItemType.Inbox : (int)MenuItemType.Sent;
                    ApiManager.SharedInstance().ListMessagesStarred(App.CurrentUser.UserEmail, App.CurrentUser.Token, messageType, limit, offset, (success, result) =>
                    {
                        ProcessResult(success, result);
                    });
                }
                else
                {
                    ApiManager.SharedInstance().ListMessages(App.CurrentUser.UserEmail, App.CurrentUser.Token, (int)DirectoryId, limit, offset, (success, result) =>
                    {
                        ProcessResult(success, result);
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        private void ProcessResult(bool success, MessagesResultInfo result)
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
        }

        async Task ExecuteLoadMoreItemsCommand()
        {
            if (IsBusy)
                return;

            IsBusy = true;
            LoadingAction?.Invoke(true);

            try
            {
                int messageType = (int)DirectoryId;
                if (DirectoryId == MenuItemType.Starred)
                {
                    messageType = IsStarredForInbox ? (int)MenuItemType.Inbox : (int)MenuItemType.Sent;
                    ApiManager.SharedInstance().ListMessagesStarred(App.CurrentUser.UserEmail, App.CurrentUser.Token, messageType, limit, offset, (success, result) =>
                    {
                        ProcessResult(success, result);
                    });
                }
                else
                {
                    ApiManager.SharedInstance().ListMessages(App.CurrentUser.UserEmail, App.CurrentUser.Token, (int)DirectoryId, limit, offset, (success, result) =>
                    {
                        ProcessResult(success, result);
                    });
                }
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
                result = await ApiManager.SharedInstance().DeleteMessage(App.CurrentUser.UserEmail, App.CurrentUser.Token, id, (int)DirectoryId, deleteOption);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            return result;
        }

        public async Task<bool> MarkUnreadCommand(Message message)
        {
            try
            {
                var result = await ApiManager.SharedInstance().SetMessageRead(App.CurrentUser.UserEmail, App.CurrentUser.Token,
                    message.senderEmailAddr, (int)message.messageId, message.IsRead);

                if (string.IsNullOrEmpty(result) == false && result.Equals("success", StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            return false;
        }

        public async Task<bool> MarkStarCommand(Message message)
        {
            try
            {
                var result = await ApiManager.SharedInstance().SetMessageStar(App.CurrentUser.UserEmail, App.CurrentUser.Token,
                    message.senderEmailAddr, (int)message.messageId, !message.IsStarred);

                if (string.IsNullOrEmpty(result) == false && result.Equals("success", StringComparison.OrdinalIgnoreCase))
                {
                    if (DirectoryId == MenuItemType.Starred)
                        Items.Remove(message);

                    message.IsStarred = !message.IsStarred;

                    return true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            return false;
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

            Items.Add(item);
        }
    }
}