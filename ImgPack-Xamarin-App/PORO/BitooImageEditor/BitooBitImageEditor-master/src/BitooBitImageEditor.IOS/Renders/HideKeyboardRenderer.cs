using BitooBitImageEditor.Helper.Controls;
using BitooBitImageEditor.IOS.Renders;
using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UIKit;
using Xamarin.Forms;

[assembly: ExportRenderer(typeof(IHideKeyboard), typeof(HideKeyboardRenderer))]
namespace BitooBitImageEditor.IOS.Renders
{
    public class HideKeyboardRenderer : IHideKeyboard
    {
        public void HideKeyboard()
        {
            UIApplication.SharedApplication.KeyWindow.EndEditing(true);
        }
    }
}