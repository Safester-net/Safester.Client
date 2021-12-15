using System.ComponentModel;
using Safester.iOS.Renderer;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(Entry), typeof(CustomTextFieldRenderer))]
namespace Safester.iOS.Renderer
{
    public class CustomTextFieldRenderer : EntryRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged(e);

            if (Control != null)
            {
                Control.TextContentType = UITextContentType.OneTimeCode;
                Control.SmartQuotesType = UITextSmartQuotesType.No;
                Control.SmartDashesType = UITextSmartDashesType.No;
                Control.SmartInsertDeleteType = UITextSmartInsertDeleteType.No;
            }
        }
    }
}