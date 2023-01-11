using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Org.Opencv.Core;
using PORO.Droid.Services;
using PORO.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

[assembly: Xamarin.Forms.Dependency(typeof(EditService))]
namespace PORO.Droid.Services
{
    public class EditService : IEditService
    {
        #region Properties
        Bitmap sourcebm, resultBitmap, grayBitmapDefault, bitmap, copyBitmap;
        Bitmap grayBitmap, blackWhiteBitmap, inverBitmap, swapBitmap, sepiaBitmap, polaroidBitmap, whiteToAlphaBitmap;
        Bitmap rotateBitmap, leftRotateBitmap, rightRotateBitmap, reverseRotateBitmap;
        Bitmap oriBitmap, cropBitmap, CroppedImage;
        WeakReference<Bitmap> drawBitmap;

        private const int whiteBalanceOffset = 2000;
        private const int whiteBalanceMaxProgress = 9000;
        private enum State { Color, BlackWhite, GreyScale, Invert, Sepia, Polaroid, WhiteToAlpha, Swap, Rotate, Sunset }
        private State modeState = State.Color;

        enum StateRotate { _default, _left, _right, _reverse };
        StateRotate stateRotate = StateRotate._default;
        private int contrastValue, brightnessValue, whiteBalanceValue;

        int state = 0;
        public int Angle;
        public int REQUEST_CODE = 123;
        #endregion

        #region Init
        public void Init()
        {
            var result = ChangeGrayscale(sourcebm);
            grayBitmap = DrawableToBitmap(result);
            grayBitmapDefault = grayBitmap;
            Reset();
        }

        #endregion

        #region Reset

        public void Reset()
        {
            brightnessValue = 50;
            contrastValue = 100;
            whiteBalanceValue = whiteBalanceMaxProgress / 2;
            if (grayBitmap != null)
            {
                grayBitmap.Recycle();
            }
            if (blackWhiteBitmap != null)
            {
                blackWhiteBitmap.Recycle();
            }
            if (inverBitmap != null)
            {
                inverBitmap.Recycle();
            }
            if (swapBitmap != null)
            {
                swapBitmap.Recycle();
            }
            if (sepiaBitmap != null)
            {
                sepiaBitmap.Recycle();
            }
            if (polaroidBitmap != null)
            {
                polaroidBitmap.Recycle();
            }
        }

        #endregion

        #region ImageFilter

        private Bitmap ImageFilter(Bitmap bm, int brightness, int contrast, bool isSave = false)
        {
            Bitmap result;

            System.Diagnostics.Debug.WriteLine($"brightness {brightness - 50}, contrast {(double)contrast / 100}");

            Mat src = new Mat(bm.Height, bm.Width, CvType.Cv8uc1);
            Org.Opencv.Android.Utils.BitmapToMat(bm, src);
            src.ConvertTo(src, -1, (double)contrast / 100, brightness - 50);
            try
            {
                result = Bitmap.CreateBitmap(src.Cols(), src.Rows(), Bitmap.Config.Argb8888);

            }
            catch
            {
                src.Release();
                return resultBitmap;
            }
            Org.Opencv.Android.Utils.MatToBitmap(src, result);
            src.Release();
            if (modeState == EditService.State.Color)
            {
                var temp = ChangeDrawableWhiteBalance(result, (int)whiteBalanceValue + whiteBalanceOffset);
                result = DrawableToBitmap(temp, bm);
            }
            return result;

        }

        #endregion

        #region ChangeDrawableWhiteBalance

        private Drawable ChangeDrawableWhiteBalance(Bitmap bmp, int whiteBalance)
        {
            float temperature = whiteBalance / 100;
            float red;
            float green;
            float blue;

            // Calculate red
            if (temperature <= 66)
                red = 255;
            else
            {
                red = temperature - 60;
                red = (float)(329.698727446 * (Math.Pow((double)red, -0.1332047592)));
                if (red < 0)
                    red = 0;
                if (red > 255)
                    red = 255;
            }

            // Calculate green
            if (temperature <= 66)
            {
                green = temperature;
                green = (float)(99.4708025861 * Math.Log(green) - 161.1195681661);
                if (green < 0)
                    green = 0;
                if (green > 255)
                    green = 255;
            }
            else
            {
                green = temperature - 60;
                green = (float)(288.1221695283 * (Math.Pow((double)green, -0.0755148492)));
                if (green < 0)
                    green = 0;
                if (green > 255)
                    green = 255;
            }

            // Calculate blue
            if (temperature >= 66)
                blue = 255;
            else if (temperature <= 19)
                blue = 0;
            else
            {
                blue = temperature - 10;
                blue = (float)(138.5177312231 * Math.Log(blue) - 305.0447927307);
                if (blue < 0)
                    blue = 0;
                if (blue > 255)
                    blue = 255;
            }

            //System.Diagnostics.Debug.WriteLine("red=" + red + ", green=" + green + ", blue=" + blue);

            ColorMatrix cMtrx = new ColorMatrix(new float[]
            {
                red / 255, 0, 0, 0, 0,
                0, green / 255, 0, 0, 0,
                0, 0, blue / 255, 0, 0,
                0, 0, 0, 1, 0
            });

            ColorFilter colorFilter = new ColorMatrixColorFilter(cMtrx);
            var drawable = new BitmapDrawable(bmp);
            drawable.SetColorFilter(colorFilter);

            return drawable;
        }

        #endregion

        #region  DrawableToBitmap

        private Bitmap DrawableToBitmap(Drawable drawable, Bitmap bitmapInput = null)
        {
            Bitmap bitmap = null;

            if (drawable.IntrinsicWidth <= 0 || drawable.IntrinsicHeight <= 0)
            {
                bitmap = Bitmap.CreateBitmap(1, 1, Bitmap.Config.Argb8888);
                copyBitmap = bitmap.Copy(Bitmap.Config.Argb8888, true);
                //drawBitmap = new WeakReference<Bitmap>(Bitmap.CreateBitmap(1, 1, Bitmap.Config.Argb8888));
            }
            else
            {
                try
                {
                    if (bitmapInput == null)
                    {
                        bitmap = Bitmap.CreateBitmap(drawable.IntrinsicWidth, drawable.IntrinsicHeight, Bitmap.Config.Argb8888);
                        copyBitmap = bitmap.Copy(Bitmap.Config.Argb8888, true);
                    }

                    else
                    {
                        bitmap = Bitmap.CreateBitmap(drawable.IntrinsicWidth, drawable.IntrinsicHeight, Bitmap.Config.Argb8888);
                        copyBitmap = bitmap.Copy(Bitmap.Config.Argb8888, true);
                    }


                }
                catch
                {
                    bitmap = resultBitmap;
                    copyBitmap = bitmap.Copy(Bitmap.Config.Argb8888, true);
                }
            }
            Canvas canvas = new Canvas(copyBitmap);
            drawable.SetBounds(0, 0, canvas.Width, canvas.Height);
            drawable.Draw(canvas);
            return copyBitmap;
        }
        #endregion

        #region ChangeGrayscale

        private Drawable ChangeGrayscale(Bitmap bmp)
        {
            ColorMatrix cMtrx = new ColorMatrix(new float[]
            {
                0.3f, 0.59f, 0.11f, 0, 0,
                0.3f, 0.59f, 0.11f, 0, 0,
                0.3f, 0.59f, 0.11f, 0, 0,
                0, 0, 0, 1, 0
            });

            ColorFilter colorFilter = new ColorMatrixColorFilter(cMtrx);
            var drawable = new BitmapDrawable(bmp);
            drawable.SetColorFilter(colorFilter);

            return drawable;
        }

        #endregion

        #region ChangeBlackWhite

        private Drawable ChangeBlackWhite(Bitmap bmp, int value)
        {
            float m = 205f;
            float t = -255 * value;
            ColorMatrix cmtrx = new ColorMatrix(new float[]
            {
                1.5f, 1.5f, 1.5f, 0, 0,
                1.5f, 1.5f, 1.5f, 0, 0,
                1.5f, 1.5f, 1.5f, 0, 0,
                -1, -1, -1, 0, 1
            });
            ColorFilter colorFilter = new ColorMatrixColorFilter(cmtrx);
            var drawable = new BitmapDrawable(bmp);
            drawable.SetColorFilter(colorFilter);
            return drawable;
        }
        private Drawable ChangeBlackWhiteImage(Bitmap bmp)
        {
            ColorMatrix cmtrx = new ColorMatrix(new float[]
            {
                1.5f, 1.5f, 1.5f, 0, 0,
                1.5f, 1.5f, 1.5f, 0, 0,
                1.5f, 1.5f, 1.5f, 0, 0,
                0, 0, 0, 1, 0
            });
            ColorFilter colorFilter = new ColorMatrixColorFilter(cmtrx);
            var drawable = new BitmapDrawable(bmp);
            drawable.SetColorFilter(colorFilter);
            return drawable;
        }

        #endregion

        #region ChangeInvert
        private Drawable ChangeInvert(Bitmap bmp)
        {
            ColorMatrix cmtrx = new ColorMatrix(new float[]
            {
                 -1,  0,  0,  0, 255,
                 0, -1,  0,  0, 255,
                 0,  0, -1,  0, 255,
                 0,  0,  0,  1,   0
            });
            ColorFilter colorFilter = new ColorMatrixColorFilter(cmtrx);
            var drawable = new BitmapDrawable(bmp);
            drawable.SetColorFilter(colorFilter);
            return drawable;
        }
        #endregion

        #region ChangeSwap
        private Drawable ChangeSwap(Bitmap bmp)
        {
            ColorMatrix cmtrx = new ColorMatrix(new float[]
            {
                0, 0, 1, 0, 0,
                0, 1, 0, 0, 0,
                1, 1, 0, 0, 0,
                0, 0, 0, 1, 0

            });
            ColorFilter colorFilter = new ColorMatrixColorFilter(cmtrx);
            var drawable = new BitmapDrawable(bmp);
            drawable.SetColorFilter(colorFilter);
            return drawable;
        }
        #endregion

        #region ChangeSepia
        private Drawable ChangeSepia(Bitmap bmp)
        {
            ColorMatrix cmtrx = new ColorMatrix(new float[]
            {
                0.393f, 0.769f, 0.189f, 0, 0,
                0.349f, 0.686f, 0.168f, 0, 0,
                0.272f, 0.534f, 0.131f, 0, 0,
                0, 0, 0, 1, 0

            });
            ColorFilter colorFilter = new ColorMatrixColorFilter(cmtrx);
            var drawable = new BitmapDrawable(bmp);
            drawable.SetColorFilter(colorFilter);
            return drawable;
        }
        #endregion

        #region ChangePolaroid
        private Drawable ChangePolaroid(Bitmap bmp)
        {
            ColorMatrix cmtrx = new ColorMatrix(new float[]
            {
                1.438f, -0.062f, -0.062f, 0, 0,
                -0.122f, 1.378f, 0.122f, 0, 0,
                -0.016f, 0.016f, 1.483f, 0, 0,
                0, 0, 0, 1, 0,
                -0.03f, 0.05f, -0.02f, 0, 1

            });
            ColorFilter colorFilter = new ColorMatrixColorFilter(cmtrx);
            var drawable = new BitmapDrawable(bmp);
            drawable.SetColorFilter(colorFilter);
            return drawable;
        }
        #endregion

        #region ChangeWhitetoAlpha
        private Drawable ChangeWhitetoAlpha(Bitmap bmp)
        {
            ColorMatrix cmtrx = new ColorMatrix(new float[]
            {
                1, 0, 0, -1, 0,
                0, 1, 0, -1, 0,
                0, 0, 1, -1, 0,
                0, 0, 0, 1, 0,
                0, 0, 0, 0, 1

            });
            ColorFilter colorFilter = new ColorMatrixColorFilter(cmtrx);
            var drawable = new BitmapDrawable(bmp);
            drawable.SetColorFilter(colorFilter);
            return drawable;
        }
        #endregion

        #region SunsetColor
        private Drawable ChangeSunset(Bitmap bmp)
        {
            ColorMatrix cmtrx = new ColorMatrix(new float[]
            {
                1.5f,  0,  0,  0, 0,
                0, 1f,  0.5f,  0, 0,
                0,  0.5f, 1,  0, 0,
                0,  0,  0,  1.5f,   0

            });
            ColorFilter colorFilter = new ColorMatrixColorFilter(cmtrx);
            var drawable = new BitmapDrawable(bmp);
            drawable.SetColorFilter(colorFilter);
            return drawable;
        }
        #endregion

        #region ContrastChange
        public string ContrastChange(int value, string path)
        {
            Bitmap bitmapPath = null;
            if (path != null)
            {
                bitmapPath = BitmapFactory.DecodeFile(path);
            }
            if (modeState != State.BlackWhite)
            {
                contrastValue = (value + 100);
                if (modeState == State.Color)
                    resultBitmap = ImageFilter(bitmapPath, brightnessValue, contrastValue);
                else if (modeState == State.BlackWhite)
                    resultBitmap = ImageFilter(blackWhiteBitmap, brightnessValue, contrastValue);
                else if (modeState == State.GreyScale)
                    resultBitmap = ImageFilter(grayBitmap, brightnessValue, contrastValue);
                else if (modeState == State.Invert)
                    resultBitmap = ImageFilter(inverBitmap, brightnessValue, contrastValue);
                else if (modeState == State.Swap)
                    resultBitmap = ImageFilter(swapBitmap, brightnessValue, contrastValue);
                else if (modeState == State.Sepia)
                    resultBitmap = ImageFilter(sepiaBitmap, brightnessValue, contrastValue);
                else if (modeState == State.Polaroid)
                    resultBitmap = ImageFilter(polaroidBitmap, brightnessValue, contrastValue);
                else if (modeState == State.WhiteToAlpha)
                    resultBitmap = ImageFilter(whiteToAlphaBitmap, brightnessValue, contrastValue);

            }
            else
            {
                contrastValue = value;
                resultBitmap = DrawableToBitmap(ChangeBlackWhite(grayBitmapDefault, contrastValue * 2));
            }
            bitmapPath.Recycle();
            var filepath = ScanUltils.SaveToExternalStorage(resultBitmap);
            return filepath;
        }
        #endregion

        #region Brightness
        public string Brightness(int value, string path)
        {
            Bitmap bitmapPath = null;
            if (path != null)
            {
                bitmapPath = BitmapFactory.DecodeFile(path);
            }
            brightnessValue = (value + 100) / 2;
            if (modeState == State.Color)
                resultBitmap = ImageFilter(bitmapPath, brightnessValue, contrastValue);
            else if (modeState == State.BlackWhite)
                resultBitmap = ImageFilter(blackWhiteBitmap, brightnessValue, contrastValue);
            else if (modeState == State.GreyScale)
                resultBitmap = ImageFilter(grayBitmap, brightnessValue, contrastValue);
            else if (modeState == State.Invert)
                resultBitmap = ImageFilter(inverBitmap, brightnessValue, contrastValue);
            else if (modeState == State.Swap)
                resultBitmap = ImageFilter(swapBitmap, brightnessValue, contrastValue);
            else if (modeState == State.Sepia)
                resultBitmap = ImageFilter(sepiaBitmap, brightnessValue, contrastValue);
            else if (modeState == State.Polaroid)
                resultBitmap = ImageFilter(polaroidBitmap, brightnessValue, contrastValue);
            else if (modeState == State.WhiteToAlpha)
                resultBitmap = ImageFilter(whiteToAlphaBitmap, brightnessValue, contrastValue);
            bitmapPath.Recycle();
            sourcebm = resultBitmap;
            var filepath = ScanUltils.SaveToExternalStorage(resultBitmap);
            return filepath;
        }
        #endregion

        #region WhiteBalance
        public string Whitebalance(int value, string path)
        {
            Bitmap bitmapPath = null;
            if (path != null)
            {
                bitmapPath = BitmapFactory.DecodeFile(path);
            }
            whiteBalanceValue = ((value + 100) * 90) / 2;
            var whiteBalance = whiteBalanceValue + whiteBalanceOffset;

            if (modeState == State.Color)
            {
                resultBitmap = ImageFilter(bitmapPath, brightnessValue, contrastValue);
            }
            else if (modeState == State.BlackWhite)
            {
                var result = ChangeDrawableWhiteBalance(blackWhiteBitmap, whiteBalance);
                resultBitmap = DrawableToBitmap(result);
            }
            else if (modeState == State.GreyScale)
            {
                var result = ChangeDrawableWhiteBalance(grayBitmap, whiteBalance);
                resultBitmap = DrawableToBitmap(result);
            }
            else if (modeState == State.Invert)
            {
                var result = ChangeDrawableWhiteBalance(inverBitmap, whiteBalance);
                resultBitmap = DrawableToBitmap(result);
            }
            else if (modeState == State.Swap)
            {
                var result = ChangeDrawableWhiteBalance(swapBitmap, whiteBalance);
                resultBitmap = DrawableToBitmap(result);
            }
            else if (modeState == State.Sepia)
            {
                var result = ChangeDrawableWhiteBalance(sepiaBitmap, whiteBalance);
                resultBitmap = DrawableToBitmap(result);
            }
            else if (modeState == State.Polaroid)
            {
                var result = ChangeDrawableWhiteBalance(polaroidBitmap, whiteBalance);
                resultBitmap = DrawableToBitmap(result);
            }
            else if (modeState == State.WhiteToAlpha)
            {
                var result = ChangeDrawableWhiteBalance(whiteToAlphaBitmap, whiteBalance);
                resultBitmap = DrawableToBitmap(result);
            }
            bitmapPath.Recycle();
            var filepath = ScanUltils.SaveToExternalStorage(resultBitmap);
            return filepath;
        }
        #endregion

        #region Color
        public void Original(string cropPath)
        {
            Reset();
            if (modeState != State.Color)
            {
                modeState = State.Color;
                state = 0;
            }
        }
        public string BlackWhite(string path)
        {
            if (resultBitmap != null)
            {
                resultBitmap.Recycle();
            }
            Reset();
            Bitmap bitmap = BitmapFactory.DecodeFile(path);
            var result = ChangeBlackWhiteImage(bitmap);
            blackWhiteBitmap = DrawableToBitmap(result);
            resultBitmap = blackWhiteBitmap;
            if (modeState != State.BlackWhite)
            {
                modeState = State.BlackWhite;
                state = 1;
            }
            bitmap.Recycle();
            var filepath = ScanUltils.SaveToExternalStorage(resultBitmap);
            return filepath;
        }
        public string GreyScale(string path)
        {
            if (resultBitmap != null)
            {
                resultBitmap.Recycle();
            }
            Reset();
            Bitmap bitmap = BitmapFactory.DecodeFile(path);
            brightnessValue = 50;
            contrastValue = 50;
            whiteBalanceValue = whiteBalanceMaxProgress / 2;
            var result = ChangeGrayscale(bitmap);
            grayBitmap = DrawableToBitmap(result);
            resultBitmap = grayBitmap;
            if (modeState != State.GreyScale)
            {
                modeState = State.GreyScale;
                state = 2;
            }
            bitmap.Recycle();
            var filePath = ScanUltils.SaveToExternalStorage(grayBitmap);
            return filePath;
        }
        public string Invert(string path)
        {
            if (resultBitmap != null)
            {
                resultBitmap.Recycle();
            }
            Reset();
            Bitmap bitmap = BitmapFactory.DecodeFile(path);
            var result = ChangeInvert(bitmap);
            inverBitmap = DrawableToBitmap(result);
            resultBitmap = inverBitmap;
            if (modeState != State.Invert)
            {
                modeState = State.Invert;
                state = 3;
            }
            bitmap.Recycle();
            var filePath = ScanUltils.SaveToExternalStorage(inverBitmap);
            return filePath;
        }
        public string Swap(string path)
        {
            if (resultBitmap != null)
            {
                resultBitmap.Recycle();
            }
            Reset();
            Bitmap bitmap = BitmapFactory.DecodeFile(path);
            var result = ChangeSwap(bitmap);
            swapBitmap = DrawableToBitmap(result);
            resultBitmap = swapBitmap;
            if (modeState != State.Swap)
            {
                modeState = State.Swap;
                state = 4;
            }
            bitmap.Recycle();
            var filePath = ScanUltils.SaveToExternalStorage(swapBitmap);
            return filePath;
        }
        public string Sepia(string path)
        {
            if (resultBitmap != null)
            {
                resultBitmap.Recycle();
            }
            Reset();
            Bitmap bitmap = BitmapFactory.DecodeFile(path);
            var result = ChangeSepia(bitmap);
            sepiaBitmap = DrawableToBitmap(result);
            resultBitmap = sepiaBitmap;
            if (modeState != State.Sepia)
            {
                modeState = State.Sepia;
                state = 5;
            }
            bitmap.Recycle();
            var filePath = ScanUltils.SaveToExternalStorage(sepiaBitmap);
            return filePath;
        }
        public string Polaroid(string path)
        {
            if (resultBitmap != null)
            {
                resultBitmap.Recycle();
            }
            Reset();
            Bitmap bitmap = BitmapFactory.DecodeFile(path);
            var result = ChangePolaroid(bitmap);
            polaroidBitmap = DrawableToBitmap(result);
            resultBitmap = polaroidBitmap;
            if (modeState != State.Polaroid)
            {
                modeState = State.Polaroid;
                state = 6;
            }
            bitmap.Recycle();
            var filePath = ScanUltils.SaveToExternalStorage(polaroidBitmap);
            return filePath;
        }
        public string Sunset(string path)
        {
            if (resultBitmap != null)
            {
                resultBitmap.Recycle();
            }
            Reset();
            Bitmap bitmap = BitmapFactory.DecodeFile(path);
            var result = ChangeSunset(bitmap);
            polaroidBitmap = DrawableToBitmap(result);
            resultBitmap = polaroidBitmap;
            if (modeState != State.Sunset)
            {
                modeState = State.Sunset;
                state = 7;
            }
            bitmap.Recycle();
            var filePath = ScanUltils.SaveToExternalStorage(polaroidBitmap);
            return filePath;
        }
        #endregion

        #region Rotate

        //public byte[] RotateRight(string path)
        //{
        //    Bitmap bitmapPath = null;
        //    if(path != null)
        //    {
        //        bitmapPath = BitmapFactory.DecodeFile(path);
        //    }
        //    switch (stateRotate)
        //    {
        //        case StateRotate._default:
        //            if (rightRotateBitmap == null)
        //            {
        //                rightRotateBitmap = Rotate(bitmapPath, 90);
        //            }
        //            rotateBitmap = rightRotateBitmap;
        //            stateRotate = StateRotate._right;
        //            Angle = 90;
        //            break;
        //        case StateRotate._right:
        //            if (reverseRotateBitmap == null)
        //            {
        //                reverseRotateBitmap = Rotate(bitmapPath, 180);
        //            }
        //            rotateBitmap = reverseRotateBitmap;
        //            stateRotate = StateRotate._reverse;
        //            Angle = 180;
        //            break;
        //        case StateRotate._reverse:
        //            if (leftRotateBitmap == null)
        //            {
        //                leftRotateBitmap = Rotate(bitmapPath, 270);
        //            }
        //            rotateBitmap = leftRotateBitmap;
        //            stateRotate = StateRotate._left;
        //            Angle = 270;
        //            break;
        //        default:
        //            rotateBitmap = bitmapPath;
        //            stateRotate = StateRotate._default;
        //            Angle = 0;
        //            break;
        //    }
        //    modeState = State.Rotate;
        //    byte[] bitmapData;
        //    using (var stream = new MemoryStream())
        //    {
        //        rotateBitmap.Compress(Bitmap.CompressFormat.Jpeg, 50, stream);
        //        bitmapData = stream.ToArray();
        //    }
        //    return bitmapData;
        //}
        //public byte[] RotateLeft(string path)
        //{
        //    Bitmap bitmapPath = null;
        //    if (path != null)
        //    {
        //        bitmapPath = BitmapFactory.DecodeFile(path);
        //    }
        //    switch (stateRotate)
        //    {
        //        case StateRotate._default:
        //            if (leftRotateBitmap == null)
        //            {
        //                leftRotateBitmap = Rotate(bitmapPath, 270);
        //            }
        //            rotateBitmap = leftRotateBitmap;
        //            stateRotate = StateRotate._left;
        //            Angle = 90;
        //            break;
        //        case StateRotate._left:
        //            if (reverseRotateBitmap == null)
        //            {
        //                reverseRotateBitmap = Rotate(bitmapPath, 180);
        //            }
        //            rotateBitmap = reverseRotateBitmap;
        //            stateRotate = StateRotate._reverse;
        //            Angle = 180;
        //            break;
        //        case StateRotate._reverse:
        //            if (rightRotateBitmap == null)
        //            {
        //                rightRotateBitmap = Rotate(bitmapPath, 90);
        //            }
        //            rotateBitmap = rightRotateBitmap;
        //            stateRotate = StateRotate._right;
        //            Angle = 270;
        //            break;
        //        default:
        //            rotateBitmap = bitmapPath;
        //            stateRotate = StateRotate._default;
        //            Angle = 0;
        //            break;
        //    }
        //    byte[] bitmapData;
        //    using (var stream = new MemoryStream())
        //    {
        //        rotateBitmap.Compress(Bitmap.CompressFormat.Jpeg, 50, stream);
        //        bitmapData = stream.ToArray();
        //    }
        //    return bitmapData;
        //}

        //private Bitmap Rotate(Bitmap bm, int angle)
        //{
        //    return ScanUltils.RotateBitmap(bm, angle);
        //}
        #endregion

        public void ResetService(string path)
        {
            Reset();
        }

        public void Crop(string cropPath)
        {

        }
        public string SavePhoto(string path)
        {
            string imagePath = ScanUltils.LastSaveToExternalStorage(resultBitmap);
            return imagePath;
        }

        public void Crop()
        {

        }
    }
}