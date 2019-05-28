using Safester.Droid.Renderers;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(ScrollView), typeof(CustomScrollRenderer))]
namespace Safester.Droid.Renderers
{
    public class CustomScrollRenderer : ScrollViewRenderer
    {
        protected override void OnElementChanged(VisualElementChangedEventArgs e)
        {
            base.OnElementChanged(e);
            this.NestedScrollingEnabled = true;
        }
    }
}