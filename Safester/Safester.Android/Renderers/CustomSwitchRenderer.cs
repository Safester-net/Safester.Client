using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Text;
using Android.Views;
using Android.Widget;
using Safester.Controls;
using Safester.Droid.Renderers;
using Safester.Utils;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(CustomSwitch), typeof(CustomSwitchRenderer))]
namespace Safester.Droid.Renderers
{
    public class CustomSwitchRenderer : SwitchRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Xamarin.Forms.Switch> e)
        {
            base.OnElementChanged(e);

            if (Control != null)
            {
                Control.CheckedChange += Control_CheckedChange;                
            }

            if (e.NewElement != null)
            {
                (e.NewElement as CustomSwitch).ColorChangedEvent += ChangeThumbColor;
            }
        }
        
        private void Control_CheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            ChangeThumbColor();
        }

        private void ChangeThumbColor()
        {
            if (Control != null)
            {
                Element.IsToggled = Control.Checked;

                if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.Lollipop)
                {
                    if (Control.Checked)
                        Control.ThumbDrawable.SetColorFilter(GetActiveColor(), Android.Graphics.PorterDuff.Mode.SrcAtop);
                    else
                        Control.ThumbDrawable.SetColorFilter(GetDeActiveColor(), Android.Graphics.PorterDuff.Mode.SrcAtop);

                    if (Control.Checked)
                        Control.TrackDrawable.SetColorFilter(GetActiveTrackerColor(), Android.Graphics.PorterDuff.Mode.Overlay);
                    else
                        Control.TrackDrawable.SetColorFilter(GetDeActiveTrackerColor(), Android.Graphics.PorterDuff.Mode.Overlay);
                }
            }
        }

        private Android.Graphics.Color GetActiveColor()
        {
            if (ThemeHelper.CurrentTheme == ThemeStyle.STANDARD_THEME)
                return Xamarin.Forms.Color.FromHex("#0356b3").ToAndroid();

            return Xamarin.Forms.Color.FromHex("#FFFFFF").ToAndroid();
        }

        private Android.Graphics.Color GetDeActiveColor()
        {
            if (ThemeHelper.CurrentTheme == ThemeStyle.STANDARD_THEME)
                return Xamarin.Forms.Color.FromHex("#747474").ToAndroid();

            return Xamarin.Forms.Color.FromHex("#ECECEC").ToAndroid();
        }

        private Android.Graphics.Color GetActiveTrackerColor()
        {
            if (ThemeHelper.CurrentTheme == ThemeStyle.STANDARD_THEME)
                return Xamarin.Forms.Color.FromHex("#2196F3").ToAndroid();

            return Xamarin.Forms.Color.FromHex("#4197FE").ToAndroid();
        }

        private Android.Graphics.Color GetDeActiveTrackerColor()
        {
            if (ThemeHelper.CurrentTheme == ThemeStyle.STANDARD_THEME)
                return Xamarin.Forms.Color.FromHex("#8B8B8B").ToAndroid();

            return Xamarin.Forms.Color.FromHex("#B2B2B2").ToAndroid();
        }
    }
}
