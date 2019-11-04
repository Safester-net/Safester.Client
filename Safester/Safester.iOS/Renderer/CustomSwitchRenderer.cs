using System.ComponentModel;
using Safester.Controls;
using Safester.iOS.Renderer;
using Safester.Utils;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(CustomSwitch), typeof(CustomSwitchRenderer))]
namespace Safester.iOS.Renderer
{
    public class CustomSwitchRenderer : SwitchRenderer
    {
        UISwitch _control { get; set; }

        protected override void OnElementChanged(ElementChangedEventArgs<Xamarin.Forms.Switch> e)
        {
            base.OnElementChanged(e);
            
            if (Control != null)
            {
                Control.OnTintColor = GetActiveTrackerColor();
                Control.TintColor = GetDeActiveTrackerColor();

                Control.ValueChanged += Control_ValueChanged;
                _control = Control;
            }

            if (e.NewElement != null)
            {
                (e.NewElement as CustomSwitch).ColorChangedEvent += ChangeThumbColor;
            }
        }        

        private void Control_ValueChanged(object sender, System.EventArgs e)
        {
            ChangeThumbColor();
        }

        private void ChangeThumbColor()
        {
            if (_control != null)
            {
                if (_control.On)
                {
                    _control.ThumbTintColor = GetActiveColor();
                }
                else
                {
                    _control.ThumbTintColor = GetDeActiveColor();
                }
            }
        }

        private UIColor GetActiveColor()
        {
            if (ThemeHelper.CurrentTheme == ThemeStyle.STANDARD_THEME)
                return Xamarin.Forms.Color.FromHex("#0356b3").ToUIColor();

            return Xamarin.Forms.Color.FromHex("#FFFFFF").ToUIColor();
        }

        private UIColor GetDeActiveColor()
        {
            if (ThemeHelper.CurrentTheme == ThemeStyle.STANDARD_THEME)
                return Xamarin.Forms.Color.FromHex("#747474").ToUIColor();

            return Xamarin.Forms.Color.FromHex("#ECECEC").ToUIColor();
        }

        private UIColor GetActiveTrackerColor()
        {
            if (ThemeHelper.CurrentTheme == ThemeStyle.STANDARD_THEME)
                return Xamarin.Forms.Color.FromHex("#2196F3").ToUIColor();

            return Xamarin.Forms.Color.FromHex("#4197FE").ToUIColor();
        }

        private UIColor GetDeActiveTrackerColor()
        {
            if (ThemeHelper.CurrentTheme == ThemeStyle.STANDARD_THEME)
                return Xamarin.Forms.Color.FromHex("#8B8B8B").ToUIColor();

            return Xamarin.Forms.Color.FromHex("#B2B2B2").ToUIColor();
        }
    }
}