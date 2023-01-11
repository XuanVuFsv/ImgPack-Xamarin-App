using PORO.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace PORO.Models
{
    public class FilterOption
    {
        public FilterOption(FilterOption filterOption)
        {
            FilterTypeChosen = filterOption.FilterTypeChosen;
            ColorContrast = filterOption.ColorContrast;
            ColorBrightness = filterOption.ColorBrightness;
            ColorWhiteBalance = filterOption.ColorWhiteBalance;
            GrayContrast = filterOption.GrayContrast;
            GrayBrightness = filterOption.GrayBrightness;
            BlackWhiteContrast = filterOption.BlackWhiteContrast;
        }

        public FilterOption()
        {
        }

        public FilterType FilterTypeChosen { get; set; }
        public float ColorContrast { get; set; }
        public float ColorBrightness { get; set; }
        public float ColorWhiteBalance { get; set; }
        public float GrayContrast { get; set; }
        public float GrayBrightness { get; set; }
        public float BlackWhiteContrast { get; set; }
    }
}
