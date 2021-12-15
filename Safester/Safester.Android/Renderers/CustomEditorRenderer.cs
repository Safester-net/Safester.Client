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
using Android.Text.Method;
using Android.Views;
using Android.Widget;
using Safester.Controls;
using Safester.Droid.Renderers;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(Editor), typeof(CustomEditorRenderer))]
namespace Safester.Droid.Renderers
{
    public class CustomEditorRenderer : EditorRenderer
    {
        public CustomEditorRenderer(Context context) : base(context)
        {

        }        

        protected override void OnElementChanged(ElementChangedEventArgs<Editor> e)
        {
            base.OnElementChanged(e);

            if (e.OldElement == null)
            {
                var nativeEditText = (global::Android.Widget.EditText)Control;

                //While scrolling inside Editor stop scrolling parent view.
                nativeEditText.OverScrollMode = OverScrollMode.Always;
                nativeEditText.ScrollBarStyle = ScrollbarStyles.InsideInset;
                nativeEditText.SetTextIsSelectable(true);
                nativeEditText.SetOnTouchListener(new DroidTouchListner());
            }

            if (e.NewElement != null)
            {
                if (e.NewElement is CustomEditor)
                {
                    (e.NewElement as CustomEditor).SetStartPosition += () =>
                    {
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            var nativeEditText = (global::Android.Widget.EditText)Control;
                            nativeEditText.SetSelection(0);
                        });
                    };
                }
            }
        }
    }

    public class DroidTouchListner : Java.Lang.Object, Android.Views.View.IOnTouchListener
    {
        public bool OnTouch(Android.Views.View v, MotionEvent e)
        {
            v.Parent?.RequestDisallowInterceptTouchEvent(true);
            if ((e.Action & MotionEventActions.Up) != 0 && (e.ActionMasked & MotionEventActions.Up) != 0)
            {
                v.Parent?.RequestDisallowInterceptTouchEvent(false);
            }

            return false;
        }
    }
}