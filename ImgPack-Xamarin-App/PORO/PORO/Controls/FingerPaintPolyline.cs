using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace PORO.Controls
{
    public class FingerPaintPolyline
    {
        public FingerPaintPolyline()
        {
            Path = new SKPath();
        }

        public SKPath Path { set; get; }

        public SKColor StrokeColor { set; get; }
        public SKBlendMode BlendMode { set; get; }

        public float StrokeWidth { set; get; }
    }
}
