using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace PORO.Controls
{
    public class EntryCustom : Entry
    {
        public static readonly BindableProperty ConerRadiusProperty =
            BindableProperty.Create("ConnerRadius", typeof(int), typeof(EntryCustom), 0);
        public int EntryConnerRadius
        {
            get
            {
                return (int)GetValue(ConerRadiusProperty);
            }
            set
            {
                SetValue(ConerRadiusProperty, value);
            }
        }
        public static readonly BindableProperty PaddingProperty =
            BindableProperty.Create("ConnerRadius", typeof(int), typeof(EntryCustom), 0);
        public int Padding
        {
            get
            {
                return (int)GetValue(PaddingProperty);
            }
            set
            {
                SetValue(PaddingProperty, value);
            }
        }
        public static readonly BindableProperty BorderColorProperty =
            BindableProperty.Create("BorderThickness", typeof(Color), typeof(EntryCustom), Color.Blue);
        public Color EntryBorderColor
        {
            get
            {
                return (Color)GetValue(BorderColorProperty);
            }
            set
            {
                SetValue(BorderColorProperty, value);
            }
        }
    }
}
