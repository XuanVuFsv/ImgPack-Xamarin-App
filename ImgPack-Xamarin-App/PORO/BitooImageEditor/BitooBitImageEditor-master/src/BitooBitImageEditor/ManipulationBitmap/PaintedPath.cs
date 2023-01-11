using SkiaSharp;
using System;

namespace BitooBitImageEditor.ManipulationBitmap
{
    internal class PaintedPath : IDisposable
    {
        internal SKPath Path { get; set; }
        internal SKColor Color { get; set; }
        internal SKBlendMode SKBlendMode { get; set; }
        internal float StrokeWidth { get; set; }

        #region IDisposable Support
        private bool disposedValue = false;

        public PaintedPath(SKPath path, SKColor color, SKBlendMode sKBlendMode = SKBlendMode.SrcOver, float strokeWidth = 10)
        {
            Path = path;
            Color = color;
            SKBlendMode = sKBlendMode;
            StrokeWidth = strokeWidth;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                { }

                Path.Dispose();

                disposedValue = true;
            }
        }

        ~PaintedPath()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
