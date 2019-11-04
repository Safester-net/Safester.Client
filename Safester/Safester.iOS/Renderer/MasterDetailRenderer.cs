using System;
using Safester.iOS.Renderer;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(MasterDetailPage), typeof(CustomMasterDetailRenderer))]

namespace Safester.iOS.Renderer
{
    public class CustomMasterDetailRenderer : PhoneMasterDetailRenderer
    {

        public override void ViewDidLayoutSubviews()
        {
            base.ViewDidLayoutSubviews();

        }
    }
}
