using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Safester.CryptoLibrary.Api;
using Safester.Models;
using Safester.Network;
using Safester.Services;
using Safester.Utils;
using Xamarin.Essentials;
using Xamarin.Forms;
using XLabs.Forms.Behaviors;

namespace Safester.ViewModels
{
    public class ItemDetailViewModel : BaseViewModel
    {
        public MenuItemType ItemType { get; set; }
        public Message Item { get; set; }
        public Command LoadDataCommand { get; set; }

        public Command<int> LoadAttachmentCommand { get; set; }
        public Command<int> DeleteCommand { get; set; }

        public RelayGesture DumpParam { get; set; }

        public Action BodyUpdated { get; set; }
        public Action<Attachment, string> DownloadFinished { get; set; }
        public Action<bool> DeleteCompleted { get; set; }
        public Action ShowToast { get; set; }

        public String FromRecipient { get; set; }
        public String ToRecipients { get; set; }
        public String CcRecipients { get; set; }
        public String BccRecipients { get; set; }
        public String Subject { get; set; }
        public long MessageDate { get; set; }
        public String Body { get; set; }
        public String BodyOriginal { get; set; }
        public int deleteOption { get; set; }

        public bool IsBodyLoaded { get; set; }

        public ObservableCollection<Attachment> Attachments { get; set; }

        public ItemDetailViewModel(Message item, MenuItemType type)
        {
            Item = item;
            ItemType = type;

            FromRecipient = string.Format("{0} <{1}>", item.senderName, item.senderEmailAddr);
            ToRecipients = CcRecipients = BccRecipients = Subject = Body = BodyOriginal = string.Empty;
            foreach (var recipient in Item.recipients)
            {
                if (recipient.recipientType == 1) // to
                {
                    if (string.IsNullOrEmpty(ToRecipients))
                        ToRecipients = string.Format("{0} <{1}>", recipient.recipientName, recipient.recipientEmailAddr);
                    else
                        ToRecipients += string.Format("; {0} <{1}>", recipient.recipientName, recipient.recipientEmailAddr);
                }
                if (recipient.recipientType == 2) // cc
                {
                    if (string.IsNullOrEmpty(CcRecipients))
                        CcRecipients = string.Format("{0} <{1}>", recipient.recipientName, recipient.recipientEmailAddr);
                    else
                        CcRecipients += string.Format("; {0} <{1}>", recipient.recipientName, recipient.recipientEmailAddr);
                }
                if (recipient.recipientType == 3) // bcc
                {
                    if (string.IsNullOrEmpty(BccRecipients))
                        BccRecipients = string.Format("{0} <{1}>", recipient.recipientName, recipient.recipientEmailAddr);
                    else
                        BccRecipients += string.Format("; {0} <{1}>", recipient.recipientName, recipient.recipientEmailAddr);
                }
            }

            FromRecipient = HttpUtility.HtmlDecode(FromRecipient);
            ToRecipients = HttpUtility.HtmlDecode(ToRecipients);
            Subject = HttpUtility.HtmlDecode(item.subject);
            MessageDate = item.date;

            LoadDataCommand = new Command(async () => await ExecuteLoadDataCommand());
            LoadAttachmentCommand = new Command<int>(async (int pos) => await ExecuteLoadAttachmentCommand(pos));
            DeleteCommand = new Command<int>(async (id) => await DeleteItemsCommand(id));
            DumpParam = new RelayGesture(OnGestureAction);
        }

        async void OnGestureAction(GestureResult gr, object obj)
        {
            if (gr.GestureType == XLabs.Forms.Controls.GestureType.LongPress)
            {
                Console.WriteLine("LonGTaped");
                await Clipboard.SetTextAsync(Body);

                ShowToast?.Invoke();
            }
        }

        async Task ExecuteLoadDataCommand()
        {
            if (IsBusy)
                return;

            IsBusy = true;

            try
            {
                ApiManager.SharedInstance().GetMessageDetail(App.CurrentUser.UserEmail, App.CurrentUser.Token, (int)Item.messageId, (success, result) =>
                {
                    IsBusy = false;
                    if (success && result != null)
                    {
                        BodyOriginal = result.body;
                        Body = Utils.Utils.DecryptMessageData(App.KeyDecryptor, BodyOriginal, false);

                        Body = HttpUtility.HtmlDecode(Body);
                        Body = Utils.Utils.GetRemovedHtmlString(Body);
                        //Body = HtmlUtilities.ConvertToPlainText(Body);

                        IsBodyLoaded = true;
                        BodyUpdated?.Invoke();

                        if (Item.hasAttachs && result.attachments != null)
                        {
                            Attachments = new ObservableCollection<Attachment>(result.attachments);
                            foreach (var item in Attachments)
                            {
                                if (!string.IsNullOrEmpty(item.filename) && item.filename.EndsWith(".pgp", StringComparison.OrdinalIgnoreCase))
                                    item.filename = HttpUtility.HtmlDecode(item.filename.Substring(0, item.filename.Length - ".pgp".Length));
                            }
                            OnPropertyChanged("Attachments");
                        }

                        MarkMessageAsRead();
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        async Task ExecuteLoadAttachmentCommand(int pos)
        {
            try
            {
                ApiManager.SharedInstance().GetMessageAttachment(App.CurrentUser.UserEmail, App.CurrentUser.Token, (int)Item.messageId, pos, (success, stream) =>
                {
                    try
                    {
                        if (success && stream != null)
                        {
                            var item = Attachments.Where(x => x.attachPosition == pos).Select(x => x).FirstOrDefault();
                            if (item != null && string.IsNullOrWhiteSpace(item.filename) == false)
                            {
                                var filesService = DependencyService.Get<IFilesService>();
                                string path = filesService.GetDownloadFolder();

                                string decryptedFile = Path.Combine(path, item.filename);
                                if (Utils.Utils.DecryptFileData(App.KeyDecryptor, stream, decryptedFile) == true)
                                {
                                    DownloadFinished?.Invoke(item, decryptedFile);
                                    return;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }

                    DownloadFinished?.Invoke(null, "");
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                DownloadFinished?.Invoke(null, "");
            }
        }

        private void MarkMessageAsRead()
        {
            ApiManager.SharedInstance().SetMessageRead(App.CurrentUser.UserEmail, App.CurrentUser.Token, Item.senderEmailAddr, (int)Item.messageId, false);
        }

        public async Task<bool> DeleteItemsCommand(int id)
        {
            bool result = false;
            try
            {
                result = await ApiManager.SharedInstance().DeleteMessage(App.CurrentUser.UserEmail, App.CurrentUser.Token, id, (int)ItemType, deleteOption);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            return result;
        }
    }
}
