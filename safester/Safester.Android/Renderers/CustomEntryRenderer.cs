using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Text;
using Android.Views;
using Android.Widget;
using Safester.Controls;
using Safester.Droid.Renderers;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(RoundedBorderEntry), typeof(CustomEntryRenderer))]
namespace Safester.Droid.Renderers
{
    public class CustomEntryRenderer : EntryRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged(e);
            Control?.SetBackgroundColor(Android.Graphics.Color.White);
            Control?.SetBackgroundDrawable(MainApplication.Context.GetDrawable(Resource.Drawable.rounded_edittext));

            Control?.SetPadding(15, 15, 15, 15);

            (Control as Android.Widget.EditText).Gravity = Android.Views.GravityFlags.CenterVertical;
        }
    }
}
