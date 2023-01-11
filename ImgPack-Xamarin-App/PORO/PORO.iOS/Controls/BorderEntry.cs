using CoreGraphics;
using Foundation;
using PORO.Controls;
using PORO.iOS.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(EntryCustom), typeof(BorderEntry))]
namespace PORO.iOS.Controls
{
    public class BorderEntry : EntryRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged(e);
            if (e.OldElement == null)
            {
                var entryCustom = (EntryCustom)e.NewElement;
                Control.Layer.CornerRadius = entryCustom.EntryConnerRadius;
                Control.Layer.BorderColor = entryCustom.EntryBorderColor.ToCGColor();
                Control.LeftView = new UIView(new CGRect(0, 0, 15, 0));
                Control.LeftViewMode = UITextFieldViewMode.Always;
                Control.RightView = new UIView(new CGRect(0, 0, 15, 0));
                Control.RightViewMode = UITextFieldViewMode.Always;
                Control.BorderStyle = UITextBorderStyle.RoundedRect;

            }
        }
    }
}