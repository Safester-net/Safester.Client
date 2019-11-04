using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Org.BouncyCastle.Bcpg.OpenPgp;
using Plugin.Messaging;
using Safester.CryptoLibrary.Api;
using Safester.CryptoLibrary.Src.Api.Util;
using Safester.Models;
using Safester.Services;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Safester.Utils
{
    public static class Utils
    {
        public const string KEY_FILE_RECIPIENTS = "recipients";
        public const string KEY_FILE_USERSETTINGS = "settings";
        public const string KEY_FILE_DRAFTMESSAGES = "drafts";
        public const string KEY_FILE_USERS = "users";

        public const bool DO_ENCRYPT_SUBJECT = true;

        public static Dictionary<string, int> PricingList = new Dictionary<string, int>()
        {
            {"FREE", 1024 * 5}, {"SILVER", 1024 * 20}, {"GOLD", 1024 * 50}, {"PLATINUM", 1024 * 250}
        };

        public const int MESSAGE_FETCH_COUNT = 25;

        public static byte[] Combine(params byte[][] arrays)
        {
            byte[] rv = new byte[arrays.Sum(a => a.Length)];
            int offset = 0;
            foreach (byte[] array in arrays)
            {
                System.Buffer.BlockCopy(array, 0, rv, offset, array.Length);
                offset += array.Length;
            }
            return rv;
        }

        public static string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);

            return hex.ToString();
        }

        public static void ParseEmailString(string str, out string name, out string email)
        {
            name = email = string.Empty;
            if (string.IsNullOrWhiteSpace(str))
                return;

            int idx = str.IndexOf("<", StringComparison.OrdinalIgnoreCase);
            if (idx != -1)
            {
                name = str.Substring(0, idx).Trim();
                int idx1 = str.IndexOf(">", StringComparison.OrdinalIgnoreCase);
                if (idx1 != -1)
                    email = str.Substring(idx + 1, idx1 - idx - 1).Trim();
                else
                    email = str.Substring(idx + 1, str.Length - idx - 1).Trim();
            }
            else
            {
                name = string.Empty;
                email = str.Trim();
            }
        }

        public static bool SaveFile(string filename, byte[] data, out string savedPath)
        {
            savedPath = string.Empty;

            try
            {
                var filesService = DependencyService.Get<IFilesService>();

                string path = filesService.GetDownloadFolder();

                string filepath = Path.Combine(path, filename);

                if (File.Exists(filepath))
                    File.Delete(filepath);

                File.WriteAllBytes(filepath, data);

                savedPath = path;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }

            return true;
        }

        public static bool SaveTempFile(string filename, byte[] data, out string savedPath)
        {
            savedPath = string.Empty;

            try
            {
                string path = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                Directory.CreateDirectory(Path.Combine(path, "safetemp"));
                string filepath = Path.Combine(path, filename);

                if (File.Exists(filepath))
                    File.Delete(filepath);

                File.WriteAllBytes(filepath, data);

                savedPath = filepath;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }

            return true;
        }

        public static bool EncryptFile(Encryptor encryptor, List<PgpPublicKey> keys, string originalfile, string filename, string subfolder, out string encfilepath)
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            Directory.CreateDirectory(Path.Combine(path, subfolder));
            string filepath = Path.Combine(Path.Combine(path, subfolder), filename + ".pgp");

            encfilepath = string.Empty;

            try
            {
                Stream inputstream = File.OpenRead(originalfile);
                Stream outputstream = File.OpenWrite(filepath);

                encryptor.Encrypt(keys, inputstream, outputstream);
                encfilepath = filepath;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }

            return true;
        }

        public static string EncryptMessageData(Encryptor decryptor, List<PgpPublicKey> keys, string data)
        {
            string result = string.Empty;

            try
            {
                result = decryptor.Encrypt(keys, data);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return result;
        }

        public static string DecryptMessageData(Decryptor decryptor, string data, bool isSubject)
        {
            string result = string.Empty;

            try
            {
                if (isSubject)
                {
                    if (DO_ENCRYPT_SUBJECT && data.Contains("-BEGIN PGP MESSAGE-") && data.Contains("Version: BCPG"))
                        result = decryptor.Decrypt(data);
                    else
                        result = data;
                }
                else
                {
                    result = decryptor.Decrypt(data);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return result;
        }

        public static bool DecryptFileData(Decryptor decryptor, string encryptedFile, string decryptedFile)
        {
            try
            {
                Stream inputStream = File.OpenRead(encryptedFile);
                Stream outputStream = File.OpenWrite(decryptedFile);

                decryptor.Decrypt(inputStream, outputStream);
                return decryptor.Verify;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return false;
        }

        public static bool DecryptFileData(Decryptor decryptor, Stream stream, string decryptedFile)
        {
            try
            {
                Stream outputStream = File.OpenWrite(decryptedFile);

                decryptor.Decrypt(stream, outputStream);
                return decryptor.Verify;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return false;
        }

        // User Profile
        public static void LoadUserProfiles()
        {
            if (App.CurrentUser == null)
                return;

            App.UserSettings = LoadDataFromFile<SettingsInfo>(KEY_FILE_USERSETTINGS, true);
            if (App.UserSettings == null)
                App.UserSettings = new SettingsInfo();

            App.DraftMessages = LoadDataFromFile<ObservableCollection<DraftMessage>>(KEY_FILE_DRAFTMESSAGES, true);
            if (App.DraftMessages == null)
                App.DraftMessages = new ObservableCollection<DraftMessage>();
        }

        // User
        public static void AddOrUpdateUser(User user)
        {
            if (App.LocalUsers == null || user == null)
                return;

            if (App.LocalUsers.Any(x => x.UserEmail.Equals(user.UserEmail)))
            {
                var item = App.LocalUsers.First(x => x.UserEmail.Equals(user.UserEmail));
                if (item != null && !string.IsNullOrEmpty(user.UserEmail))
                {
                    item.UserEmail = user.UserEmail;
                    item.UserName = user.UserName;
                    item.UserPassword = user.UserPassword;
                    item.PassPhrase = user.PassPhrase;
                }
            }
            else
            {
                App.LocalUsers.Add(user.Clone());
            }

            if (App.ConnectedUsers == null)
                App.ConnectedUsers = new ObservableCollection<User>();

            if (App.ConnectedUsers.Any(x => x.UserEmail.Equals(user.UserEmail)))
            {
                var item = App.ConnectedUsers.First(x => x.UserEmail.Equals(user.UserEmail));
                if (item != null && !string.IsNullOrEmpty(user.UserEmail))
                {
                    item.UserEmail = user.UserEmail;
                    item.UserName = user.UserName;
                    item.UserPassword = user.UserPassword;
                    item.PassPhrase = user.PassPhrase;
                }
            }
            else
            {
                App.ConnectedUsers.Add(user.Clone());
            }
        }

        // Contacts
        public static void AddOrUpdateRecipient(Recipient recipient)
        {
            if (App.Recipients == null || recipient == null)
                return;

            if (App.Recipients.Any(x => x.recipientEmailAddr.Equals(recipient.recipientEmailAddr)))
            {
                var item = App.Recipients.First(x => x.recipientEmailAddr.Equals(recipient.recipientEmailAddr));
                if (item != null && !string.IsNullOrEmpty(recipient.recipientName))
                    item.recipientName = recipient.recipientName;
            }
            else
            {
                App.Recipients.Add(recipient);
            }
        }

        //DraftMessages
        public static void AddOrUpdateDraft(DraftMessage message)
        {
            if (App.DraftMessages == null || message == null)
                return;

            if (App.DraftMessages.Any(x => x.Id == message.Id))
            {
                var item = App.DraftMessages.First(x => x.Id == message.Id);
                if (item != null)
                    item = message;
            }
            else
            {
                App.DraftMessages.Add(message);
            }
        }

        // Save/Load Data
        public static T LoadDataFromFile<T>(string key, bool isUserData = false)
        {
            //
            if (isUserData)
                key = App.CurrentUser.UserEmail + "_" + key;

            var dataStr = DependencyService.Get<SettingsService>().LoadSettings(key);
            T data;

            try
            {
                data = JsonConvert.DeserializeObject<T>(dataStr);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                data = default(T);
            }

            return data;
        }

        public static void SaveDataToFile<T>(T data, string key, bool isUserData = false)
        {
            if (isUserData)
                key = App.CurrentUser.UserEmail + "_" + key;

            string dataStr = string.Empty;
            dataStr = JsonConvert.SerializeObject(data);

            DependencyService.Get<SettingsService>().SaveSettings(key, dataStr);
        }

        public static string GetProductName(int product)
        {
            if (product < 0 || product > PricingList.Count)
                return string.Empty;

            var item = PricingList.ElementAt(product);
            return item.Key;
        }

        public static int GetCountPerScroll(string settingsCount)
        {
            int count = 0;

            if (string.IsNullOrEmpty(settingsCount) == false)
            {
                int.TryParse(settingsCount, out count);
            }

            if (count == 0)
                count = MESSAGE_FETCH_COUNT;

            return count;
        }

        public static string GetSizeString(long bytes)
        {
            try
            {
                double size = bytes;
                if (size < 1024)
                    return "1KB";

                size /= 1024;
                if (size < 1024) // 1MB
                    return (int)size + "KB";

                size /= 1024;
                if (size < 1024) // 1GB
                    return string.Format("{0:f2}", size) + AppResources.MB;

                size /= 1024;
                if (size < 1024) // 1TB
                    return string.Format("{0:f2}", size) + AppResources.GB;

                size /= 1024;
                return string.Format("{0:f2}TB", size);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Size Converter Exception - {0}", ex);
            }

            return "0B";
        }

        public static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        public static long GetUnixEpochTimeStamp(DateTime date)
        {
            return (long)(date.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds);
        }

        public static string GetRemovedHtmlString(string str)
        {
            if (string.IsNullOrEmpty(str))
                return string.Empty;

            int idx = str.IndexOf("<style", StringComparison.OrdinalIgnoreCase);
            if (idx != -1)
            {
                int endidx = str.IndexOf("</style>", idx, StringComparison.OrdinalIgnoreCase);
                if (endidx != -1)
                    str = str.Substring(endidx + "</style>".Length);
            }

            // Remove linespace
            while (string.IsNullOrEmpty(str) == false && (str[0] == '\n' || str[0] == ' '))
                str = str.Substring(1);

            str = ParseLineInDiv(str);

            return str;
        }

        private static string ParseLineInDiv(string str)
        {
            StringBuilder result = new StringBuilder();
            bool isInDiv = false;

            if (string.IsNullOrEmpty(str) == false)
            {
                string[] lines = str.Split('\n');
                if (lines != null)
                {
                    foreach (var line in lines)
                    {
                        if (string.IsNullOrWhiteSpace(line) && isInDiv)
                            result.Append("<br>");
                        else
                        {
                            if (line.Contains("<div"))
                            {
                                isInDiv = true;
                                result.Append(line);
                            }
                            else if (line.Contains("</div"))
                            {
                                isInDiv = false;
                                result.Append(line);
                            }
                            else
                                result.Append(line.Replace("\n", "<br>"));
                        }
                    }
                }
            }

            return result.ToString();
        }

        public static bool SendEmail(string emailAddr)
        {
            try{
                var emailMessenger = CrossMessaging.Current.EmailMessenger;
                if (emailMessenger.CanSendEmail)
                {
                    emailMessenger.SendEmail(emailAddr, "Safester", "");
                    return true;
                }
            }
            catch (Exception ex)
            {
                
            }

            return false;
        }

        public static bool CallPhoneNum(string phoneNum)
        {
            try{
                var phoneDialer = CrossMessaging.Current.PhoneDialer;
                if (phoneDialer.CanMakePhoneCall)
                {
                    phoneDialer.MakePhoneCall(phoneNum);
                    return true;
                }
            }
            catch (Exception ex)
            {
                
            }

            return false;
        }

        public static async Task GetPhoneContacts()
        {
            try
            {
                var contacts = await Plugin.ContactService.CrossContactService.Current.GetContactListAsync();

                if (contacts != null && contacts.Count > 0)
                {
                    foreach (var contact in contacts)
                    {
                        var email = contact.Email;
                        var name = contact.Name;

                        if (string.IsNullOrEmpty(email) == false)
                        {
                            if (string.IsNullOrEmpty(name))
                                name = email;

                            name = name.Replace("\r\n", " ");
                            AddOrUpdateRecipient(new Recipient
                            {
                                recipientName = name,
                                recipientEmailAddr = email,
                                recipientPosition = 0,
                                recipientType = 0,
                            });
                        }
                    }
                }

                SaveDataToFile(App.Recipients, KEY_FILE_RECIPIENTS);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public static String GetEncryptedPassphrase(string passPhrase)
        {
            List<string> list = new List<string>();
            list.Add(DeviceInfo.Model);
            list.Add(DeviceInfo.Manufacturer);
            list.Add(DeviceInfo.Name);
            list.Add(DeviceInfo.VersionString);
            list.Add(DeviceInfo.Platform.ToString());
            list.Add(DeviceInfo.Idiom.ToString());
            list.Add(DeviceInfo.DeviceType.ToString());

            String keyHexa = ShaUtil.Compute(list);
            return Crypto.Encrypt(passPhrase, keyHexa);
        }

        public static String GetDecryptedPassphrase(string passPhrase)
        {
            try
            {
                List<string> list = new List<string>();
                list.Add(DeviceInfo.Model);
                list.Add(DeviceInfo.Manufacturer);
                list.Add(DeviceInfo.Name);
                list.Add(DeviceInfo.VersionString);
                list.Add(DeviceInfo.Platform.ToString());
                list.Add(DeviceInfo.Idiom.ToString());
                list.Add(DeviceInfo.DeviceType.ToString());

                String keyHexa = ShaUtil.Compute(list);
                return Crypto.Decrypt(passPhrase, keyHexa);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return string.Empty;
        }

        public static string GetFileImageName(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return "icon_file.png";

            if (fileName.EndsWith(".pgp", StringComparison.OrdinalIgnoreCase))
                fileName = fileName.Substring(0, fileName.Length - 4);

            if (fileName.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
                return "icon_txt.png";
            if (fileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                return "icon_pdf.png";
            if (fileName.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase))
                return "icon_jpeg.png";
            if (fileName.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase))
                return "icon_jpeg.png";
            if (fileName.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                return "icon_jpeg.png";
            if (fileName.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase))
                return "icon_jpeg.png";

            return "icon_file.png";
        }
    }
}
