using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using Org.BouncyCastle.Bcpg.OpenPgp;
using Safester.CryptoLibrary.Api;
using Safester.Models;
using Safester.Network;
using Safester.Utils;
using Xamarin.Forms;

namespace Safester.ViewModels
{
    public class NewItemViewModel : BaseViewModel
    {
        public string Subject { get; set; }
        public string Body { get; set; }
        public ObservableCollection<Recipient> ToRecipients { get; set; }
        public ObservableCollection<Recipient> CcRecipients { get; set; }
        public ObservableCollection<Recipient> BccRecipients { get; set; }
        public ObservableCollection<Attachment> Attachments { get; set; }

        public Command SendMessgeCommand { get; set; }
        public Command SaveDraftCommand { get; set; }

        public Action<bool, string> Finished { get; set; }
        private List<Recipient> MailRecipients { get; set; }
        private List<PgpPublicKey> MailKeys { get; set; }

        public string BodyEncrypted { get; set; }

        public NewItemViewModel()
        {
            SendMessgeCommand = new Command(async () => await ExecuteSendMessgeCommand());
            SaveDraftCommand = new Command(async () => await ExecuteSaveDraftCommand());

            ToRecipients = new ObservableCollection<Recipient>();
            CcRecipients = new ObservableCollection<Recipient>();
            BccRecipients = new ObservableCollection<Recipient>();
            Attachments = new ObservableCollection<Attachment>();
        }

        async Task ExecuteSendMessgeCommand()
        {
            try
            {
                MailKeys = new List<PgpPublicKey>();
                var senderKeyInfo = await ApiManager.SharedInstance().GetPublicKey(App.CurrentUser.UserEmail, App.CurrentUser.Token, App.CurrentUser.UserEmail);
                if (senderKeyInfo == null)
                {
                    Finished?.Invoke(false, AppResources.ErrorGetUserKey);
                    return;
                }

                // Add sender pgp public key
                MailKeys.Add(PgpPublicKeyGetter.ReadPublicKey(senderKeyInfo.publicKey));

                int recpos = 0;
                MailRecipients = new List<Recipient>();

                if (ToRecipients != null)
                {
                    foreach (var item in ToRecipients)
                    {
                        if (string.IsNullOrWhiteSpace(item.recipientEmailAddr) == false)
                            await GetKeyAndAddRecipient(item, 1, recpos++);
                    }
                }

                if (CcRecipients != null)
                {
                    foreach (var item in CcRecipients)
                    {
                        if (string.IsNullOrWhiteSpace(item.recipientEmailAddr) == false)
                            await GetKeyAndAddRecipient(item, 2, recpos++);
                    }
                }

                if (BccRecipients != null)
                {
                    foreach (var item in BccRecipients)
                    {
                        if (string.IsNullOrWhiteSpace(item.recipientEmailAddr) == false)
                            await GetKeyAndAddRecipient(item, 3, recpos++);
                    }
                }

                if (string.IsNullOrEmpty(Body))
                    Body = string.Empty;
                else
                    Body = Body.Replace("\n", "<br>");

                var jsonData = new SenderMailMessage
                {
                    senderEmailAddr = App.CurrentUser.UserEmail,
                    subject = HttpUtility.HtmlEncode(Subject),//App.KeyEncryptor.Encrypt(MailKeys, HttpUtility.HtmlEncode(Subject)),
                    body = App.KeyEncryptor.Encrypt(MailKeys, HttpUtility.HtmlEncode(Body)),
                };

                jsonData.attachments = new List<Attachment>();
                var fileList = new List<KeyValuePair<string, Stream>>();

                if (Attachments != null)
                {
                    int pos = 1;
                    foreach (var attachment in Attachments)
                    {
                        string encfilepath = string.Empty;
                        string originalpath = attachment.filepath;
                        if (attachment.filepath.StartsWith("content://", StringComparison.OrdinalIgnoreCase) && attachment.fileData != null)
                        {
                            if (Utils.Utils.SaveTempFile(attachment.filename, attachment.fileData, out originalpath) == false)
                            {
                                Finished?.Invoke(false, AppResources.ErrorAddAttachment + attachment.filename);
                                return;
                            }
                        }

                        if (Utils.Utils.EncryptFile(App.KeyEncryptor, MailKeys, originalpath, attachment.filename, "fileenc", out encfilepath) == false)
                        {
                            Finished?.Invoke(false, AppResources.ErrorAddAttachment + attachment.filename);
                            return;
                        }

                        var streamContent = File.Open(encfilepath, FileMode.Open, FileAccess.Read);
                        FileInfo info = new FileInfo(encfilepath);
                        jsonData.attachments.Add(new Attachment
                        {
                            attachPosition = pos++,
                            filename = attachment.filename + ".pgp",
                            size = info.Length,
                        });

                        fileList.Add(new KeyValuePair<string, Stream>(attachment.filename + ".pgp", streamContent));
                    }
                }

                jsonData.recipients = MailRecipients;

                jsonData.size = 0;

                var jsonStringData = JsonConvert.SerializeObject(jsonData);
                ApiManager.SharedInstance().SendMessage(App.CurrentUser.UserEmail, App.CurrentUser.Token, jsonStringData, fileList, (success, result) =>
                {
                    try
                    {
                        if (fileList != null)
                        {
                            foreach (var file in fileList)
                            {
                                file.Value.Close();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }

                    Device.BeginInvokeOnMainThread(() =>
                    {
                        try
                        {
                            if (success)
                            {
                                foreach (var newRecipient in MailRecipients)
                                    Utils.Utils.AddOrUpdateRecipient(newRecipient);
                                Utils.Utils.SaveDataToFile(App.Recipients, Utils.Utils.KEY_FILE_RECIPIENTS);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                        }
                        finally
                        {
                            BodyEncrypted = Utils.Utils.EncryptMessageData(App.KeyEncryptor, MailKeys, Body);
                            Finished?.Invoke(success, success ? string.Empty : result);
                        }
                    });
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                Finished?.Invoke(false, AppResources.ErrorUnknownException + ex.ToString());
            }
        }

        async Task ExecuteSaveDraftCommand()
        {
            Finished?.Invoke(true, string.Empty);
        }

        private async Task GetKeyAndAddRecipient(Recipient item, int type, int pos)
        {
            //string recipientName = string.Empty;
            //string recipientEmail = string.Empty;

            //Utils.Utils.ParseEmailString(item, out recipientName, out recipientEmail);
            if (MailRecipients.Any(x => x.recipientEmailAddr.ToLower().Equals(item.recipientEmailAddr.ToLower())))
            {
                return;
            }

            var keyInfo = await ApiManager.SharedInstance().GetPublicKey(App.CurrentUser.UserEmail, App.CurrentUser.Token, item.recipientEmailAddr.ToLower());
            if (keyInfo == null)
            {
                keyInfo = await ApiManager.SharedInstance().GetPublicKey(App.CurrentUser.UserEmail, App.CurrentUser.Token, "contact@safelogic.com");
                if (keyInfo == null)
                    return;
            }

            var newRecipient = new Recipient
            {
                recipientEmailAddr = item.recipientEmailAddr.ToLower(),
                recipientName = item.recipientName,
                recipientPosition = pos,
                recipientType = type
            };

            MailRecipients.Add(newRecipient);

            // Add recipient pgp public key
            MailKeys.Add(PgpPublicKeyGetter.ReadPublicKey(keyInfo.publicKey));
        }
    }
}
