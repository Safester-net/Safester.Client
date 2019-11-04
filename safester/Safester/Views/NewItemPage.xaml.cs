using System;
using System.Collections.Generic;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using Safester.Models;
using Safester.ViewModels;
using System.Linq;
using System.Collections.ObjectModel;
using Plugin.FilePicker.Abstractions;
using Plugin.FilePicker;
using Acr.UserDialogs;
using Newtonsoft.Json;
using Safester.Services;
using Plugin.Media;
using System.IO;
using Safester.Utils;
using Safester.Controls;

namespace Safester.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class NewItemPage : ContentPage
    {
        private NewItemViewModel viewModel { get; set; }
        private bool isDraft { get; set; }
        private bool isSending { get; set; }
        private bool isAppearing { get; set; }

        public DraftMessage DraftMessageData { get; set; }

        public NewItemPage(DraftMessage data = null)
        {
            InitializeComponent();

            DraftMessageData = data;
            viewModel = new NewItemViewModel();
            if (data != null)
            {
                viewModel.Subject = data.subject;
                if (data.attachments != null && data.attachments.Count > 0)
                {
                    viewModel.Attachments = new ObservableCollection<Attachment>();
                    foreach (var item in data.attachments)
                        viewModel.Attachments.Add(item);
                }
            }

            BindingContext = viewModel;

            viewModel.Finished = FinishedAction;

            listAttachment.ItemSelected += async (object sender, SelectedItemChangedEventArgs e) =>
            {
                if (e.SelectedItem == null)
                    return;

                bool result = await CustomAlertPage.Show(AppResources.Warning, AppResources.RemoveAttachment, AppResources.Yes, AppResources.No);
                if (result)
                    viewModel.Attachments.Remove(e.SelectedItem as Attachment);

                listAttachment.SelectedItem = null;
            };

            suggestBoxTo.DataSource = App.Recipients;
            suggestBoxCc.DataSource = App.Recipients;
            suggestBoxBcc.DataSource = App.Recipients;

            editorBody.Focused += (sender, e) =>
            {
                editorBody.HeightRequest = editorHeight / 2;
            };

            editorBody.Unfocused += (sender, e) =>
            {
                editorBody.HeightRequest = editorHeight;
            };
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (isAppearing == false)
            {
                isAppearing = true;

                UpdateDraftData();

                editorBody.Focus();
            }

            ChangeTheme();
        }

        double editorHeight = 0;

        protected override void LayoutChildren(double x, double y, double width, double height)
        {
            base.LayoutChildren(x, y, width, height);

            if (editorBody.Height.Equals(0) == false)
            {
                if (editorHeight.Equals(0))
                    editorHeight = editorBody.Height;

                //editorBody.HeightRequest = editorHeight;
            }
        }

        private void UpdateDraftData()
        {
            try
            {
                if (DraftMessageData != null)
                {
                    if (DraftMessageData.ToRecipients != null && DraftMessageData.ToRecipients.Count > 0)
                    {
                        suggestBoxTo.Text = string.Join(";", DraftMessageData.ToRecipients);
                    }
                    if (DraftMessageData.CcRecipients != null && DraftMessageData.CcRecipients.Count > 0)
                    {
                        suggestBoxCc.Text = string.Join(";", DraftMessageData.CcRecipients);
                    }
                    if (DraftMessageData.BccRecipients != null && DraftMessageData.BccRecipients.Count > 0)
                    {
                        suggestBoxBcc.Text = string.Join(";", DraftMessageData.BccRecipients);
                    }

                    if (string.IsNullOrEmpty(DraftMessageData.subject) == false)
                        entrySubject.Text = DraftMessageData.subject;

                    if (string.IsNullOrEmpty(DraftMessageData.body) == false)
                        editorBody.Text = DraftMessageData.body;
                }
                
                if (string.IsNullOrEmpty(editorBody.Text))
                {
                    var _settingsService = DependencyService.Get<SettingsService>();
                    var enableSignature = _settingsService.LoadSettings("enable_mobile_signature");
                    if (string.IsNullOrEmpty(enableSignature) == false && enableSignature.Equals("1"))
                    {
                        editorBody.Text = string.Format("\n\n{0}", _settingsService.LoadSettings("mobile_signature"));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        void Save_Clicked(object sender, EventArgs e)
        {
            if (DraftMessageData == null)
                DraftMessageData = new DraftMessage();

            if (DraftMessageData.Id == 0)
                DraftMessageData.Id = App.DraftMessages.Count + 1;

            SyncSuggestDataWithVM();

            DraftMessageData.ToRecipients = viewModel.ToRecipients;
            DraftMessageData.CcRecipients = viewModel.CcRecipients;
            DraftMessageData.BccRecipients = viewModel.BccRecipients;

            if (string.IsNullOrEmpty(entrySubject.Text) == false)
                DraftMessageData.subject = entrySubject.Text;
            if (string.IsNullOrEmpty(editorBody.Text) == false)
                DraftMessageData.body = editorBody.Text;

            if (viewModel.Attachments != null)
            {
                DraftMessageData.attachments = viewModel.Attachments;
            }

            Utils.Utils.AddOrUpdateDraft(DraftMessageData);
            Utils.Utils.SaveDataToFile(App.DraftMessages, Utils.Utils.KEY_FILE_DRAFTMESSAGES, true);

            CustomAlertPage.Show("", AppResources.SaveSuccess, AppResources.OK);
            ClosePage();
        }

        void Send_Clicked(object sender, EventArgs e)
        {
            if (isSending == true)
                return;

            SyncSuggestDataWithVM();

            if (viewModel.ToRecipients.Count == 0)
            {
                CustomAlertPage.Show(AppResources.Warning, AppResources.InputReceiverEmail, AppResources.OK);
                return;
            }

            if (viewModel.ToRecipients.Count > 0)
            {
                foreach (var item in viewModel.ToRecipients)
                {
                    if (Utils.Utils.IsValidEmail(item.recipientEmailAddr) == false)
                    {
                        CustomAlertPage.Show(AppResources.Warning, AppResources.InputReceiverEmail, AppResources.OK);
                        return;
                    }
                }
            }

            if (viewModel.CcRecipients.Count > 0)
            {
                foreach (var item in viewModel.CcRecipients)
                {
                    if (Utils.Utils.IsValidEmail(item.recipientEmailAddr) == false)
                    {
                        CustomAlertPage.Show(AppResources.Warning, AppResources.InputReceiverEmail, AppResources.OK);
                        return;
                    }
                }
            }

            if (viewModel.BccRecipients.Count > 0)
            {
                foreach (var item in viewModel.BccRecipients)
                {
                    if (Utils.Utils.IsValidEmail(item.recipientEmailAddr) == false)
                    {
                        CustomAlertPage.Show(AppResources.Warning, AppResources.InputReceiverEmail, AppResources.OK);
                        return;
                    }
                }
            }

            if (string.IsNullOrEmpty(viewModel.Subject))
            {
                CustomAlertPage.Show(AppResources.Warning, AppResources.InputSubject, AppResources.OK);
                return;
            }

            viewModel.Body = editorBody.Text;
            if (string.IsNullOrEmpty(viewModel.Body))
                viewModel.Body = string.Empty;

            isSending = true;
            ShowLoading(true);
            isDraft = false;
            viewModel.SendMessgeCommand.Execute(null);
        }

        async void AddFile_Clicked(object sender, System.EventArgs e)
        {
            string option = await DisplayActionSheet("", AppResources.Cancel, null, (Device.RuntimePlatform == Device.iOS) ? new string[] { AppResources.TakePhoto, AppResources.SelectPhoto, AppResources.PickFile } : new string[] { AppResources.PickFile });
            if (string.IsNullOrEmpty(option))
                return;

            Attachment attachment = new Attachment();
            attachment.filename = string.Empty;
            attachment.filepath = string.Empty;
            attachment.size = 0;
            attachment.fileData = null;

            if (option.Equals(AppResources.TakePhoto)) // Take Photo from Camera
            {
                await CrossMedia.Current.Initialize();

                if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
                {
                    return;
                }

                attachment.filename = String.Format("camera_{0}.jpg", System.DateTime.Now.ToString("yyyyMMddHHmmss"));
                var file = await CrossMedia.Current.TakePhotoAsync(new Plugin.Media.Abstractions.StoreCameraMediaOptions
                {
                    Directory = "Sample",
                    Name = attachment.filename,
                    PhotoSize = Plugin.Media.Abstractions.PhotoSize.MaxWidthHeight,
                    MaxWidthHeight = 1024,
                    CompressionQuality = 80,
                });

                if (file != null)
                    attachment.filepath = file.Path;
            }
            else if (option.Equals(AppResources.SelectPhoto))// Take Photo from Library
            { 
                await CrossMedia.Current.Initialize();

                if (!CrossMedia.Current.IsPickPhotoSupported)
                {
                    return;
                }

                var file = await CrossMedia.Current.PickPhotoAsync(new Plugin.Media.Abstractions.PickMediaOptions
                {
                    CompressionQuality = 80,
                    PhotoSize = Plugin.Media.Abstractions.PhotoSize.MaxWidthHeight,
                    MaxWidthHeight = 1024,
                });

                if (file != null)
                {
                    attachment.filepath = file.Path;
                    attachment.filename = attachment.filepath.Substring(attachment.filepath.LastIndexOf('/') + 1);
                }
            }
            else if (option.Equals(AppResources.PickFile))// Pick file from the device
            {
                try
                {
                    FileData filedata = new FileData();
                    var crossFileData = CrossFilePicker.Current;
                    filedata = await crossFileData.PickFile();

                    if (filedata == null || string.IsNullOrEmpty(filedata.FilePath))
                        return;

                    attachment.filepath = filedata.FilePath;
                    attachment.filename = filedata.FileName;
                    attachment.size = filedata.DataArray.Length;
                    attachment.fileData = filedata.DataArray;
                }
                catch (Exception ex)
                {
                    await CustomAlertPage.Show(AppResources.Warning, AppResources.FileAttachNameError, AppResources.OK);
                    return;
                }
            }

            if (string.IsNullOrEmpty(attachment.filename) || string.IsNullOrEmpty(attachment.filepath))
                return;

            if (viewModel.Attachments.Any(x => string.IsNullOrEmpty(x.filepath) && x.filepath.Equals(attachment.filepath)))
            {
                await CustomAlertPage.Show("", AppResources.FileAdded, AppResources.OK);
                return;
            }

            try
            {
                if (attachment.size == 0)
                {
                    FileInfo info = new FileInfo(attachment.filepath);
                    attachment.size = info.Length;
                }

                if (attachment.size > 200 * 1024 * 1024)
                {
                    await CustomAlertPage.Show(AppResources.Warning, AppResources.FileAttachSizeError, AppResources.OK);
                    return;
                }
                viewModel.Attachments.Add(attachment);
            }
            catch (Exception ex)
            {
                await CustomAlertPage.Show(AppResources.Warning, AppResources.FileAttachNameError, AppResources.OK);
            }
        }

        void ClearAll_Clicked(object sender, System.EventArgs e)
        {
            viewModel.Attachments = new ObservableCollection<Attachment>();
        }

        async void FinishedAction(bool success, string errorMsg)
        {
            isSending = false;
            ShowLoading(false);
            if (success)
            {
                if (isDraft == false)
                {
                    editorBody.Text = viewModel.BodyEncrypted;
                    await CustomAlertPage.Show(AppResources.Success, AppResources.MessageEncrypted, AppResources.OK);

                    if (DraftMessageData != null && DraftMessageData.Id > 0)
                    {
                        Device.BeginInvokeOnMainThread(async () =>
                        {
                            int idx = App.DraftMessages.IndexOf(DraftMessageData);
                            App.DraftMessages.RemoveAt(idx);
                            Utils.Utils.SaveDataToFile(App.DraftMessages, Utils.Utils.KEY_FILE_DRAFTMESSAGES, true);
                            ClosePage();
                        });
                    }
                    else
                    {
                        ClosePage();
                    }
                }
                else
                {
                    await CustomAlertPage.Show(AppResources.Success, AppResources.SaveSuccess, AppResources.OK);
                }
            }
            else
            {
                await CustomAlertPage.Show(AppResources.Warning, errorMsg, AppResources.OK);
            }
        }

        private void SyncSuggestDataWithVM()
        {
            string lastRecipientName = string.Empty;

            viewModel.ToRecipients = new ObservableCollection<Recipient>();
            if (suggestBoxTo.SelectedIndices != null && suggestBoxTo.SelectedIndices is List<int>)
            {
                try
                {
                    var indicies = suggestBoxTo.SelectedIndices as List<int>;
                    foreach (var item in indicies)
                    {
                        viewModel.ToRecipients.Add(App.Recipients[item]);
                    }                    
                }
                catch (Exception ex)
                {

                }
            }

            if (string.IsNullOrEmpty(suggestBoxTo.Text) == false)
            {
                string recipientName = string.Empty;
                string recipientEmail = string.Empty;

                string[] recipients = suggestBoxTo.Text.Split(';');
                if (recipients != null && recipients.Length > 0)
                {
                    foreach (var recipient in recipients)
                    {
                        Utils.Utils.ParseEmailString(recipient, out recipientName, out recipientEmail);

                        if (string.IsNullOrWhiteSpace(recipientEmail))
                            continue;

                        bool isExist = false;
                        if (viewModel.ToRecipients != null && viewModel.ToRecipients.Any(x => x.recipientEmailAddr.Equals(recipientEmail)))
                            isExist = true;

                        if (isExist  == false)
                            viewModel.ToRecipients.Add(new Recipient { recipientEmailAddr = recipientEmail, recipientName = recipientName });
                    }
                }                
            }

            viewModel.CcRecipients = new ObservableCollection<Recipient>();
            if (suggestBoxCc.SelectedIndices != null && suggestBoxCc.SelectedIndices is List<int>)
            {
                try
                {
                    var indicies = suggestBoxCc.SelectedIndices as List<int>;
                    foreach (var item in indicies)
                    {
                        viewModel.CcRecipients.Add(App.Recipients[item]);
                    }
                }
                catch (Exception ex)
                {

                }
            }

            if (string.IsNullOrEmpty(suggestBoxCc.Text) == false && suggestBoxCc.Text.Equals(lastRecipientName) == false)
            {
                string recipientName = string.Empty;
                string recipientEmail = string.Empty;

                string[] recipients = suggestBoxCc.Text.Split(';');
                if (recipients != null && recipients.Length > 0)
                {
                    foreach (var recipient in recipients)
                    {
                        Utils.Utils.ParseEmailString(recipient, out recipientName, out recipientEmail);
                        if (string.IsNullOrWhiteSpace(recipientEmail))
                            continue;

                        bool isExist = false;
                        if (viewModel.CcRecipients != null && viewModel.CcRecipients.Any(x => x.recipientEmailAddr.Equals(recipientEmail)))
                            isExist = true;

                        if (isExist == false)
                            viewModel.CcRecipients.Add(new Recipient { recipientEmailAddr = recipientEmail, recipientName = recipientName });
                    }
                }
            }

            viewModel.BccRecipients = new ObservableCollection<Recipient>();
            if (suggestBoxBcc.SelectedIndices != null && suggestBoxBcc.SelectedIndices is List<int>)
            {
                try
                {
                    var indicies = suggestBoxBcc.SelectedIndices as List<int>;
                    foreach (var item in indicies)
                    {
                        viewModel.BccRecipients.Add(App.Recipients[item]);
                    }
                }
                catch (Exception ex)
                {

                }
            }

            if (string.IsNullOrEmpty(suggestBoxBcc.Text) == false && suggestBoxBcc.Text.Equals(lastRecipientName) == false)
            {
                string recipientName = string.Empty;
                string recipientEmail = string.Empty;

                string[] recipients = suggestBoxBcc.Text.Split(';');
                if (recipients != null && recipients.Length > 0)
                {
                    foreach (var recipient in recipients)
                    {
                        Utils.Utils.ParseEmailString(recipient, out recipientName, out recipientEmail);
                        if (string.IsNullOrWhiteSpace(recipientEmail))
                            continue;

                        bool isExist = false;
                        if (viewModel.BccRecipients != null && viewModel.BccRecipients.Any(x => x.recipientEmailAddr.Equals(recipientEmail)))
                            isExist = true;

                        if (isExist == false)
                            viewModel.BccRecipients.Add(new Recipient { recipientEmailAddr = recipientEmail, recipientName = recipientName });
                    }
                }
            }
        }

        private void ClosePage()
        {
            /*if (Navigation.NavigationStack != null && Navigation.NavigationStack.Count > 1)
            {
                Navigation.PopAsync();
                return;
            }

            MainPage.MainMasterPage.Detail = new NavigationPage(new NewItemPage());*/
            Navigation.PopAsync();
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

            editorBody.TextColor = ThemeHelper.GetThemeTextColor();

            lblMailTo.TextColor = ThemeHelper.GetReadMailLabelColor();
            lblMailCc.TextColor = ThemeHelper.GetReadMailLabelColor();
            lblMailBcc.TextColor = ThemeHelper.GetReadMailLabelColor();
            lblSubject.TextColor = ThemeHelper.GetReadMailLabelColor();

            suggestBoxTo.TextColor = ThemeHelper.GetThemeTextColor();
            suggestBoxCc.TextColor = ThemeHelper.GetThemeTextColor();
            suggestBoxBcc.TextColor = ThemeHelper.GetThemeTextColor();
            entrySubject.TextColor = ThemeHelper.GetThemeTextColor();

            suggestBoxTo.DropDownBackgroundColor = ThemeHelper.GetReadMailBGColor();
            suggestBoxCc.DropDownBackgroundColor = ThemeHelper.GetReadMailBGColor();
            suggestBoxBcc.DropDownBackgroundColor = ThemeHelper.GetReadMailBGColor();

            suggestBoxTo.BorderColor = ThemeHelper.GetSearchEntryBorderColor();
            suggestBoxCc.BorderColor = ThemeHelper.GetSearchEntryBorderColor();
            suggestBoxBcc.BorderColor = ThemeHelper.GetSearchEntryBorderColor();

            entrySubject.BackgroundColor = ThemeHelper.GetReadMailBGColor();
        }
    }
}