using System;
using Safester.iOS.Renderer;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(NavigationPage), typeof(CustomNavigationRenderer))]

namespace Safester.iOS.Renderer
{
    public class CustomNavigationRenderer : NavigationRenderer
    {

        public override void ViewDidLayoutSubviews()
        {
            base.ViewDidLayoutSubviews();

            try
            {
                var logo = NavigationBar.TopItem.RightBarButtonItem.Image;
                if (logo != null)
                {
                    if (logo.RenderingMode == UIImageRenderingMode.AlwaysOriginal)
                    {
                        return;
                    }
                }

                foreach (var item in NavigationBar.TopItem.RightBarButtonItems)
                {
                    if (item.Image == null)
                        continue;

                    item.Image = item.Image.ImageWithRenderingMode(UIImageRenderingMode.AlwaysOriginal);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
