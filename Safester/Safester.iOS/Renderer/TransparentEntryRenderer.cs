using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoreGraphics;
using Safester.Controls;
using Safester.iOS.Renderer;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(TransparentEntry), typeof(TransparentEntryRenderer))]
namespace Safester.iOS.Renderer
{
    public class TransparentEntryRenderer : EntryRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged(e);

            if (Control != null)
            {
                Control.BackgroundColor = new UIColor(0, 0, 0, 0);

                Control.TextContentType = UITextContentType.OneTimeCode;
                Control.SmartQuotesType = UITextSmartQuotesType.No;
                Control.SmartDashesType = UITextSmartDashesType.No;
                Control.SmartInsertDeleteType = UITextSmartInsertDeleteType.No;
            }
        }
    }
}
