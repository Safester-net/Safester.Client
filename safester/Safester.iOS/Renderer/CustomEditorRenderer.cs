using System.ComponentModel;
using Safester.iOS.Renderer;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(Editor), typeof(CustomEditorRenderer))]
namespace Safester.iOS.Renderer
{
    public class CustomEditorRenderer : EditorRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Editor> e)
        {
            base.OnElementChanged(e);

            if (e.NewElement != null)
            {
                e.NewElement.Focused -= Element_Focused;
                e.NewElement.Focused += Element_Focused;
            }
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);
        }

        void Element_Focused(object sender, FocusEventArgs e)
        {
            if (Control != null)
            {
                //Control.ScrollRangeToVisible(new Foundation.NSRange(0, 0));
                //Control.SetContentOffset(new CoreGraphics.CGPoint(0, 0), false);
                Control.SelectedTextRange = Control.GetTextRange(Control.BeginningOfDocument, Control.BeginningOfDocument);
            }
        }
    }
}