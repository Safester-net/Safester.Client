using System;
using Xamarin.Forms;
using Safester.Services;
using Safester.iOS;
using UIKit;
using Foundation;
using System.IO;

[assembly: Xamarin.Forms.Dependency(typeof(iOSFilesService))]
namespace Safester.iOS
{
    public class iOSFilesService : IFilesService
    {
        public void OpenUri(string uri)
        {
            try
            {
                var PreviewController = UIDocumentInteractionController.FromUrl(NSUrl.FromFilename(uri));
                PreviewController.Delegate = new CustomInteractionDelegate(UIApplication.SharedApplication.KeyWindow.RootViewController);

                Device.BeginInvokeOnMainThread(() =>
                {
                    PreviewController.PresentPreview(true);
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public string GetDownloadFolder()
        {
            var pathFile = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            pathFile = Path.Combine(pathFile, "Downloads");

            try
            {
                Directory.CreateDirectory(pathFile);
            }
            catch (Exception ex)
            {

            }

            return pathFile;
        }
    }

    public class CustomInteractionDelegate : UIDocumentInteractionControllerDelegate
    {
        UIViewController parent;
        public CustomInteractionDelegate(UIViewController controller)
        {
            parent = controller;
        }

        public override UIViewController ViewControllerForPreview(UIDocumentInteractionController controller)
        {
            return parent;
        }
    }
}

