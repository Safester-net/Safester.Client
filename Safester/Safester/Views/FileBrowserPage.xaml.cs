using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace Safester.Views
{
    public partial class FileBrowserPage : ContentPage
    {
        public FileBrowserPage(string fileUri)
        {
            InitializeComponent();

            webView.Source = new UrlWebViewSource()
            {
                Url = "file://" + fileUri
            };
        }
    }
}
