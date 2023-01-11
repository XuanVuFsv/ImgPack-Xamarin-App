using Android.App;
using Android.Content;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using PORO.Controls;
using PORO.Droid.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(EntryCustom), typeof(BorderEntry))]
namespace PORO.Droid.Controls
{
    public class BorderEntry : EntryRenderer
    {
        public BorderEntry(Context context) : base(context)
        {

        }
        EntryCustom borderEntry;
        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged(e);
            if (e.OldElement == null)
            {
                borderEntry = (EntryCustom)e.NewElement;
                var gradientDrawable = new GradientDrawable();
                gradientDrawable.SetCornerRadius(10);
                gradientDrawable.SetStroke(2, borderEntry.EntryBorderColor.ToAndroid());
                Control.SetBackground(gradientDrawable);
                Control.SetPadding(15, borderEntry.Padding, borderEntry.Padding, borderEntry.Padding);
            }
        }
    }
}