using PORO.Models;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace PORO.Untilities
{
    public class FilterMatrix
    {
        #region GetColorFilterMatrix

        public static SKColorFilter GetColorFilterMatrix(FilterOption currentFilterOption)
        {
            // Brightness
            var br = currentFilterOption.ColorBrightness;

            // Contrast
            var c = currentFilterOption.ColorContrast;

            // White Balance
            var r = currentFilterOption.ColorWhiteBalance < 0 ? 0 - currentFilterOption.ColorWhiteBalance : 0;
            var bl = currentFilterOption.ColorWhiteBalance > 0 ? currentFilterOption.ColorWhiteBalance : 0;

            var colorFilter =
                SKColorFilter.CreateColorMatrix(new float[]
                {
                        1+c+r,  0,      0,      0,      br+128*(0-c),
                        0,      1+c,    0,      0,      br+128*(0-c),
                        0,      0,      1+c+bl, 0,      br+128*(0-c),
                        0,      0,      0,      1,      0
                });

            return colorFilter;
        }

        #endregion

        #region GetGrayScaleFilterMatrix

        public static SKColorFilter GetGrayScaleFilterMatrix(FilterOption currentFilterOption)
        {
            // Brightness
            var br = currentFilterOption.GrayBrightness;

            // Contrast
            var c = currentFilterOption.GrayContrast;

            var colorFilter =
                SKColorFilter.CreateColorMatrix(new float[]
                {
                        (1+c)*0.33f,  (1+c)*0.33f,  (1+c)*0.33f,   0,  br+128*(0-c),
                        (1+c)*0.33f,  (1+c)*0.33f,  (1+c)*0.33f,   0,  br+128*(0-c),
                        (1+c)*0.33f,  (1+c)*0.33f,  (1+c)*0.33f,   0,  br+128*(0-c),
                        0,            0,            0,             1,  0
                });

            return colorFilter;
        }

        #endregion

        #region GetBlackAndWhiteFilterMatrix

        public static SKColorFilter GetBlackAndWhiteFilterMatrix(FilterOption currentFilterOption)
        {
            // Contrast
            var c = currentFilterOption.BlackWhiteContrast;

            var colorFilter =
                SKColorFilter.CreateColorMatrix(new float[]
                {
                        (1+c)*85f,  (1+c)*85f,  (1+c)*85f,   0,  -128*255,
                        (1+c)*85f,  (1+c)*85f,  (1+c)*85f,   0,  -128*255,
                        (1+c)*85f,  (1+c)*85f,  (1+c)*85f,   0,  -128*255,
                        0,          0,          0,           1,  0
                });

            return colorFilter;
        }

        #endregion

        #region InvertFilterMatrix
        public static SKColorFilter GetInvertFilterMatrix(FilterOption currentFilterOption)
        {
            var colorFilter =
                SKColorFilter.CreateColorMatrix(new float[]
                {
                    -1,  0,  0,  0, 255,
                     0, -1,  0,  0, 255,
                     0,  0, -1,  0, 255,
                     0,  0,  0,  1,   0
                });
            return colorFilter;
        }
        #endregion

        #region SwapFilterMatrix
        public static SKColorFilter GetSwapFilterMatrix(FilterOption currentFilterOption)
        {
            var colorFilter =
                SKColorFilter.CreateColorMatrix(new float[]
                {
                    0, 0, 1, 0, 0,
                    0, 1, 0, 0, 0,
                    1, 1, 0, 0, 0,
                    0, 0, 0, 1, 0
                });
            return colorFilter;
        }
        #endregion

        #region SepiaFilterMatrix
        public static SKColorFilter GetSepiaFilterMatrix(FilterOption currentFilterOption)
        {
            var colorFilter =
                SKColorFilter.CreateColorMatrix(new float[]
                {
                    0.393f, 0.769f, 0.189f, 0, 0,
                    0.349f, 0.686f, 0.168f, 0, 0,
                    0.272f, 0.534f, 0.131f, 0, 0,
                    0, 0, 0, 1, 0
                });
            return colorFilter;
        }
        #endregion

        #region PolaroidFilterMatrix
        public static SKColorFilter GetPolaroidFilterMatrix(FilterOption currentFilterOption)
        {
            var colorFilter =
                SKColorFilter.CreateColorMatrix(new float[]
                {
                    1.438f, -0.062f, -0.062f, 0, 0,
                    -0.122f, 1.378f, 0.122f, 0, 0,
                    -0.016f, 0.016f, 1.483f, 0, 0,
                    0, 0, 0, 1, 0
                });
            return colorFilter;
        }
        #endregion

        #region SunsetFilterMatrix
        public static SKColorFilter GetSunsetFilterMatrix(FilterOption currentFilterOption)
        {
            var colorFilter =
                SKColorFilter.CreateColorMatrix(new float[]
                {
                     1.5f,  0,  0,  0, 0,
                     0, 1f,  0.5f,  0, 0,
                     0,  0.5f, 1,  0, 0,
                     0,  0,  0,  1.5f,   0
                });
            return colorFilter;
        }
        #endregion

        #region GrayYellowFilterMatrix
        public static SKColorFilter GetGrayYellowFilterMatrix(FilterOption currentFilterOption)
        {
            var colorFilter =
                SKColorFilter.CreateColorMatrix(new float[]
                {
                     1,  0.5f,  -1f,  -0.1f, 0.2f,
                     0.1f, 0.5f,  0.3f,  0, 0,
                     -0.5f,  1, 0.5f,  0, 0,
                     0,  0,  0,  1,   0
                });
            return colorFilter;
        }
        #endregion

        #region GrayYellowFilterMatrix1
        public static SKColorFilter GetGrayYellowFilterMatrix1(FilterOption currentFilterOption)
        {
            var colorFilter =
                SKColorFilter.CreateColorMatrix(new float[]
                {
                     1.25f,  0,  -0.75f,  -0.05f, 0.1f,
                    0.05f, 0.75f,  0.15f,  0, 0,
                    -0.25f,  0.5f, 0.75f,  0, 0,
                    0,  0,  0,  1,   0
                });
            return colorFilter;
        }
        #endregion

        #region RedBlueFilterMatrix
        public static SKColorFilter GetRedBlueFilterMatrix(FilterOption currentFilterOption)
        {
            var colorFilter =
                SKColorFilter.CreateColorMatrix(new float[]
                {
                    1.5f,  -0.5f,  -0.5f,  0, 0,
                    -0.3f, 1.5f,  0.2f,  0, 0,
                    -0.5f,  0.5f, 1.5f,  0, 0,
                    0,  0,  0,  1,   0
                });
            return colorFilter;
        }
        #endregion

        #region RedBlueFilterMatrix1
        public static SKColorFilter GetRedBlueFilterMatrix1(FilterOption currentFilterOption)
        {
            var colorFilter =
                SKColorFilter.CreateColorMatrix(new float[]
                {
                    1.25f,  0,  -0.75f,  -0.05f, 0.1f,
                    -0.2f, 1,  0.25f,  0, 0,
                    -0.25f,  0.5f, 1.25f,  0.05f, 0,
                    0,  0,  0,  1,   0
                });
            return colorFilter;
        }
        #endregion

        #region SkyBlueFilterMatrix
        public static SKColorFilter GetSkyBlueFilterMatrix(FilterOption currentFilterOption)
        {
            var colorFilter =
                SKColorFilter.CreateColorMatrix(new float[]
                {
                    1.5f,  -0.5f,  -0.5f,  0, 0,
                    0, 1,  0,  0, 0,
                    0,  0, 1,  0, 0,
                    0,  0,  0,  1,   0
                });
            return colorFilter;
        }
        #endregion

        #region BlueFilterMatrix
        public static SKColorFilter GetBlueFilterMatrix(FilterOption currentFilterOption)
        {
            var colorFilter =
                SKColorFilter.CreateColorMatrix(new float[]
                {
                    1,  0.5f,  -1f,  -0.1f, 0.2f,
                    0.1f, 0.5f,  0.3f,  0, 0,
                    0,  0.5f, 1,  0.1f, 0,
                    0,  0,  0,  1,   0
                });
            return colorFilter;
        }
        #endregion

        #region BlueFilterMatrix1
        public static SKColorFilter GetBlueFilterMatrix1(FilterOption currentFilterOption)
        {
            var colorFilter =
                SKColorFilter.CreateColorMatrix(new float[]
                {
                    0.75f,  1,  -1f,  -0.1f, 0.2f,
                    -0.2f, 0.75f,  0.65f,  0, 0,
                    0,  0.5f, 1,  0.1f, 0,
                    0,  0,  0,  1,   0
                });
            return colorFilter;
        }
        #endregion

        #region BlueVioletFilterMatrix
        public static SKColorFilter GetBlueVioletFilterMatrix(FilterOption currentFilterOption)
        {
            var colorFilter =
                SKColorFilter.CreateColorMatrix(new float[]
                {
                    0.5f,  1.5f,  -1f,  -0.1f, 0.2f,
                    -0.5f, 1,  1,  0, 0,
                    0,  0.5f, 1,  0.1f, 0,
                    0,  0,  0,  1,   0
                });
            return colorFilter;
        }
        #endregion

        #region RedBrowFilterMatrix
        public static SKColorFilter GetRedBrowFilterMatrix(FilterOption currentFilterOption)
        {
            var colorFilter =
                SKColorFilter.CreateColorMatrix(new float[]
                {
                    0.5f,  1.5f,  -1f,  0, 0,
                    -0.5f, 1,  1,  0, 0,
                    -1,  1, 1,  0, 0,
                    0,  0,  0,  1,   0
                });
            return colorFilter;
        }
        #endregion

        #region ClassicFilterMatrix
        public static SKColorFilter GetClassicFilterMatrix(FilterOption currentFilterOption)
        {
            var colorFilter =
                SKColorFilter.CreateColorMatrix(new float[]
                {
                    0.5f,  0,  0,  0, 0,
                    0, 1,  0,  0, 0,
                    0,  0, 1,  0, 0,
                    0,  0,  0,  1,   0
                });
            return colorFilter;
        }
        #endregion

        #region YellowClassicFilterMatrix
        public static SKColorFilter GetYellowClassicFilterMatrix(FilterOption currentFilterOption)
        {
            var colorFilter =
                SKColorFilter.CreateColorMatrix(new float[]
                {
                    0.5f,  1,  0.1f,  0, 0,
                    0, 1,  0.5f,  0, 0,
                    0,  0, 1,  0, 0,
                    0,  0,  0,  1,   0
                });
            return colorFilter;
        }
        #endregion

        #region YellowClassicFilterMatrix1
        public static SKColorFilter GetYellowClassicFilterMatrix1(FilterOption currentFilterOption)
        {
            var colorFilter =
                SKColorFilter.CreateColorMatrix(new float[]
                {
                    0.5f,  1,  0.1f,  0, 0,
                    0, 1,  0.1f,  0, 0,
                    0,  0, 1,  0, 0,
                    0,  0,  0,  1,   0
                });
            return colorFilter;
        }
        #endregion

        #region YellowClassicFilterMatrix2
        public static SKColorFilter GetYellowClassicFilterMatrix2(FilterOption currentFilterOption)
        {
            var colorFilter =
                SKColorFilter.CreateColorMatrix(new float[]
                {
                    0.5f,  1,  0,  0, 0,
                    0, 1,  0,  0, 0,
                    0,  0, 1,  0, 0,
                    0,  0,  0,  1,   0
                });
            return colorFilter;
        }
        #endregion

        #region BlueClassicFilterMatrix
        public static SKColorFilter GetBlueClassicFilterMatrix(FilterOption currentFilterOption)
        {
            var colorFilter =
                SKColorFilter.CreateColorMatrix(new float[]
                {
                    0.5f,  0,  0,  0, 0,
                    0, 1,  0,  0, 0,
                    0,  0, 1,  0.05f, 0,
                    0,  0,  0,  1,   0
                });
            return colorFilter;
        }
        #endregion

        #region BlueClassicFilterMatrix1
        public static SKColorFilter GetBlueClassicFilterMatrix1(FilterOption currentFilterOption)
        {
            var colorFilter =
                SKColorFilter.CreateColorMatrix(new float[]
                {
                    0.5f,  0.5f,  -1f,  0, 0,
                    0.1f, 0.5f,  0.3f,  0, 0,
                    0,  0, 1,  0, 0,
                    0,  0,  0,  1,   0
                });
            return colorFilter;
        }
        #endregion

        #region BlueClassicFilterMatrix2
        public static SKColorFilter GetBlueClassicFilterMatrix2(FilterOption currentFilterOption)
        {
            var colorFilter =
                SKColorFilter.CreateColorMatrix(new float[]
                {
                    0.5f,  1,  -1f,  0, 0,
                    0.1f, 0.5f,  0.3f,  0, 0,
                    -1,  1, 0.5f,  0, 0,
                    0,  0,  0,  1,   0
                });
            return colorFilter;
        }
        #endregion
    }
}
