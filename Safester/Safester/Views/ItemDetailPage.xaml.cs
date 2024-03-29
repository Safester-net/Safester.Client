﻿using System;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using Safester.Models;
using Safester.ViewModels;
using System.IO;
using Safester.Services;
using Acr.UserDialogs;
using System.Linq;
using Xamarin.Essentials;
using Safester.Utils;
using Safester.Controls;

namespace Safester.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ItemDetailPage : ContentPage
    {
        ItemDetailViewModel viewModel { get; set; }
        bool isNeedOpenFile { get; set; }

        public ItemDetailPage(ItemDetailViewModel viewModel)
        {
            InitializeComponent();

            BindingContext = this.viewModel = viewModel;
            Initialize();
        }

        public ItemDetailPage()
        {
            InitializeComponent();

            viewModel = new ItemDetailViewModel(new Message(), MenuItemType.Inbox);
            BindingContext = viewModel;

            Initialize();
        }

        private void Initialize()
        {
            listAttachment.BindingContext = viewModel;

            viewModel.BodyUpdated = BodyUpdatedAction;
            viewModel.DownloadFinished = DownloadFinishedAction;

            listAttachment.ItemSelected += ListAttachment_ItemSelected;
            switchShowOriginal.Toggled += SwitchShowOriginal_Toggled;
            htmlLabel.LongClicked = (copyFlag) =>
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    try
                    { 
                        await Clipboard.SetTextAsync(htmlLabel.PlainText);

                        CustomAlertPage.Show("", AppResources.ClipboardSuccess, AppResources.OK);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                });
            };

            htmlLabel.TextColor = ThemeHelper.GetThemeTextColor();

            lblMailFrom.TextColor = ThemeHelper.GetReadMailLabelColor();
            lblMailFromContent.TextColor = ThemeHelper.GetThemeTextColor();

            lblMailTo.TextColor = ThemeHelper.GetReadMailLabelColor();
            lblMailToContent.TextColor = ThemeHelper.GetThemeTextColor();

            lblMailCc.TextColor = ThemeHelper.GetReadMailLabelColor();
            lblMailCcContent.TextColor = ThemeHelper.GetThemeTextColor();

            lblMailBcc.TextColor = ThemeHelper.GetReadMailLabelColor();
            lblMailBccContent.TextColor = ThemeHelper.GetThemeTextColor();

            lblSubject.TextColor = ThemeHelper.GetReadMailLabelColor();
            lblSubjectContent.TextColor = ThemeHelper.GetThemeTextColor();

            lblDate.TextColor = ThemeHelper.GetReadMailLabelColor();
            lblDateContent.TextColor = ThemeHelper.GetThemeTextColor();

            lblAsStored.TextColor = ThemeHelper.GetReadMailLabelColor();
        }

        void SwitchShowOriginal_Toggled(object sender, ToggledEventArgs e)
        {
            if (e.Value)
            {
                htmlLabel.Text = viewModel.BodyOriginal;
            }
            else
            {
                htmlLabel.Text = viewModel.Body;
            }
        }

        private void BodyUpdatedAction()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                htmlLabel.Text = viewModel.Body;
            });
        }

        private void DownloadFinishedAction(Attachment item, string filePath)
        {
            ShowLoading(false);
            Device.BeginInvokeOnMainThread(async () =>
            {
                if (item == null || string.IsNullOrWhiteSpace(filePath))
                {
                    await CustomAlertPage.Show(AppResources.Warning, AppResources.FileDownloadFailure, AppResources.OK);
                    return;
                }

                string alertMsg = string.Empty; 
                if (Device.RuntimePlatform == Device.iOS)
                    alertMsg = AppResources.FileDownloadToFolderiOS.Replace("\\n", "\n");
                else
                    alertMsg = AppResources.FileDownloadToFolderAndroid.Replace("\\n", "\n");

                if (isNeedOpenFile == false)
                    await CustomAlertPage.Show(AppResources.Success, alertMsg, AppResources.OK);
                else
                {
                    var filesService = DependencyService.Get<IFilesService>();
                    filesService.OpenUri(filePath);
                }
            });
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            BackgroundColor = ThemeHelper.GetReadMailBGColor();
            switchShowOriginal.ColorChangedEvent?.Invoke();

            if (viewModel.Item.hasAttachs == false)
            {
                layoutAttachment.IsVisible = false;
                layoutAttachment.HeightRequest = 0;
            }

            viewModel.LoadDataCommand.Execute(null);
        }

        async void ListAttachment_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem == null)
                return;

            var itemData = e.SelectedItem as Attachment;
            if (itemData == null)
                return;

            listAttachment.SelectedItem = null;
            string option = await DisplayActionSheet("", AppResources.Cancel, null, new string[] { AppResources.OpenFile, AppResources.DownloadFile } );
            if (string.IsNullOrEmpty(option))
                return;

            if (option.Equals(AppResources.DownloadFile, StringComparison.OrdinalIgnoreCase)) // Download Only
            {
                isNeedOpenFile = false;
                ShowLoading(true);
                viewModel.LoadAttachmentCommand.Execute(itemData.attachPosition);
            }
            else if (option.Equals(AppResources.OpenFile, StringComparison.OrdinalIgnoreCase)) // Download and open
            {
                isNeedOpenFile = true;
                ShowLoading(true);
                viewModel.LoadAttachmentCommand.Execute(itemData.attachPosition);
            }
        }

        private async void DeleteItem_Clicked(object sender, EventArgs e)
        {
            bool result = await CustomAlertPage.Show(ALERTTYPE.Picker, AppResources.Warning, AppResources.DeleteMail, AppResources.Yes, AppResources.Cancel, new string[] { AppResources.DeleteForEveryone, AppResources.DeleteForMe });
            if (result)
            {
                viewModel.deleteOption = CustomAlertPage.GetSelectedIndex();

                ShowLoading(true);
                result = await viewModel.DeleteItemsCommand((int)viewModel.Item.messageId);
                ShowLoading(false);

                if (result == true)
                {
                    ItemsPage.NeedForceReload = true;
                    await Navigation.PopAsync();
                }
                else
                {
                    await CustomAlertPage.Show(AppResources.Warning, AppResources.TryAgain, AppResources.OK);
                }
            }
        }

        private void ReplyItem_Clicked(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(viewModel.FromRecipient))
                    return;

                var recipient = ProcessReplyRecipient(viewModel.FromRecipient);
                if (recipient == null)
                    return;

                if (viewModel.IsBodyLoaded == false)
                    return;

                var draftMessage = new DraftMessage { Id = 0 };
                draftMessage.ToRecipients = new System.Collections.ObjectModel.ObservableCollection<Recipient>();
                draftMessage.ToRecipients.Add(recipient);
                draftMessage.CcRecipients = new System.Collections.ObjectModel.ObservableCollection<Recipient>();
                draftMessage.BccRecipients = new System.Collections.ObjectModel.ObservableCollection<Recipient>();
                draftMessage.attachments = null;
                draftMessage.subject = "Re:" + viewModel.Subject;
                draftMessage.body = "\n\n----- original message --------\n" + htmlLabel.PlainText;
                if (string.IsNullOrEmpty(draftMessage.body) == false)
                    draftMessage.body = draftMessage.body.Replace("<br>", "\n");

                Navigation.PushAsync(new NewItemPage(draftMessage));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private void ReplyAllItem_Clicked(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(viewModel.FromRecipient))
                    return;

                if (viewModel.IsBodyLoaded == false)
                    return;

                var draftMessage = new DraftMessage { Id = 0 };
                draftMessage.ToRecipients = new System.Collections.ObjectModel.ObservableCollection<Recipient>();
                draftMessage.CcRecipients = new System.Collections.ObjectModel.ObservableCollection<Recipient>();
                draftMessage.BccRecipients = new System.Collections.ObjectModel.ObservableCollection<Recipient>();

                var recipient = ProcessReplyRecipient(viewModel.FromRecipient);
                if (recipient == null)
                    return;

                draftMessage.ToRecipients.Add(recipient);

                if (string.IsNullOrEmpty(viewModel.ToRecipients) == false)
                {
                    string[] recipients = viewModel.ToRecipients.Split(';');
                    if (recipients != null)
                    {
                        foreach (var item in recipients)
                        {
                            if (string.IsNullOrWhiteSpace(item) == false)
                            {
                                recipient = ProcessReplyRecipient(item);
                                if (recipient != null && !recipient.recipientEmailAddr.Equals(App.CurrentUser.UserEmail, StringComparison.OrdinalIgnoreCase))
                                    draftMessage.ToRecipients.Add(recipient);
                            }
                        }
                    }
                }

                if (string.IsNullOrEmpty(viewModel.CcRecipients) == false)
                {
                    string[] recipients = viewModel.CcRecipients.Split(';');
                    if (recipients != null)
                    {
                        foreach (var item in recipients)
                        {
                            if (string.IsNullOrWhiteSpace(item) == false)
                            {
                                recipient = ProcessReplyRecipient(item);
                                if (recipient != null && !recipient.recipientEmailAddr.Equals(App.CurrentUser.UserEmail, StringComparison.OrdinalIgnoreCase))
                                    draftMessage.CcRecipients.Add(recipient);
                            }
                        }
                    }
                }

                if (string.IsNullOrEmpty(viewModel.BccRecipients) == false)
                {
                    string[] recipients = viewModel.BccRecipients.Split(';');
                    if (recipients != null)
                    {
                        foreach (var item in recipients)
                        {
                            if (string.IsNullOrWhiteSpace(item) == false)
                            {
                                recipient = ProcessReplyRecipient(item);
                                if (recipient != null && !recipient.recipientEmailAddr.Equals(App.CurrentUser.UserEmail, StringComparison.OrdinalIgnoreCase))
                                    draftMessage.BccRecipients.Add(recipient);
                            }
                        }
                    }
                }

                draftMessage.attachments = null;
                draftMessage.subject = "Re:" + viewModel.Subject;
                draftMessage.body = "\n\n----- original message --------\n" + htmlLabel.PlainText;
                if (string.IsNullOrEmpty(draftMessage.body) == false)
                    draftMessage.body = draftMessage.body.Replace("<br>", "\n");

                Navigation.PushAsync(new NewItemPage(draftMessage));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private Recipient ProcessReplyRecipient(string recpStr)
        {
            string userName;
            string userEmail;

            Utils.Utils.ParseEmailString(recpStr, out userName, out userEmail);
            if (string.IsNullOrEmpty(userEmail))
                return null;

            var recipient = App.Recipients.Where(x => x.recipientEmailAddr.Equals(userEmail)).Select(x => x).FirstOrDefault();

            if (recipient == null)
            {
                recipient = new Recipient
                {
                    recipientName = string.Empty,
                    recipientEmailAddr = userEmail,
                    recipientType = 0,
                    recipientPosition = 0,
                };

                Utils.Utils.AddOrUpdateRecipient(recipient);
            }

            return recipient;
        }

        private void ShowLoading(bool isShowing)
        {
            if (isShowing)
                UserDialogs.Instance.Loading(AppResources.Pleasewait, null, null, true);
            else
                UserDialogs.Instance.Loading().Hide();
        }
    }
}