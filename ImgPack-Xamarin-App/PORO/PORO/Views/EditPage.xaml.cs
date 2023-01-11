using BitooBitImageEditor;
using PORO.Controls;
using PORO.Enums;
using PORO.Models;
using PORO.Services;
using PORO.Untilities;
using PORO.ViewModels;
using PORO.Views.Popups;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using BitmapStretch = PORO.Controls.BitmapStretch;

namespace PORO.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class EditPage : ContentPage
    {
        #region Properties
        public static EditPage Instance { get; private set; }
        public string Path { get; set; }
        public string CropPath { get; set; }
        IFileService fileService = DependencyService.Get<IFileService>();
        SKBitmap bitmap = new SKBitmap();
        SKBitmap currentBitmap = new SKBitmap();
        SKBitmap originalBitmap = new SKBitmap(), orientedWExif = new SKBitmap(), resizedBitmap;
        SKEncodedOrigin orientation;
        FilterType filterTypeChosen = FilterType.ColorFilter;
        FilterType filterType = FilterType.ColorFilter;
        const float DEFAULT_BRIGHTNESS = 0f, DEFAULT_CONTRAST = 0f, DEFAULT_WHITEBALANCE = 0f;

        float brightness = DEFAULT_BRIGHTNESS, contrast = DEFAULT_CONTRAST, whitebalance = DEFAULT_WHITEBALANCE;
        float grayscale_brightness = DEFAULT_BRIGHTNESS, grayscale_contrast = DEFAULT_CONTRAST;
        float blackwhite_contrast = DEFAULT_CONTRAST;

        public SKColorFilter sKColorFilter { get; set; }
        SKPaint paint = new SKPaint();
        FilterOption currentFilterOption = new FilterOption();

        Dictionary<long, FingerPaintPolyline> inProgressPolylines = new Dictionary<long, FingerPaintPolyline>();
        Dictionary<long, FingerPaintPolyline> inProgressUndoPolylines = new Dictionary<long, FingerPaintPolyline>();
        List<FingerPaintPolyline> completedPolylines = new List<FingerPaintPolyline>();
        List<FingerPaintPolyline> completedUndoPolylines = new List<FingerPaintPolyline>();

        SKColor SKColor = SKColors.Black;
        SKColor SKColor1 = SKColors.Black;
        SKBlendMode SKBlendMode = SKBlendMode.SrcOver;

        SKPaint drawpaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            StrokeCap = SKStrokeCap.Round,
            StrokeJoin = SKStrokeJoin.Round,

        };

        private List<EditImageModel> editImageModels;
        public List<EditImageModel> EditImageModels
        {
            get => editImageModels;
            set { editImageModels = value; }
        }
        private EditImageModel _editImageModel;
        public EditImageModel EditImageModel
        {
            get => _editImageModel;
            set { _editImageModel = value; }
        }
        #endregion

        #region Contructors
        public EditPage()
        {
            InitializeComponent();
            if (Device.RuntimePlatform == Device.iOS)
            {
                Header.HeightRequest = 70;
            }
            else if (Device.RuntimePlatform == Device.Android)
            {
                Header.HeightRequest = 50;
            }

            Instance = this;

            EditImageModels = new List<EditImageModel>();
            EditImageModel = new EditImageModel();

            bitmap = null;
            assembly = GetType().GetTypeInfo().Assembly;
            var display = DeviceDisplay.MainDisplayInfo;

            ContrastValue.Text = "0";
            BrightnessValue.Text = "0";
            WhiteBalanceValue.Text = "0";
            PenSize.Value = 10;
            PenSizeValue.Text = "10";
            drawVisile.IsVisible = true;
            eraserVisile.IsVisible = false;
            undo.Opacity = 0.3;
            undo.IsEnabled = false;
            clear.Opacity = 0.3;
            clear.IsEnabled = false;
            redo.Opacity = 0.3;
            redo.IsEnabled = false;
            color.BackgroundColor = Color.White;

            currentFilterOption = new FilterOption
            {
                ColorContrast = contrast,
                ColorBrightness = brightness,
                ColorWhiteBalance = whitebalance,
                GrayContrast = grayscale_contrast,
                GrayBrightness = grayscale_brightness,
                BlackWhiteContrast = blackwhite_contrast
            };
            paint.ColorFilter = FilterMatrix.GetColorFilterMatrix(currentFilterOption);
        }

        protected override bool OnBackButtonPressed()
        {
            return true;
        }
        #endregion

        #region Save

        public string Done()
        {
            string filePath = EditPageViewModel.Instance.GetPath();
            var drawBitmap = SaveDraw();
            if (bitmap == null && originalBitmap == null)
            {
                using (Stream stream = File.OpenRead(filePath))
                {
                    bitmap = SKBitmap.Decode(stream);
                    switch (filterTypeChosen)
                    {
                        case FilterType.ColorFilter:
                            {
                                paint.ColorFilter = FilterMatrix.GetColorFilterMatrix(currentFilterOption);
                                SKBitmap newBitmap = new SKBitmap(originalBitmap.Width, originalBitmap.Height);
                                using (SKCanvas newCanvas = new SKCanvas(newBitmap))
                                {
                                    newCanvas.Clear();
                                    newCanvas.DrawBitmap(bitmap: originalBitmap, 0, 0, paint: paint);
                                }
                                var dstInfo1 = new SKImageInfo(newBitmap.Width, newBitmap.Height);
                                resizedBitmap = drawBitmap.Resize(dstInfo1, SKBitmapResizeMethod.Hamming);

                                if (newBitmap != null)
                                {
                                    int offset = newBitmap.Width / 2 - resizedBitmap.Width / 2;
                                    int offsetTop = newBitmap.Height / 2 - resizedBitmap.Height / 2;

                                    using (SKCanvas canvas = new SKCanvas(newBitmap))
                                    {
                                        canvas.DrawBitmap(resizedBitmap, SKRect.Create(offset, offsetTop, resizedBitmap.Width, resizedBitmap.Height));
                                    }
                                }

                                Path = fileService.SaveToExternalStorage(newBitmap, filePath);
                            }
                            break;
                        default:
                            SKBitmap newBitmap2 = new SKBitmap(bitmap.Width, bitmap.Height);
                            using (SKCanvas newCanvas = new SKCanvas(newBitmap2))
                            {
                                newCanvas.Clear();
                                newCanvas.DrawBitmap(bitmap: bitmap, 0, 0, paint: paint);
                            }

                            var dstInfo2 = new SKImageInfo(newBitmap2.Width, newBitmap2.Height);
                            resizedBitmap = drawBitmap.Resize(dstInfo2, SKBitmapResizeMethod.Hamming);

                            if (newBitmap2 != null)
                            {
                                int offset = newBitmap2.Width / 2 - resizedBitmap.Width / 2;
                                int offsetTop = newBitmap2.Height / 2 - resizedBitmap.Height / 2;

                                using (SKCanvas canvas = new SKCanvas(newBitmap2))
                                {
                                    canvas.DrawBitmap(resizedBitmap, SKRect.Create(offset, offsetTop, resizedBitmap.Width, resizedBitmap.Height));
                                }
                            }

                            Path = fileService.SaveToExternalStorage(newBitmap2, filePath);
                            break;
                    }

                    return Path;

                }
            }
            else
            {
                switch (filterTypeChosen)
                {
                    case FilterType.ColorFilter:
                        {
                            paint.ColorFilter = FilterMatrix.GetColorFilterMatrix(currentFilterOption);
                            SKBitmap newBitmap = new SKBitmap(originalBitmap.Width, originalBitmap.Height);
                            using (SKCanvas newCanvas = new SKCanvas(newBitmap))
                            {
                                newCanvas.Clear();
                                newCanvas.DrawBitmap(bitmap: originalBitmap, 0, 0, paint: paint);
                            }
                            var dstInfo1 = new SKImageInfo(newBitmap.Width, newBitmap.Height);
                            resizedBitmap = drawBitmap.Resize(dstInfo1, SKBitmapResizeMethod.Hamming);

                            if (newBitmap != null)
                            {
                                int offset = newBitmap.Width / 2 - resizedBitmap.Width / 2;
                                int offsetTop = newBitmap.Height / 2 - resizedBitmap.Height / 2;

                                using (SKCanvas canvas = new SKCanvas(newBitmap))
                                {
                                    canvas.DrawBitmap(resizedBitmap, SKRect.Create(offset, offsetTop, resizedBitmap.Width, resizedBitmap.Height));
                                }
                            }

                            Path = fileService.SaveToExternalStorage(newBitmap, filePath);
                        }
                        break;
                    default:
                        SKBitmap newBitmap2 = new SKBitmap(bitmap.Width, bitmap.Height);
                        using (SKCanvas newCanvas = new SKCanvas(newBitmap2))
                        {
                            newCanvas.Clear();
                            newCanvas.DrawBitmap(bitmap: bitmap, 0, 0, paint: paint);
                        }

                        var dstInfo2 = new SKImageInfo(newBitmap2.Width, newBitmap2.Height);
                        resizedBitmap = drawBitmap.Resize(dstInfo2, SKBitmapResizeMethod.Hamming);

                        if (newBitmap2 != null)
                        {
                            int offset = newBitmap2.Width / 2 - resizedBitmap.Width / 2;
                            int offsetTop = newBitmap2.Height / 2 - resizedBitmap.Height / 2;

                            using (SKCanvas canvas = new SKCanvas(newBitmap2))
                            {
                                canvas.DrawBitmap(resizedBitmap, SKRect.Create(offset, offsetTop, resizedBitmap.Width, resizedBitmap.Height));
                            }
                        }

                        Path = fileService.SaveToExternalStorage(newBitmap2, filePath);
                        break;
                }

                return Path;
            }

        }
        #endregion

        #region GetBitmap
        public SKBitmap GetBitmap()
        {
            string filePath = EditPageViewModel.Instance.GetPath();
            var drawBitmap = SaveDraw();
            if (bitmap == null && originalBitmap == null)
            {
                using (Stream stream = File.OpenRead(filePath))
                {
                    bitmap = SKBitmap.Decode(stream);
                    switch (filterTypeChosen)
                    {
                        case FilterType.ColorFilter:
                            {
                                paint.ColorFilter = FilterMatrix.GetColorFilterMatrix(currentFilterOption);
                                SKBitmap newBitmap = new SKBitmap(originalBitmap.Width, originalBitmap.Height);
                                using (SKCanvas newCanvas = new SKCanvas(newBitmap))
                                {
                                    newCanvas.Clear();
                                    newCanvas.DrawBitmap(bitmap: originalBitmap, 0, 0, paint: paint);
                                }
                                var dstInfo1 = new SKImageInfo(newBitmap.Width, newBitmap.Height);
                                resizedBitmap = drawBitmap.Resize(dstInfo1, SKBitmapResizeMethod.Hamming);

                                if (newBitmap != null)
                                {
                                    int offset = newBitmap.Width / 2 - resizedBitmap.Width / 2;
                                    int offsetTop = newBitmap.Height / 2 - resizedBitmap.Height / 2;

                                    using (SKCanvas canvas = new SKCanvas(newBitmap))
                                    {
                                        canvas.DrawBitmap(resizedBitmap, SKRect.Create(offset, offsetTop, resizedBitmap.Width, resizedBitmap.Height));
                                    }
                                }

                                drawBitmap = newBitmap;
                            }
                            break;
                        default:
                            SKBitmap newBitmap2 = new SKBitmap(bitmap.Width, bitmap.Height);
                            using (SKCanvas newCanvas = new SKCanvas(newBitmap2))
                            {
                                newCanvas.Clear();
                                newCanvas.DrawBitmap(bitmap: bitmap, 0, 0, paint: paint);
                            }

                            var dstInfo2 = new SKImageInfo(newBitmap2.Width, newBitmap2.Height);
                            resizedBitmap = drawBitmap.Resize(dstInfo2, SKBitmapResizeMethod.Hamming);

                            if (newBitmap2 != null)
                            {
                                int offset = newBitmap2.Width / 2 - resizedBitmap.Width / 2;
                                int offsetTop = newBitmap2.Height / 2 - resizedBitmap.Height / 2;

                                using (SKCanvas canvas = new SKCanvas(newBitmap2))
                                {
                                    canvas.DrawBitmap(resizedBitmap, SKRect.Create(offset, offsetTop, resizedBitmap.Width, resizedBitmap.Height));
                                }
                            }

                            drawBitmap = newBitmap2;
                            break;
                    }

                    return drawBitmap;

                }
            }
            else
            {
                switch (filterTypeChosen)
                {
                    case FilterType.ColorFilter:
                        {
                            paint.ColorFilter = FilterMatrix.GetColorFilterMatrix(currentFilterOption);
                            SKBitmap newBitmap = new SKBitmap(originalBitmap.Width, originalBitmap.Height);
                            using (SKCanvas newCanvas = new SKCanvas(newBitmap))
                            {
                                newCanvas.Clear();
                                newCanvas.DrawBitmap(bitmap: originalBitmap, 0, 0, paint: paint);
                            }
                            var dstInfo1 = new SKImageInfo(newBitmap.Width, newBitmap.Height);
                            resizedBitmap = drawBitmap.Resize(dstInfo1, SKBitmapResizeMethod.Hamming);

                            if (newBitmap != null)
                            {
                                int offset = newBitmap.Width / 2 - resizedBitmap.Width / 2;
                                int offsetTop = newBitmap.Height / 2 - resizedBitmap.Height / 2;

                                using (SKCanvas canvas = new SKCanvas(newBitmap))
                                {
                                    canvas.DrawBitmap(resizedBitmap, SKRect.Create(offset, offsetTop, resizedBitmap.Width, resizedBitmap.Height));
                                }
                            }

                            drawBitmap = newBitmap;
                        }
                        break;
                    default:
                        SKBitmap newBitmap2 = new SKBitmap(bitmap.Width, bitmap.Height);
                        using (SKCanvas newCanvas = new SKCanvas(newBitmap2))
                        {
                            newCanvas.Clear();
                            newCanvas.DrawBitmap(bitmap: bitmap, 0, 0, paint: paint);
                        }

                        var dstInfo2 = new SKImageInfo(newBitmap2.Width, newBitmap2.Height);
                        resizedBitmap = drawBitmap.Resize(dstInfo2, SKBitmapResizeMethod.Hamming);

                        if (newBitmap2 != null)
                        {
                            int offset = newBitmap2.Width / 2 - resizedBitmap.Width / 2;
                            int offsetTop = newBitmap2.Height / 2 - resizedBitmap.Height / 2;

                            using (SKCanvas canvas = new SKCanvas(newBitmap2))
                            {
                                canvas.DrawBitmap(resizedBitmap, SKRect.Create(offset, offsetTop, resizedBitmap.Width, resizedBitmap.Height));
                            }
                        }

                        drawBitmap = newBitmap2;
                        break;
                }

                return drawBitmap;
            }
        }

        public SKBitmap SaveDraw()
        {

            SKBitmap newBitmap = new SKBitmap((int)drawView.CanvasSize.Width, (int)drawView.CanvasSize.Height);
            using (SKCanvas newCanvas = new SKCanvas(newBitmap))
            {
                newCanvas.Clear();
                foreach (FingerPaintPolyline polyline in completedPolylines)
                {
                    drawpaint.Color = polyline.StrokeColor;
                    drawpaint.BlendMode = polyline.BlendMode;
                    drawpaint.StrokeWidth = polyline.StrokeWidth;
                    newCanvas.DrawPath(polyline.Path, drawpaint);
                }

                foreach (FingerPaintPolyline polyline in inProgressPolylines.Values)
                {
                    drawpaint.Color = polyline.StrokeColor;
                    drawpaint.BlendMode = polyline.BlendMode;
                    drawpaint.StrokeWidth = polyline.StrokeWidth;
                    newCanvas.DrawPath(polyline.Path, drawpaint);
                }
            }
            return newBitmap;
        }
        #endregion

        #region Canvas
        private async void redBrowClick(object sender, EventArgs e)
        {
            filterTypeChosen = FilterType.RedBrow;
            filterType = FilterType.RedBrow;

            #region Border
            redBrowBorder.Thickness = 2;
            blackWhiteBorder.Thickness = 0;
            OriginalBorder.Thickness = 0;
            grayScaleBorder.Thickness = 0;
            invertBorder.Thickness = 0;
            swapBorder.Thickness = 0;
            sepiaBorder.Thickness = 0;
            polaroidBorder.Thickness = 0;
            sunsetBorder.Thickness = 0;
            grayYellowBorder.Thickness = 0;
            redBlueBorder.Thickness = 0;
            skyBlueBorder.Thickness = 0;
            blueBorder.Thickness = 0;
            greenVioletBorder.Thickness = 0;
            classicBorder.Thickness = 0;
            yellowClassicBorder.Thickness = 0;
            blueClassicBorder.Thickness = 0;
            blueClassicBorder1.Thickness = 0;
            blueClassicBorder2.Thickness = 0;
            yellowClassicBorder1.Thickness = 0;
            yellowClassicBorder2.Thickness = 0;
            grayYellowBorder1.Thickness = 0;
            redBlueBorder1.Thickness = 0;
            blueBorder1.Thickness = 0;
            #endregion

            var currentFilterOption = new FilterOption
            {
                ColorContrast = contrast,
                ColorBrightness = brightness,
                ColorWhiteBalance = whitebalance,
                GrayContrast = grayscale_contrast,
                GrayBrightness = grayscale_brightness,
                BlackWhiteContrast = blackwhite_contrast
            };


            paint.ColorFilter = FilterMatrix.GetRedBrowFilterMatrix(currentFilterOption);
            canvasView.InvalidateSurface();

            //orientation = CheckOrigintation(Path);
            //orientedWExif = HandleOrientation(bitmap, orientation);
            SKBitmap newBitmap = new SKBitmap(bitmap.Width, bitmap.Height);
            using (SKCanvas newCanvas = new SKCanvas(newBitmap))
            {
                newCanvas.Clear();
                newCanvas.DrawBitmap(bitmap: bitmap, 0, 0, paint: paint);
            }
            originalBitmap = newBitmap;
        }

        private async void greenvioletClick(object sender, EventArgs e)
        {
            filterTypeChosen = FilterType.BlueViolet;
            filterType = FilterType.BlueViolet;

            #region Border
            blackWhiteBorder.Thickness = 0;
            OriginalBorder.Thickness = 0;
            grayScaleBorder.Thickness = 0;
            invertBorder.Thickness = 0;
            swapBorder.Thickness = 0;
            sepiaBorder.Thickness = 0;
            polaroidBorder.Thickness = 0;
            sunsetBorder.Thickness = 0;
            grayYellowBorder.Thickness = 0;
            redBlueBorder.Thickness = 0;
            skyBlueBorder.Thickness = 0;
            blueBorder.Thickness = 0;
            greenVioletBorder.Thickness = 2;
            redBrowBorder.Thickness = 0;
            classicBorder.Thickness = 0;
            yellowClassicBorder.Thickness = 0;
            blueClassicBorder.Thickness = 0;
            blueClassicBorder1.Thickness = 0;
            blueClassicBorder2.Thickness = 0;
            yellowClassicBorder1.Thickness = 0;
            yellowClassicBorder2.Thickness = 0;
            grayYellowBorder1.Thickness = 0;
            redBlueBorder1.Thickness = 0;
            blueBorder1.Thickness = 0;
            #endregion

            var currentFilterOption = new FilterOption
            {
                ColorContrast = contrast,
                ColorBrightness = brightness,
                ColorWhiteBalance = whitebalance,
                GrayContrast = grayscale_contrast,
                GrayBrightness = grayscale_brightness,
                BlackWhiteContrast = blackwhite_contrast
            };


            paint.ColorFilter = FilterMatrix.GetBlueVioletFilterMatrix(currentFilterOption);
            canvasView.InvalidateSurface();

            //orientation = CheckOrigintation(Path);
            //orientedWExif = HandleOrientation(bitmap, orientation);
            SKBitmap newBitmap = new SKBitmap(bitmap.Width, bitmap.Height);
            using (SKCanvas newCanvas = new SKCanvas(newBitmap))
            {
                newCanvas.Clear();
                newCanvas.DrawBitmap(bitmap: bitmap, 0, 0, paint: paint);
            }
            originalBitmap = newBitmap;
        }

        private async void originalClick(object sender, EventArgs e)
        {
            filterTypeChosen = FilterType.ColorFilter;
            filterType = FilterType.ColorFilter;

            #region Border
            blackWhiteBorder.Thickness = 0;
            OriginalBorder.Thickness = 2;
            grayScaleBorder.Thickness = 0;
            invertBorder.Thickness = 0;
            swapBorder.Thickness = 0;
            sepiaBorder.Thickness = 0;
            polaroidBorder.Thickness = 0;
            sunsetBorder.Thickness = 0;
            grayYellowBorder.Thickness = 0;
            redBlueBorder.Thickness = 0;
            skyBlueBorder.Thickness = 0;
            blueBorder.Thickness = 0;
            greenVioletBorder.Thickness = 0;
            redBrowBorder.Thickness = 0;
            classicBorder.Thickness = 0;
            yellowClassicBorder.Thickness = 0;
            blueClassicBorder.Thickness = 0;
            blueClassicBorder1.Thickness = 0;
            blueClassicBorder2.Thickness = 0;
            yellowClassicBorder1.Thickness = 0;
            yellowClassicBorder2.Thickness = 0;
            grayYellowBorder1.Thickness = 0;
            redBlueBorder1.Thickness = 0;
            blueBorder1.Thickness = 0;
            #endregion

            var currentFilterOption = new FilterOption
            {
                ColorContrast = contrast,
                ColorBrightness = brightness,
                ColorWhiteBalance = whitebalance,
                GrayContrast = grayscale_contrast,
                GrayBrightness = grayscale_brightness,
                BlackWhiteContrast = blackwhite_contrast
            };


            paint.ColorFilter = FilterMatrix.GetColorFilterMatrix(currentFilterOption);
            canvasView.InvalidateSurface();

            //orientation = CheckOrigintation(Path);
            //orientedWExif = HandleOrientation(bitmap, orientation);
            SKBitmap newBitmap = new SKBitmap(bitmap.Width, bitmap.Height);
            using (SKCanvas newCanvas = new SKCanvas(newBitmap))
            {
                newCanvas.Clear();
                newCanvas.DrawBitmap(bitmap: bitmap, 0, 0, paint: paint);
            }
            originalBitmap = newBitmap;
        }

        private async void blackwhiteClick(object sender, EventArgs e)
        {
            filterTypeChosen = FilterType.BlackAndWhiteFilter;
            filterType = FilterType.BlackAndWhiteFilter;

            var currentFilterOption = new FilterOption
            {
                ColorContrast = contrast,
                ColorBrightness = brightness,
                ColorWhiteBalance = whitebalance,
                GrayContrast = grayscale_contrast,
                GrayBrightness = grayscale_brightness,
                BlackWhiteContrast = blackwhite_contrast
            };


            paint.ColorFilter = FilterMatrix.GetBlackAndWhiteFilterMatrix(currentFilterOption);
            canvasView.InvalidateSurface();

            //orientation = CheckOrigintation(Path);
            //orientedWExif = HandleOrientation(bitmap, orientation);
            SKBitmap newBitmap = new SKBitmap(bitmap.Width, bitmap.Height);
            using (SKCanvas newCanvas = new SKCanvas(newBitmap))
            {
                newCanvas.Clear();
                newCanvas.DrawBitmap(bitmap: bitmap, 0, 0, paint: paint);
            }
            originalBitmap = newBitmap;

            #region Border
            blackWhiteBorder.Thickness = 2;
            OriginalBorder.Thickness = 0;
            grayScaleBorder.Thickness = 0;
            invertBorder.Thickness = 0;
            swapBorder.Thickness = 0;
            sepiaBorder.Thickness = 0;
            polaroidBorder.Thickness = 0;
            sunsetBorder.Thickness = 0;
            grayYellowBorder.Thickness = 0;
            redBlueBorder.Thickness = 0;
            skyBlueBorder.Thickness = 0;
            blueBorder.Thickness = 0;
            greenVioletBorder.Thickness = 0;
            redBrowBorder.Thickness = 0;
            classicBorder.Thickness = 0;
            yellowClassicBorder.Thickness = 0;
            blueClassicBorder.Thickness = 0;
            blueClassicBorder1.Thickness = 0;
            blueClassicBorder2.Thickness = 0;
            yellowClassicBorder1.Thickness = 0;
            yellowClassicBorder2.Thickness = 0;
            grayYellowBorder1.Thickness = 0;
            redBlueBorder1.Thickness = 0;
            blueBorder1.Thickness = 0;
            #endregion
        }

        private async void grayscaleClick(object sender, EventArgs e)
        {
            filterTypeChosen = FilterType.GrayScaleFilter;
            filterType = FilterType.GrayScaleFilter;

            var currentFilterOption = new FilterOption
            {
                ColorContrast = contrast,
                ColorBrightness = brightness,
                ColorWhiteBalance = whitebalance,
                GrayContrast = grayscale_contrast,
                GrayBrightness = grayscale_brightness,
                BlackWhiteContrast = blackwhite_contrast
            };


            paint.ColorFilter = FilterMatrix.GetGrayScaleFilterMatrix(currentFilterOption);
            canvasView.InvalidateSurface();

            //orientation = CheckOrigintation(Path);
            //orientedWExif = HandleOrientation(bitmap, orientation);
            SKBitmap newBitmap = new SKBitmap(bitmap.Width, bitmap.Height);
            using (SKCanvas newCanvas = new SKCanvas(newBitmap))
            {
                newCanvas.Clear();
                newCanvas.DrawBitmap(bitmap: bitmap, 0, 0, paint: paint);
            }
            originalBitmap = newBitmap;

            #region Border
            blackWhiteBorder.Thickness = 0;
            OriginalBorder.Thickness = 0;
            grayScaleBorder.Thickness = 2;
            invertBorder.Thickness = 0;
            swapBorder.Thickness = 0;
            sepiaBorder.Thickness = 0;
            polaroidBorder.Thickness = 0;
            sunsetBorder.Thickness = 0;
            grayYellowBorder.Thickness = 0;
            redBlueBorder.Thickness = 0;
            skyBlueBorder.Thickness = 0;
            blueBorder.Thickness = 0;
            greenVioletBorder.Thickness = 0;
            redBrowBorder.Thickness = 0;
            classicBorder.Thickness = 0;
            yellowClassicBorder.Thickness = 0;
            blueClassicBorder.Thickness = 0;
            blueClassicBorder1.Thickness = 0;
            blueClassicBorder2.Thickness = 0;
            yellowClassicBorder1.Thickness = 0;
            yellowClassicBorder2.Thickness = 0;
            grayYellowBorder1.Thickness = 0;
            redBlueBorder1.Thickness = 0;
            blueBorder1.Thickness = 0;
            #endregion
        }

        private async void invertClick(object sender, EventArgs e)
        {
            filterTypeChosen = FilterType.InvertFilter;
            filterType = FilterType.InvertFilter;

            var currentFilterOption = new FilterOption
            {
                ColorContrast = contrast,
                ColorBrightness = brightness,
                ColorWhiteBalance = whitebalance,
                GrayContrast = grayscale_contrast,
                GrayBrightness = grayscale_brightness,
                BlackWhiteContrast = blackwhite_contrast
            };


            paint.ColorFilter = FilterMatrix.GetInvertFilterMatrix(currentFilterOption);
            canvasView.InvalidateSurface();

            //orientation = CheckOrigintation(Path);
            //orientedWExif = HandleOrientation(bitmap, orientation);
            SKBitmap newBitmap = new SKBitmap(bitmap.Width, bitmap.Height);
            using (SKCanvas newCanvas = new SKCanvas(newBitmap))
            {
                newCanvas.Clear();
                newCanvas.DrawBitmap(bitmap: bitmap, 0, 0, paint: paint);
            }
            originalBitmap = newBitmap;

            #region Border
            blackWhiteBorder.Thickness = 0;
            OriginalBorder.Thickness = 0;
            grayScaleBorder.Thickness = 0;
            invertBorder.Thickness = 2;
            swapBorder.Thickness = 0;
            sepiaBorder.Thickness = 0;
            polaroidBorder.Thickness = 0;
            sunsetBorder.Thickness = 0;
            grayYellowBorder.Thickness = 0;
            redBlueBorder.Thickness = 0;
            skyBlueBorder.Thickness = 0;
            blueBorder.Thickness = 0;
            greenVioletBorder.Thickness = 0;
            redBrowBorder.Thickness = 0;
            classicBorder.Thickness = 0;
            yellowClassicBorder.Thickness = 0;
            blueClassicBorder.Thickness = 0;
            blueClassicBorder1.Thickness = 0;
            blueClassicBorder2.Thickness = 0;
            yellowClassicBorder1.Thickness = 0;
            yellowClassicBorder2.Thickness = 0;
            grayYellowBorder1.Thickness = 0;
            redBlueBorder1.Thickness = 0;
            blueBorder1.Thickness = 0;
            #endregion
        }

        private async void swapClick(object sender, EventArgs e)
        {
            filterTypeChosen = FilterType.SwapFilter;
            filterType = FilterType.SwapFilter;

            var currentFilterOption = new FilterOption
            {
                ColorContrast = contrast,
                ColorBrightness = brightness,
                ColorWhiteBalance = whitebalance,
                GrayContrast = grayscale_contrast,
                GrayBrightness = grayscale_brightness,
                BlackWhiteContrast = blackwhite_contrast
            };


            paint.ColorFilter = FilterMatrix.GetSwapFilterMatrix(currentFilterOption);
            canvasView.InvalidateSurface();

            //orientation = CheckOrigintation(Path);
            //orientedWExif = HandleOrientation(bitmap, orientation);
            SKBitmap newBitmap = new SKBitmap(bitmap.Width, bitmap.Height);
            using (SKCanvas newCanvas = new SKCanvas(newBitmap))
            {
                newCanvas.Clear();
                newCanvas.DrawBitmap(bitmap: bitmap, 0, 0, paint: paint);
            }
            originalBitmap = newBitmap;

            #region Border
            blackWhiteBorder.Thickness = 0;
            OriginalBorder.Thickness = 0;
            grayScaleBorder.Thickness = 0;
            invertBorder.Thickness = 0;
            swapBorder.Thickness = 2;
            sepiaBorder.Thickness = 0;
            polaroidBorder.Thickness = 0;
            sunsetBorder.Thickness = 0;
            grayYellowBorder.Thickness = 0;
            redBlueBorder.Thickness = 0;
            skyBlueBorder.Thickness = 0;
            blueBorder.Thickness = 0;
            greenVioletBorder.Thickness = 0;
            redBrowBorder.Thickness = 0;
            classicBorder.Thickness = 0;
            yellowClassicBorder.Thickness = 0;
            blueClassicBorder.Thickness = 0;
            blueClassicBorder1.Thickness = 0;
            blueClassicBorder2.Thickness = 0;
            yellowClassicBorder1.Thickness = 0;
            yellowClassicBorder2.Thickness = 0;
            grayYellowBorder1.Thickness = 0;
            redBlueBorder1.Thickness = 0;
            blueBorder1.Thickness = 0;
            #endregion
        }

        private async void sepiaClick(object sender, EventArgs e)
        {
            filterTypeChosen = FilterType.SepiaFilter;
            filterType = FilterType.SepiaFilter;

            var currentFilterOption = new FilterOption
            {
                ColorContrast = contrast,
                ColorBrightness = brightness,
                ColorWhiteBalance = whitebalance,
                GrayContrast = grayscale_contrast,
                GrayBrightness = grayscale_brightness,
                BlackWhiteContrast = blackwhite_contrast
            };


            paint.ColorFilter = FilterMatrix.GetSepiaFilterMatrix(currentFilterOption);
            canvasView.InvalidateSurface();

            //orientation = CheckOrigintation(Path);
            //orientedWExif = HandleOrientation(bitmap, orientation);
            SKBitmap newBitmap = new SKBitmap(bitmap.Width, bitmap.Height);
            using (SKCanvas newCanvas = new SKCanvas(newBitmap))
            {
                newCanvas.Clear();
                newCanvas.DrawBitmap(bitmap: bitmap, 0, 0, paint: paint);
            }
            originalBitmap = newBitmap;

            #region Border
            blackWhiteBorder.Thickness = 0;
            OriginalBorder.Thickness = 0;
            grayScaleBorder.Thickness = 0;
            invertBorder.Thickness = 0;
            swapBorder.Thickness = 0;
            sepiaBorder.Thickness = 2;
            polaroidBorder.Thickness = 0;
            sunsetBorder.Thickness = 0;
            grayYellowBorder.Thickness = 0;
            redBlueBorder.Thickness = 0;
            skyBlueBorder.Thickness = 0;
            blueBorder.Thickness = 0;
            greenVioletBorder.Thickness = 0;
            redBrowBorder.Thickness = 0;
            classicBorder.Thickness = 0;
            yellowClassicBorder.Thickness = 0;
            blueClassicBorder.Thickness = 0;
            blueClassicBorder1.Thickness = 0;
            blueClassicBorder2.Thickness = 0;
            yellowClassicBorder1.Thickness = 0;
            yellowClassicBorder2.Thickness = 0;
            grayYellowBorder1.Thickness = 0;
            redBlueBorder1.Thickness = 0;
            blueBorder1.Thickness = 0;
            #endregion
        }

        private async void polaroidClick(object sender, EventArgs e)
        {
            filterTypeChosen = FilterType.PolaroidFilter;
            filterType = FilterType.PolaroidFilter;

            var currentFilterOption = new FilterOption
            {
                ColorContrast = contrast,
                ColorBrightness = brightness,
                ColorWhiteBalance = whitebalance,
                GrayContrast = grayscale_contrast,
                GrayBrightness = grayscale_brightness,
                BlackWhiteContrast = blackwhite_contrast
            };


            paint.ColorFilter = FilterMatrix.GetPolaroidFilterMatrix(currentFilterOption);
            canvasView.InvalidateSurface();

            //orientation = CheckOrigintation(Path);
            //orientedWExif = HandleOrientation(bitmap, orientation);
            SKBitmap newBitmap = new SKBitmap(bitmap.Width, bitmap.Height);
            using (SKCanvas newCanvas = new SKCanvas(newBitmap))
            {
                newCanvas.Clear();
                newCanvas.DrawBitmap(bitmap: bitmap, 0, 0, paint: paint);
            }
            originalBitmap = newBitmap;

            #region Border
            blackWhiteBorder.Thickness = 0;
            OriginalBorder.Thickness = 0;
            grayScaleBorder.Thickness = 0;
            invertBorder.Thickness = 0;
            swapBorder.Thickness = 0;
            sepiaBorder.Thickness = 0;
            polaroidBorder.Thickness = 2;
            sunsetBorder.Thickness = 0;
            grayYellowBorder.Thickness = 0;
            redBlueBorder.Thickness = 0;
            skyBlueBorder.Thickness = 0;
            blueBorder.Thickness = 0;
            greenVioletBorder.Thickness = 0;
            redBrowBorder.Thickness = 0;
            classicBorder.Thickness = 0;
            yellowClassicBorder.Thickness = 0;
            blueClassicBorder.Thickness = 0;
            blueClassicBorder1.Thickness = 0;
            blueClassicBorder2.Thickness = 0;
            yellowClassicBorder1.Thickness = 0;
            yellowClassicBorder2.Thickness = 0;
            grayYellowBorder1.Thickness = 0;
            redBlueBorder1.Thickness = 0;
            blueBorder1.Thickness = 0;
            #endregion
        }

        private async void sunsetClick(object sender, EventArgs e)
        {
            filterTypeChosen = FilterType.Sunset;
            filterType = FilterType.Sunset;

            var currentFilterOption = new FilterOption
            {
                ColorContrast = contrast,
                ColorBrightness = brightness,
                ColorWhiteBalance = whitebalance,
                GrayContrast = grayscale_contrast,
                GrayBrightness = grayscale_brightness,
                BlackWhiteContrast = blackwhite_contrast
            };


            paint.ColorFilter = FilterMatrix.GetSunsetFilterMatrix(currentFilterOption);
            canvasView.InvalidateSurface();

            //orientation = CheckOrigintation(Path);
            //orientedWExif = HandleOrientation(bitmap, orientation);
            SKBitmap newBitmap = new SKBitmap(bitmap.Width, bitmap.Height);
            using (SKCanvas newCanvas = new SKCanvas(newBitmap))
            {
                newCanvas.Clear();
                newCanvas.DrawBitmap(bitmap: bitmap, 0, 0, paint: paint);
            }
            originalBitmap = newBitmap;

            #region Border
            blackWhiteBorder.Thickness = 0;
            OriginalBorder.Thickness = 0;
            grayScaleBorder.Thickness = 0;
            invertBorder.Thickness = 0;
            swapBorder.Thickness = 0;
            sepiaBorder.Thickness = 0;
            polaroidBorder.Thickness = 0;
            sunsetBorder.Thickness = 2;
            grayYellowBorder.Thickness = 0;
            redBlueBorder.Thickness = 0;
            skyBlueBorder.Thickness = 0;
            blueBorder.Thickness = 0;
            greenVioletBorder.Thickness = 0;
            redBrowBorder.Thickness = 0;
            classicBorder.Thickness = 0;
            yellowClassicBorder.Thickness = 0;
            blueClassicBorder.Thickness = 0;
            blueClassicBorder1.Thickness = 0;
            blueClassicBorder2.Thickness = 0;
            yellowClassicBorder1.Thickness = 0;
            yellowClassicBorder2.Thickness = 0;
            grayYellowBorder1.Thickness = 0;
            redBlueBorder1.Thickness = 0;
            blueBorder1.Thickness = 0;
            #endregion
        }

        private async void grayyellowClick(object sender, EventArgs e)
        {
            filterTypeChosen = FilterType.GrayYellow;
            filterType = FilterType.GrayYellow;

            var currentFilterOption = new FilterOption
            {
                ColorContrast = contrast,
                ColorBrightness = brightness,
                ColorWhiteBalance = whitebalance,
                GrayContrast = grayscale_contrast,
                GrayBrightness = grayscale_brightness,
                BlackWhiteContrast = blackwhite_contrast
            };


            paint.ColorFilter = FilterMatrix.GetGrayYellowFilterMatrix(currentFilterOption);
            canvasView.InvalidateSurface();

            //orientation = CheckOrigintation(Path);
            //orientedWExif = HandleOrientation(bitmap, orientation);
            SKBitmap newBitmap = new SKBitmap(bitmap.Width, bitmap.Height);
            using (SKCanvas newCanvas = new SKCanvas(newBitmap))
            {
                newCanvas.Clear();
                newCanvas.DrawBitmap(bitmap: bitmap, 0, 0, paint: paint);
            }
            originalBitmap = newBitmap;

            #region Border
            blackWhiteBorder.Thickness = 0;
            OriginalBorder.Thickness = 0;
            grayScaleBorder.Thickness = 0;
            invertBorder.Thickness = 0;
            swapBorder.Thickness = 0;
            sepiaBorder.Thickness = 0;
            polaroidBorder.Thickness = 0;
            sunsetBorder.Thickness = 0;
            grayYellowBorder.Thickness = 2;
            redBlueBorder.Thickness = 0;
            skyBlueBorder.Thickness = 0;
            blueBorder.Thickness = 0;
            greenVioletBorder.Thickness = 0;
            redBrowBorder.Thickness = 0;
            classicBorder.Thickness = 0;
            yellowClassicBorder.Thickness = 0;
            blueClassicBorder.Thickness = 0;
            blueClassicBorder1.Thickness = 0;
            blueClassicBorder2.Thickness = 0;
            yellowClassicBorder1.Thickness = 0;
            yellowClassicBorder2.Thickness = 0;
            grayYellowBorder1.Thickness = 0;
            redBlueBorder1.Thickness = 0;
            blueBorder1.Thickness = 0;
            #endregion
        }

        private async void redblueClick(object sender, EventArgs e)
        {
            filterTypeChosen = FilterType.RedBlue;
            filterType = FilterType.RedBlue;

            var currentFilterOption = new FilterOption
            {
                ColorContrast = contrast,
                ColorBrightness = brightness,
                ColorWhiteBalance = whitebalance,
                GrayContrast = grayscale_contrast,
                GrayBrightness = grayscale_brightness,
                BlackWhiteContrast = blackwhite_contrast
            };


            paint.ColorFilter = FilterMatrix.GetRedBlueFilterMatrix(currentFilterOption);
            canvasView.InvalidateSurface();

            //orientation = CheckOrigintation(Path);
            //orientedWExif = HandleOrientation(bitmap, orientation);
            SKBitmap newBitmap = new SKBitmap(bitmap.Width, bitmap.Height);
            using (SKCanvas newCanvas = new SKCanvas(newBitmap))
            {
                newCanvas.Clear();
                newCanvas.DrawBitmap(bitmap: bitmap, 0, 0, paint: paint);
            }
            originalBitmap = newBitmap;

            #region Border
            redBlueBorder.Thickness = 2;
            blackWhiteBorder.Thickness = 0;
            OriginalBorder.Thickness = 0;
            grayScaleBorder.Thickness = 0;
            invertBorder.Thickness = 0;
            swapBorder.Thickness = 0;
            sepiaBorder.Thickness = 0;
            polaroidBorder.Thickness = 0;
            sunsetBorder.Thickness = 0;
            grayYellowBorder.Thickness = 0;
            redBrowBorder.Thickness = 0;
            skyBlueBorder.Thickness = 0;
            blueBorder.Thickness = 0;
            greenVioletBorder.Thickness = 0;
            classicBorder.Thickness = 0;
            yellowClassicBorder.Thickness = 0;
            blueClassicBorder.Thickness = 0;
            blueClassicBorder1.Thickness = 0;
            blueClassicBorder2.Thickness = 0;
            yellowClassicBorder1.Thickness = 0;
            yellowClassicBorder2.Thickness = 0;
            grayYellowBorder1.Thickness = 0;
            redBlueBorder1.Thickness = 0;
            blueBorder1.Thickness = 0;
            #endregion
        }

        private async void skyblueClick(object sender, EventArgs e)
        {
            filterTypeChosen = FilterType.SkyBlue;
            filterType = FilterType.SkyBlue;

            var currentFilterOption = new FilterOption
            {
                ColorContrast = contrast,
                ColorBrightness = brightness,
                ColorWhiteBalance = whitebalance,
                GrayContrast = grayscale_contrast,
                GrayBrightness = grayscale_brightness,
                BlackWhiteContrast = blackwhite_contrast
            };


            paint.ColorFilter = FilterMatrix.GetSkyBlueFilterMatrix(currentFilterOption);
            canvasView.InvalidateSurface();

            //orientation = CheckOrigintation(Path);
            //orientedWExif = HandleOrientation(bitmap, orientation);
            SKBitmap newBitmap = new SKBitmap(bitmap.Width, bitmap.Height);
            using (SKCanvas newCanvas = new SKCanvas(newBitmap))
            {
                newCanvas.Clear();
                newCanvas.DrawBitmap(bitmap: bitmap, 0, 0, paint: paint);
            }
            originalBitmap = newBitmap;

            #region Border
            blackWhiteBorder.Thickness = 0;
            OriginalBorder.Thickness = 0;
            grayScaleBorder.Thickness = 0;
            invertBorder.Thickness = 0;
            swapBorder.Thickness = 0;
            sepiaBorder.Thickness = 0;
            polaroidBorder.Thickness = 0;
            sunsetBorder.Thickness = 0;
            grayYellowBorder.Thickness = 0;
            redBlueBorder.Thickness = 0;
            skyBlueBorder.Thickness = 2;
            blueBorder.Thickness = 0;
            greenVioletBorder.Thickness = 0;
            redBrowBorder.Thickness = 0;
            classicBorder.Thickness = 0;
            yellowClassicBorder.Thickness = 0;
            blueClassicBorder.Thickness = 0;
            blueClassicBorder1.Thickness = 0;
            blueClassicBorder2.Thickness = 0;
            yellowClassicBorder1.Thickness = 0;
            yellowClassicBorder2.Thickness = 0;
            grayYellowBorder1.Thickness = 0;
            redBlueBorder1.Thickness = 0;
            blueBorder1.Thickness = 0;
            #endregion
        }

        private async void blueClick(object sender, EventArgs e)
        {
            filterTypeChosen = FilterType.Blue;
            filterType = FilterType.Blue;

            var currentFilterOption = new FilterOption
            {
                ColorContrast = contrast,
                ColorBrightness = brightness,
                ColorWhiteBalance = whitebalance,
                GrayContrast = grayscale_contrast,
                GrayBrightness = grayscale_brightness,
                BlackWhiteContrast = blackwhite_contrast
            };


            paint.ColorFilter = FilterMatrix.GetBlueFilterMatrix(currentFilterOption);
            canvasView.InvalidateSurface();

            //orientation = CheckOrigintation(Path);
            //orientedWExif = HandleOrientation(bitmap, orientation);
            SKBitmap newBitmap = new SKBitmap(bitmap.Width, bitmap.Height);
            using (SKCanvas newCanvas = new SKCanvas(newBitmap))
            {
                newCanvas.Clear();
                newCanvas.DrawBitmap(bitmap: bitmap, 0, 0, paint: paint);
            }
            originalBitmap = newBitmap;

            #region Border
            blackWhiteBorder.Thickness = 0;
            OriginalBorder.Thickness = 0;
            grayScaleBorder.Thickness = 0;
            invertBorder.Thickness = 0;
            swapBorder.Thickness = 0;
            sepiaBorder.Thickness = 0;
            polaroidBorder.Thickness = 0;
            sunsetBorder.Thickness = 0;
            grayYellowBorder.Thickness = 0;
            redBlueBorder.Thickness = 0;
            skyBlueBorder.Thickness = 0;
            blueBorder.Thickness = 2;
            greenVioletBorder.Thickness = 0;
            redBrowBorder.Thickness = 0;
            classicBorder.Thickness = 0;
            yellowClassicBorder.Thickness = 0;
            blueClassicBorder.Thickness = 0;
            blueClassicBorder1.Thickness = 0;
            blueClassicBorder2.Thickness = 0;
            yellowClassicBorder1.Thickness = 0;
            yellowClassicBorder2.Thickness = 0;
            grayYellowBorder1.Thickness = 0;
            redBlueBorder1.Thickness = 0;
            blueBorder1.Thickness = 0;
            #endregion
        }

        private void classicClick(object sender, EventArgs e)
        {
            filterTypeChosen = FilterType.Classic;
            filterType = FilterType.Classic;

            var currentFilterOption = new FilterOption
            {
                ColorContrast = contrast,
                ColorBrightness = brightness,
                ColorWhiteBalance = whitebalance,
                GrayContrast = grayscale_contrast,
                GrayBrightness = grayscale_brightness,
                BlackWhiteContrast = blackwhite_contrast
            };


            paint.ColorFilter = FilterMatrix.GetClassicFilterMatrix(currentFilterOption);
            canvasView.InvalidateSurface();

            //orientation = CheckOrigintation(Path);
            //orientedWExif = HandleOrientation(bitmap, orientation);
            SKBitmap newBitmap = new SKBitmap(bitmap.Width, bitmap.Height);
            using (SKCanvas newCanvas = new SKCanvas(newBitmap))
            {
                newCanvas.Clear();
                newCanvas.DrawBitmap(bitmap: bitmap, 0, 0, paint: paint);
            }
            originalBitmap = newBitmap;

            #region Border
            blackWhiteBorder.Thickness = 0;
            OriginalBorder.Thickness = 0;
            grayScaleBorder.Thickness = 0;
            invertBorder.Thickness = 0;
            swapBorder.Thickness = 0;
            sepiaBorder.Thickness = 0;
            polaroidBorder.Thickness = 0;
            sunsetBorder.Thickness = 0;
            grayYellowBorder.Thickness = 0;
            redBlueBorder.Thickness = 0;
            skyBlueBorder.Thickness = 0;
            blueBorder.Thickness = 0;
            greenVioletBorder.Thickness = 0;
            redBrowBorder.Thickness = 0;
            classicBorder.Thickness = 2;
            yellowClassicBorder.Thickness = 0;
            blueClassicBorder.Thickness = 0;
            blueClassicBorder1.Thickness = 0;
            blueClassicBorder2.Thickness = 0;
            yellowClassicBorder1.Thickness = 0;
            yellowClassicBorder2.Thickness = 0;
            grayYellowBorder1.Thickness = 0;
            redBlueBorder1.Thickness = 0;
            blueBorder1.Thickness = 0;
            #endregion
        }

        private void yellowClassicClick(object sender, EventArgs e)
        {
            filterTypeChosen = FilterType.YellowClassic;
            filterType = FilterType.YellowClassic;

            var currentFilterOption = new FilterOption
            {
                ColorContrast = contrast,
                ColorBrightness = brightness,
                ColorWhiteBalance = whitebalance,
                GrayContrast = grayscale_contrast,
                GrayBrightness = grayscale_brightness,
                BlackWhiteContrast = blackwhite_contrast
            };


            paint.ColorFilter = FilterMatrix.GetYellowClassicFilterMatrix(currentFilterOption);
            canvasView.InvalidateSurface();

            //orientation = CheckOrigintation(Path);
            //orientedWExif = HandleOrientation(bitmap, orientation);
            SKBitmap newBitmap = new SKBitmap(bitmap.Width, bitmap.Height);
            using (SKCanvas newCanvas = new SKCanvas(newBitmap))
            {
                newCanvas.Clear();
                newCanvas.DrawBitmap(bitmap: bitmap, 0, 0, paint: paint);
            }
            originalBitmap = newBitmap;

            #region Border
            blackWhiteBorder.Thickness = 0;
            OriginalBorder.Thickness = 0;
            grayScaleBorder.Thickness = 0;
            invertBorder.Thickness = 0;
            swapBorder.Thickness = 0;
            sepiaBorder.Thickness = 0;
            polaroidBorder.Thickness = 0;
            sunsetBorder.Thickness = 0;
            grayYellowBorder.Thickness = 0;
            redBlueBorder.Thickness = 0;
            skyBlueBorder.Thickness = 0;
            blueBorder.Thickness = 0;
            greenVioletBorder.Thickness = 0;
            redBrowBorder.Thickness = 0;
            classicBorder.Thickness = 0;
            yellowClassicBorder.Thickness = 2;
            blueClassicBorder.Thickness = 0;
            blueClassicBorder1.Thickness = 0;
            blueClassicBorder2.Thickness = 0;
            yellowClassicBorder1.Thickness = 0;
            yellowClassicBorder2.Thickness = 0;
            grayYellowBorder1.Thickness = 0;
            redBlueBorder1.Thickness = 0;
            blueBorder1.Thickness = 0;
            #endregion
        }

        private void blueClassicClick(object sender, EventArgs e)
        {
            filterTypeChosen = FilterType.BlueClassic;
            filterType = FilterType.BlueClassic;

            var currentFilterOption = new FilterOption
            {
                ColorContrast = contrast,
                ColorBrightness = brightness,
                ColorWhiteBalance = whitebalance,
                GrayContrast = grayscale_contrast,
                GrayBrightness = grayscale_brightness,
                BlackWhiteContrast = blackwhite_contrast
            };


            paint.ColorFilter = FilterMatrix.GetBlueClassicFilterMatrix(currentFilterOption);
            canvasView.InvalidateSurface();

            //orientation = CheckOrigintation(Path);
            //orientedWExif = HandleOrientation(bitmap, orientation);
            SKBitmap newBitmap = new SKBitmap(bitmap.Width, bitmap.Height);
            using (SKCanvas newCanvas = new SKCanvas(newBitmap))
            {
                newCanvas.Clear();
                newCanvas.DrawBitmap(bitmap: bitmap, 0, 0, paint: paint);
            }
            originalBitmap = newBitmap;

            #region Border
            blackWhiteBorder.Thickness = 0;
            OriginalBorder.Thickness = 0;
            grayScaleBorder.Thickness = 0;
            invertBorder.Thickness = 0;
            swapBorder.Thickness = 0;
            sepiaBorder.Thickness = 0;
            polaroidBorder.Thickness = 0;
            sunsetBorder.Thickness = 0;
            grayYellowBorder.Thickness = 0;
            redBlueBorder.Thickness = 0;
            skyBlueBorder.Thickness = 0;
            blueBorder.Thickness = 0;
            greenVioletBorder.Thickness = 0;
            redBrowBorder.Thickness = 0;
            classicBorder.Thickness = 0;
            yellowClassicBorder.Thickness = 0;
            blueClassicBorder.Thickness = 2;
            blueClassicBorder1.Thickness = 0;
            blueClassicBorder2.Thickness = 0;
            yellowClassicBorder1.Thickness = 0;
            yellowClassicBorder2.Thickness = 0;
            grayYellowBorder1.Thickness = 0;
            redBlueBorder1.Thickness = 0;
            blueBorder1.Thickness = 0;
            #endregion
        }

        private void yellowclassic1Click(object sender, EventArgs e)
        {
            filterTypeChosen = FilterType.YellowClassic1;
            filterType = FilterType.YellowClassic1;

            var currentFilterOption = new FilterOption
            {
                ColorContrast = contrast,
                ColorBrightness = brightness,
                ColorWhiteBalance = whitebalance,
                GrayContrast = grayscale_contrast,
                GrayBrightness = grayscale_brightness,
                BlackWhiteContrast = blackwhite_contrast
            };


            paint.ColorFilter = FilterMatrix.GetYellowClassicFilterMatrix1(currentFilterOption);
            canvasView.InvalidateSurface();

            //orientation = CheckOrigintation(Path);
            //orientedWExif = HandleOrientation(bitmap, orientation);
            SKBitmap newBitmap = new SKBitmap(bitmap.Width, bitmap.Height);
            using (SKCanvas newCanvas = new SKCanvas(newBitmap))
            {
                newCanvas.Clear();
                newCanvas.DrawBitmap(bitmap: bitmap, 0, 0, paint: paint);
            }
            originalBitmap = newBitmap;

            #region Border
            blackWhiteBorder.Thickness = 0;
            OriginalBorder.Thickness = 0;
            grayScaleBorder.Thickness = 0;
            invertBorder.Thickness = 0;
            swapBorder.Thickness = 0;
            sepiaBorder.Thickness = 0;
            polaroidBorder.Thickness = 0;
            sunsetBorder.Thickness = 0;
            grayYellowBorder.Thickness = 0;
            redBlueBorder.Thickness = 0;
            skyBlueBorder.Thickness = 0;
            blueBorder.Thickness = 0;
            greenVioletBorder.Thickness = 0;
            redBrowBorder.Thickness = 0;
            classicBorder.Thickness = 0;
            yellowClassicBorder.Thickness = 0;
            blueClassicBorder.Thickness = 0;
            blueClassicBorder1.Thickness = 0;
            blueClassicBorder2.Thickness = 0;
            yellowClassicBorder1.Thickness = 2;
            yellowClassicBorder2.Thickness = 0;
            grayYellowBorder1.Thickness = 0;
            redBlueBorder1.Thickness = 0;
            blueBorder1.Thickness = 0;
            #endregion
        }

        private void yellowclassic2Click(object sender, EventArgs e)
        {
            filterTypeChosen = FilterType.YellowClassic2;
            filterType = FilterType.YellowClassic2;

            var currentFilterOption = new FilterOption
            {
                ColorContrast = contrast,
                ColorBrightness = brightness,
                ColorWhiteBalance = whitebalance,
                GrayContrast = grayscale_contrast,
                GrayBrightness = grayscale_brightness,
                BlackWhiteContrast = blackwhite_contrast
            };


            paint.ColorFilter = FilterMatrix.GetYellowClassicFilterMatrix2(currentFilterOption);
            canvasView.InvalidateSurface();

            //orientation = CheckOrigintation(Path);
            //orientedWExif = HandleOrientation(bitmap, orientation);
            SKBitmap newBitmap = new SKBitmap(bitmap.Width, bitmap.Height);
            using (SKCanvas newCanvas = new SKCanvas(newBitmap))
            {
                newCanvas.Clear();
                newCanvas.DrawBitmap(bitmap: bitmap, 0, 0, paint: paint);
            }
            originalBitmap = newBitmap;

            #region Border
            blackWhiteBorder.Thickness = 0;
            OriginalBorder.Thickness = 0;
            grayScaleBorder.Thickness = 0;
            invertBorder.Thickness = 0;
            swapBorder.Thickness = 0;
            sepiaBorder.Thickness = 0;
            polaroidBorder.Thickness = 0;
            sunsetBorder.Thickness = 0;
            grayYellowBorder.Thickness = 0;
            redBlueBorder.Thickness = 0;
            skyBlueBorder.Thickness = 0;
            blueBorder.Thickness = 0;
            greenVioletBorder.Thickness = 0;
            redBrowBorder.Thickness = 0;
            classicBorder.Thickness = 0;
            yellowClassicBorder.Thickness = 0;
            blueClassicBorder.Thickness = 0;
            blueClassicBorder1.Thickness = 0;
            blueClassicBorder2.Thickness = 0;
            yellowClassicBorder1.Thickness = 0;
            yellowClassicBorder2.Thickness = 2;
            grayYellowBorder1.Thickness = 0;
            redBlueBorder1.Thickness = 0;
            blueBorder1.Thickness = 0;
            #endregion
        }

        private void blueClassic1Click(object sender, EventArgs e)
        {
            filterTypeChosen = FilterType.BlueClassic1;
            filterType = FilterType.BlueClassic1;

            var currentFilterOption = new FilterOption
            {
                ColorContrast = contrast,
                ColorBrightness = brightness,
                ColorWhiteBalance = whitebalance,
                GrayContrast = grayscale_contrast,
                GrayBrightness = grayscale_brightness,
                BlackWhiteContrast = blackwhite_contrast
            };


            paint.ColorFilter = FilterMatrix.GetBlueClassicFilterMatrix1(currentFilterOption);
            canvasView.InvalidateSurface();

            //orientation = CheckOrigintation(Path);
            //orientedWExif = HandleOrientation(bitmap, orientation);
            SKBitmap newBitmap = new SKBitmap(bitmap.Width, bitmap.Height);
            using (SKCanvas newCanvas = new SKCanvas(newBitmap))
            {
                newCanvas.Clear();
                newCanvas.DrawBitmap(bitmap: bitmap, 0, 0, paint: paint);
            }
            originalBitmap = newBitmap;

            #region Border
            blackWhiteBorder.Thickness = 0;
            OriginalBorder.Thickness = 0;
            grayScaleBorder.Thickness = 0;
            invertBorder.Thickness = 0;
            swapBorder.Thickness = 0;
            sepiaBorder.Thickness = 0;
            polaroidBorder.Thickness = 0;
            sunsetBorder.Thickness = 0;
            grayYellowBorder.Thickness = 0;
            redBlueBorder.Thickness = 0;
            skyBlueBorder.Thickness = 0;
            blueBorder.Thickness = 0;
            greenVioletBorder.Thickness = 0;
            redBrowBorder.Thickness = 0;
            classicBorder.Thickness = 0;
            yellowClassicBorder.Thickness = 0;
            blueClassicBorder.Thickness = 0;
            blueClassicBorder1.Thickness = 2;
            blueClassicBorder2.Thickness = 0;
            yellowClassicBorder1.Thickness = 0;
            yellowClassicBorder2.Thickness = 0;
            grayYellowBorder1.Thickness = 0;
            redBlueBorder1.Thickness = 0;
            blueBorder1.Thickness = 0;
            #endregion
        }

        private void blueClassic2Click(object sender, EventArgs e)
        {
            filterTypeChosen = FilterType.BlueClassic2;
            filterType = FilterType.BlueClassic2;

            var currentFilterOption = new FilterOption
            {
                ColorContrast = contrast,
                ColorBrightness = brightness,
                ColorWhiteBalance = whitebalance,
                GrayContrast = grayscale_contrast,
                GrayBrightness = grayscale_brightness,
                BlackWhiteContrast = blackwhite_contrast
            };


            paint.ColorFilter = FilterMatrix.GetBlueClassicFilterMatrix2(currentFilterOption);
            canvasView.InvalidateSurface();

            //orientation = CheckOrigintation(Path);
            //orientedWExif = HandleOrientation(bitmap, orientation);
            SKBitmap newBitmap = new SKBitmap(bitmap.Width, bitmap.Height);
            using (SKCanvas newCanvas = new SKCanvas(newBitmap))
            {
                newCanvas.Clear();
                newCanvas.DrawBitmap(bitmap: bitmap, 0, 0, paint: paint);
            }
            originalBitmap = newBitmap;

            #region Border
            blackWhiteBorder.Thickness = 0;
            OriginalBorder.Thickness = 0;
            grayScaleBorder.Thickness = 0;
            invertBorder.Thickness = 0;
            swapBorder.Thickness = 0;
            sepiaBorder.Thickness = 0;
            polaroidBorder.Thickness = 0;
            sunsetBorder.Thickness = 0;
            grayYellowBorder.Thickness = 0;
            redBlueBorder.Thickness = 0;
            skyBlueBorder.Thickness = 0;
            blueBorder.Thickness = 0;
            greenVioletBorder.Thickness = 0;
            redBrowBorder.Thickness = 0;
            classicBorder.Thickness = 0;
            yellowClassicBorder.Thickness = 0;
            blueClassicBorder.Thickness = 0;
            blueClassicBorder1.Thickness = 0;
            blueClassicBorder2.Thickness = 2;
            yellowClassicBorder1.Thickness = 0;
            yellowClassicBorder2.Thickness = 0;
            grayYellowBorder1.Thickness = 0;
            redBlueBorder1.Thickness = 0;
            blueBorder1.Thickness = 0;
            #endregion
        }

        private void blueClick1(object sender, EventArgs e)
        {
            filterTypeChosen = FilterType.Blue1;
            filterType = FilterType.Blue1;

            var currentFilterOption = new FilterOption
            {
                ColorContrast = contrast,
                ColorBrightness = brightness,
                ColorWhiteBalance = whitebalance,
                GrayContrast = grayscale_contrast,
                GrayBrightness = grayscale_brightness,
                BlackWhiteContrast = blackwhite_contrast
            };


            paint.ColorFilter = FilterMatrix.GetBlueFilterMatrix1(currentFilterOption);
            canvasView.InvalidateSurface();

            //orientation = CheckOrigintation(Path);
            //orientedWExif = HandleOrientation(bitmap, orientation);
            SKBitmap newBitmap = new SKBitmap(bitmap.Width, bitmap.Height);
            using (SKCanvas newCanvas = new SKCanvas(newBitmap))
            {
                newCanvas.Clear();
                newCanvas.DrawBitmap(bitmap: bitmap, 0, 0, paint: paint);
            }
            originalBitmap = newBitmap;

            #region Border
            blackWhiteBorder.Thickness = 0;
            OriginalBorder.Thickness = 0;
            grayScaleBorder.Thickness = 0;
            invertBorder.Thickness = 0;
            swapBorder.Thickness = 0;
            sepiaBorder.Thickness = 0;
            polaroidBorder.Thickness = 0;
            sunsetBorder.Thickness = 0;
            grayYellowBorder.Thickness = 0;
            redBlueBorder.Thickness = 0;
            skyBlueBorder.Thickness = 0;
            blueBorder.Thickness = 0;
            greenVioletBorder.Thickness = 0;
            redBrowBorder.Thickness = 0;
            classicBorder.Thickness = 0;
            yellowClassicBorder.Thickness = 0;
            blueClassicBorder.Thickness = 0;
            blueClassicBorder1.Thickness = 0;
            blueClassicBorder2.Thickness = 0;
            yellowClassicBorder1.Thickness = 0;
            yellowClassicBorder2.Thickness = 0;
            grayYellowBorder1.Thickness = 0;
            redBlueBorder1.Thickness = 0;
            blueBorder1.Thickness = 2;
            #endregion
        }

        private void redblueClick1(object sender, EventArgs e)
        {
            filterTypeChosen = FilterType.RedBlue1;
            filterType = FilterType.RedBlue1;

            var currentFilterOption = new FilterOption
            {
                ColorContrast = contrast,
                ColorBrightness = brightness,
                ColorWhiteBalance = whitebalance,
                GrayContrast = grayscale_contrast,
                GrayBrightness = grayscale_brightness,
                BlackWhiteContrast = blackwhite_contrast
            };


            paint.ColorFilter = FilterMatrix.GetRedBlueFilterMatrix1(currentFilterOption);
            canvasView.InvalidateSurface();

            //orientation = CheckOrigintation(Path);
            //orientedWExif = HandleOrientation(bitmap, orientation);
            SKBitmap newBitmap = new SKBitmap(bitmap.Width, bitmap.Height);
            using (SKCanvas newCanvas = new SKCanvas(newBitmap))
            {
                newCanvas.Clear();
                newCanvas.DrawBitmap(bitmap: bitmap, 0, 0, paint: paint);
            }
            originalBitmap = newBitmap;

            #region Border
            blackWhiteBorder.Thickness = 0;
            OriginalBorder.Thickness = 0;
            grayScaleBorder.Thickness = 0;
            invertBorder.Thickness = 0;
            swapBorder.Thickness = 0;
            sepiaBorder.Thickness = 0;
            polaroidBorder.Thickness = 0;
            sunsetBorder.Thickness = 0;
            grayYellowBorder.Thickness = 0;
            redBlueBorder.Thickness = 0;
            skyBlueBorder.Thickness = 0;
            blueBorder.Thickness = 0;
            greenVioletBorder.Thickness = 0;
            redBrowBorder.Thickness = 0;
            classicBorder.Thickness = 0;
            yellowClassicBorder.Thickness = 0;
            blueClassicBorder.Thickness = 0;
            blueClassicBorder1.Thickness = 0;
            blueClassicBorder2.Thickness = 0;
            yellowClassicBorder1.Thickness = 0;
            yellowClassicBorder2.Thickness = 0;
            grayYellowBorder1.Thickness = 0;
            redBlueBorder1.Thickness = 1;
            blueBorder1.Thickness = 0;
            #endregion
        }

        private void grayyellowClick1(object sender, EventArgs e)
        {
            filterTypeChosen = FilterType.GrayYellow1;
            filterType = FilterType.GrayYellow1;

            var currentFilterOption = new FilterOption
            {
                ColorContrast = contrast,
                ColorBrightness = brightness,
                ColorWhiteBalance = whitebalance,
                GrayContrast = grayscale_contrast,
                GrayBrightness = grayscale_brightness,
                BlackWhiteContrast = blackwhite_contrast
            };

            EditImageModel.ColorContrast = contrast;
            EditImageModel.ColorBrightness = brightness;
            EditImageModel.ColorWhiteBalance = whitebalance;
            EditImageModel.GrayBrightness = grayscale_brightness;
            EditImageModel.GrayContrast = grayscale_brightness;
            EditImageModel.BlackWhiteContrast = blackwhite_contrast;
            EditImageModel.FilterTypeChosen = filterTypeChosen;

            paint.ColorFilter = FilterMatrix.GetGrayYellowFilterMatrix1(currentFilterOption);
            canvasView.InvalidateSurface();

            //orientation = CheckOrigintation(Path);
            //orientedWExif = HandleOrientation(bitmap, orientation);
            SKBitmap newBitmap = new SKBitmap(bitmap.Width, bitmap.Height);
            using (SKCanvas newCanvas = new SKCanvas(newBitmap))
            {
                newCanvas.Clear();
                newCanvas.DrawBitmap(bitmap: bitmap, 0, 0, paint: paint);
            }
            originalBitmap = newBitmap;

            #region Border
            blackWhiteBorder.Thickness = 0;
            OriginalBorder.Thickness = 0;
            grayScaleBorder.Thickness = 0;
            invertBorder.Thickness = 0;
            swapBorder.Thickness = 0;
            sepiaBorder.Thickness = 0;
            polaroidBorder.Thickness = 0;
            sunsetBorder.Thickness = 0;
            grayYellowBorder.Thickness = 0;
            redBlueBorder.Thickness = 0;
            skyBlueBorder.Thickness = 0;
            blueBorder.Thickness = 0;
            greenVioletBorder.Thickness = 0;
            redBrowBorder.Thickness = 0;
            classicBorder.Thickness = 0;
            yellowClassicBorder.Thickness = 0;
            blueClassicBorder.Thickness = 0;
            blueClassicBorder1.Thickness = 0;
            blueClassicBorder2.Thickness = 0;
            yellowClassicBorder1.Thickness = 0;
            yellowClassicBorder2.Thickness = 0;
            grayYellowBorder1.Thickness = 2;
            redBlueBorder1.Thickness = 0;
            blueBorder1.Thickness = 0;
            #endregion
        }

        #endregion

        #region CheckRotate

        public SKEncodedOrigin CheckOrigintation(string path)
        {
            var imageByte = File.ReadAllBytes(path);
            SKEncodedOrigin orientation;

            using (MemoryStream pathStream = new MemoryStream(imageByte))
            {
                using (var inputStream = new SKManagedStream(pathStream))
                {
                    using (var codec = SKCodec.Create(inputStream))
                    {
                        orientation = codec.EncodedOrigin;
                        return orientation;
                    }
                }
            }
        }

        public static SKBitmap HandleOrientation(SKBitmap bitmap, SKEncodedOrigin orientation)
        {
            SKBitmap rotated;
            switch (orientation)
            {
                case SKEncodedOrigin.BottomRight:

                    using (var surface = new SKCanvas(bitmap))
                    {
                        surface.RotateDegrees(180, bitmap.Width / 2, bitmap.Height / 2);
                        surface.DrawBitmap(bitmap.Copy(), 0, 0);
                    }

                    return bitmap;

                case SKEncodedOrigin.RightTop:
                    rotated = new SKBitmap(bitmap.Height, bitmap.Width);

                    using (var surface = new SKCanvas(rotated))
                    {
                        surface.Translate(rotated.Width, 0);
                        surface.RotateDegrees(90);
                        surface.DrawBitmap(bitmap, 0, 0);
                    }

                    return rotated;

                case SKEncodedOrigin.LeftBottom:
                    rotated = new SKBitmap(bitmap.Height, bitmap.Width);

                    using (var surface = new SKCanvas(rotated))
                    {
                        surface.Translate(0, rotated.Height);
                        surface.RotateDegrees(270);
                        surface.DrawBitmap(bitmap, 0, 0);
                    }

                    return rotated;

                default:
                    return bitmap;
            }
        }
        #endregion

        #region Studio
        private void WhiteBalance_click(object sender, EventArgs e)
        {
            WhiteBalanceValue.Text = ((int)WhiteBalance.Value).ToString();
            float newValue = (float)(WhiteBalance.Value) / 100;
            whitebalance = newValue;
            filterTypeChosen = FilterType.ColorFilter;
            currentFilterOption = new FilterOption
            {
                ColorContrast = contrast,
                ColorBrightness = brightness,
                ColorWhiteBalance = whitebalance,
                GrayContrast = grayscale_contrast,
                GrayBrightness = grayscale_brightness,
                BlackWhiteContrast = blackwhite_contrast
            };

            paint.ColorFilter = FilterMatrix.GetColorFilterMatrix(currentFilterOption);
            canvasView.InvalidateSurface();
        }

        private void brightNess_Click(object sender, EventArgs e)
        {
            BrightnessValue.Text = ((int)Brightness.Value).ToString();
            float newValue = (float)Brightness.Value;
            brightness = newValue;
            filterTypeChosen = FilterType.ColorFilter;

            currentFilterOption = new FilterOption
            {
                ColorContrast = contrast,
                ColorBrightness = brightness,
                ColorWhiteBalance = whitebalance,
                GrayContrast = grayscale_contrast,
                GrayBrightness = grayscale_brightness,
                BlackWhiteContrast = blackwhite_contrast
            };

            #region Check Filter Option
            //switch (filterTypeChosen)
            //{
            //    case FilterType.BlackAndWhiteFilter:
            //        paint.ColorFilter = FilterMatrices.GetBlackAndWhiteFilterMatrix(currentFilterOption);
            //        break;
            //    case FilterType.GrayScaleFilter:
            //        paint.ColorFilter = FilterMatrices.GetGrayScaleFilterMatrix(currentFilterOption);
            //        break;
            //    case FilterType.InvertFilter:
            //        paint.ColorFilter = FilterMatrices.GetInvertFilterMatrix(currentFilterOption);
            //        break;
            //    case FilterType.PolaroidFilter:
            //        paint.ColorFilter = FilterMatrices.GetPolaroidFilterMatrix(currentFilterOption);
            //        break;
            //    case FilterType.SepiaFilter:
            //        paint.ColorFilter = FilterMatrices.GetSepiaFilterMatrix(currentFilterOption);
            //        break;
            //    case FilterType.SwapFilter:
            //        paint.ColorFilter = FilterMatrices.GetSwapFilterMatrix(currentFilterOption);
            //        break;
            //    case FilterType.Sunset:
            //        paint.ColorFilter = FilterMatrices.GetSunsetFilterMatrix(currentFilterOption);
            //        break;
            //    case FilterType.GrayYellow:
            //        paint.ColorFilter = FilterMatrices.GetGrayYellowFilterMatrix(currentFilterOption);
            //        break;
            //    case FilterType.GrayYellow1:
            //        paint.ColorFilter = FilterMatrices.GetGrayYellowFilterMatrix1(currentFilterOption);
            //        break;
            //    case FilterType.SkyBlue:
            //        paint.ColorFilter = FilterMatrices.GetSkyBlueFilterMatrix(currentFilterOption);
            //        break;
            //    case FilterType.RedBlue:
            //        paint.ColorFilter = FilterMatrices.GetRedBlueFilterMatrix(currentFilterOption);
            //        break;
            //    case FilterType.RedBlue1:
            //        paint.ColorFilter = FilterMatrices.GetRedBlueFilterMatrix1(currentFilterOption);
            //        break;
            //    case FilterType.Blue:
            //        paint.ColorFilter = FilterMatrices.GetBlueFilterMatrix(currentFilterOption);
            //        break;
            //    case FilterType.Blue1:
            //        paint.ColorFilter = FilterMatrices.GetBlueFilterMatrix1(currentFilterOption);
            //        break;
            //    case FilterType.BlueViolet:
            //        paint.ColorFilter = FilterMatrices.GetBlueVioletFilterMatrix(currentFilterOption);
            //        break;
            //    case FilterType.RedBrow:
            //        paint.ColorFilter = FilterMatrices.GetRedBrowFilterMatrix(currentFilterOption);
            //        break;
            //    case FilterType.Classic:
            //        paint.ColorFilter = FilterMatrices.GetClassicFilterMatrix(currentFilterOption);
            //        break;
            //    case FilterType.BlueClassic:
            //        paint.ColorFilter = FilterMatrices.GetBlueClassicFilterMatrix(currentFilterOption);
            //        break;
            //    case FilterType.BlueClassic1:
            //        paint.ColorFilter = FilterMatrices.GetBlueClassicFilterMatrix1(currentFilterOption);
            //        break;
            //    case FilterType.BlueClassic2:
            //        paint.ColorFilter = FilterMatrices.GetBlueClassicFilterMatrix2(currentFilterOption);
            //        break;
            //    case FilterType.YellowClassic:
            //        paint.ColorFilter = FilterMatrices.GetYellowClassicFilterMatrix(currentFilterOption);
            //        break;
            //    case FilterType.YellowClassic1:
            //        paint.ColorFilter = FilterMatrices.GetYellowClassicFilterMatrix1(currentFilterOption);
            //        break;
            //    case FilterType.YellowClassic2:
            //        paint.ColorFilter = FilterMatrices.GetYellowClassicFilterMatrix2(currentFilterOption);
            //        break;
            //    default:
            //        paint.ColorFilter = FilterMatrices.GetColorFilterMatrix(currentFilterOption);
            //        break;
            //}
            #endregion
            paint.ColorFilter = FilterMatrix.GetColorFilterMatrix(currentFilterOption);
            canvasView.InvalidateSurface();
        }

        private void contrast_Click(object sender, EventArgs e)
        {
            ContrastValue.Text = ((int)Contrast.Value).ToString();
            float newValue = (float)(Contrast.Value) / 100;
            contrast = newValue;
            filterTypeChosen = FilterType.ColorFilter;

            currentFilterOption = new FilterOption
            {
                ColorContrast = contrast,
                ColorBrightness = brightness,
                ColorWhiteBalance = whitebalance,
                GrayContrast = grayscale_contrast,
                GrayBrightness = grayscale_brightness,
                BlackWhiteContrast = blackwhite_contrast
            };

            #region Check Filter Option
            //switch (filterTypeChosen)
            //{
            //    case FilterType.BlackAndWhiteFilter:
            //        paint.ColorFilter = FilterMatrices.GetBlackAndWhiteFilterMatrix(currentFilterOption);
            //        break;
            //    case FilterType.GrayScaleFilter:
            //        paint.ColorFilter = FilterMatrices.GetGrayScaleFilterMatrix(currentFilterOption);
            //        break;
            //    case FilterType.InvertFilter:
            //        paint.ColorFilter = FilterMatrices.GetInvertFilterMatrix(currentFilterOption);
            //        break;
            //    case FilterType.PolaroidFilter:
            //        paint.ColorFilter = FilterMatrices.GetPolaroidFilterMatrix(currentFilterOption);
            //        break;
            //    case FilterType.SepiaFilter:
            //        paint.ColorFilter = FilterMatrices.GetSepiaFilterMatrix(currentFilterOption);
            //        break;
            //    case FilterType.SwapFilter:
            //        paint.ColorFilter = FilterMatrices.GetSwapFilterMatrix(currentFilterOption);
            //        break;
            //    case FilterType.Sunset:
            //        paint.ColorFilter = FilterMatrices.GetSunsetFilterMatrix(currentFilterOption);
            //        break;
            //    case FilterType.GrayYellow:
            //        paint.ColorFilter = FilterMatrices.GetGrayYellowFilterMatrix(currentFilterOption);
            //        break;
            //    case FilterType.GrayYellow1:
            //        paint.ColorFilter = FilterMatrices.GetGrayYellowFilterMatrix1(currentFilterOption);
            //        break;
            //    case FilterType.SkyBlue:
            //        paint.ColorFilter = FilterMatrices.GetSkyBlueFilterMatrix(currentFilterOption);
            //        break;
            //    case FilterType.RedBlue:
            //        paint.ColorFilter = FilterMatrices.GetRedBlueFilterMatrix(currentFilterOption);
            //        break;
            //    case FilterType.RedBlue1:
            //        paint.ColorFilter = FilterMatrices.GetRedBlueFilterMatrix1(currentFilterOption);
            //        break;
            //    case FilterType.Blue:
            //        paint.ColorFilter = FilterMatrices.GetBlueFilterMatrix(currentFilterOption);
            //        break;
            //    case FilterType.Blue1:
            //        paint.ColorFilter = FilterMatrices.GetBlueFilterMatrix1(currentFilterOption);
            //        break;
            //    case FilterType.BlueViolet:
            //        paint.ColorFilter = FilterMatrices.GetBlueVioletFilterMatrix(currentFilterOption);
            //        break;
            //    case FilterType.RedBrow:
            //        paint.ColorFilter = FilterMatrices.GetRedBrowFilterMatrix(currentFilterOption);
            //        break;
            //    case FilterType.Classic:
            //        paint.ColorFilter = FilterMatrices.GetClassicFilterMatrix(currentFilterOption);
            //        break;
            //    case FilterType.BlueClassic:
            //        paint.ColorFilter = FilterMatrices.GetBlueClassicFilterMatrix(currentFilterOption);
            //        break;
            //    case FilterType.BlueClassic1:
            //        paint.ColorFilter = FilterMatrices.GetBlueClassicFilterMatrix1(currentFilterOption);
            //        break;
            //    case FilterType.BlueClassic2:
            //        paint.ColorFilter = FilterMatrices.GetBlueClassicFilterMatrix2(currentFilterOption);
            //        break;
            //    case FilterType.YellowClassic:
            //        paint.ColorFilter = FilterMatrices.GetYellowClassicFilterMatrix(currentFilterOption);
            //        break;
            //    case FilterType.YellowClassic1:
            //        paint.ColorFilter = FilterMatrices.GetYellowClassicFilterMatrix1(currentFilterOption);
            //        break;
            //    case FilterType.YellowClassic2:
            //        paint.ColorFilter = FilterMatrices.GetYellowClassicFilterMatrix2(currentFilterOption);
            //        break;
            //    default:
            //        paint.ColorFilter = FilterMatrices.GetColorFilterMatrix(currentFilterOption);
            //        break;
            //}
            #endregion
            paint.ColorFilter = FilterMatrix.GetColorFilterMatrix(currentFilterOption);
            canvasView.InvalidateSurface();
        }
        #endregion

        #region Drawing
        private void OnTouchEffectAction(object sender, Controls.TouchActionEventArgs e)
        {
            undo.Opacity = 1;
            undo.IsEnabled = true;
            clear.Opacity = 1;
            clear.IsEnabled = true;
            //filterTypeChosen = FilterType.DrawOrPaint;
            switch (e.Type)
            {
                case TouchActionType.Pressed:
                    if (!inProgressPolylines.ContainsKey(e.Id))
                    {
                        FingerPaintPolyline polyline = new FingerPaintPolyline
                        {
                            StrokeColor = SKColor,
                            BlendMode = SKBlendMode,
                            StrokeWidth = (float)PenSize.Value
                        };
                        polyline.Path.MoveTo(ConvertToPixel(e.Location));
                        inProgressPolylines.Add(e.Id, polyline);
                        drawView.InvalidateSurface();
                    }
                    break;

                case TouchActionType.Moved:
                    if (inProgressPolylines.ContainsKey(e.Id))
                    {
                        FingerPaintPolyline polyline = inProgressPolylines[e.Id];
                        polyline.Path.LineTo(ConvertToPixel(e.Location));
                        drawView.InvalidateSurface();
                    }
                    break;

                case TouchActionType.Released:
                    {
                        completedPolylines.Add(inProgressPolylines[e.Id]);
                        inProgressPolylines.Remove(e.Id);
                        drawView.InvalidateSurface();
                    }
                    break;

                case TouchActionType.Cancelled:
                    if (inProgressPolylines.ContainsKey(e.Id))
                    {
                        inProgressPolylines.Remove(e.Id);
                        drawView.InvalidateSurface();
                    }
                    break;
            }
        }

        #region ConvertToPixel
        SKPoint ConvertToPixel(Point pt)
        {
            return new SKPoint((float)(drawView.CanvasSize.Width * pt.X / drawView.Width),
                               (float)(drawView.CanvasSize.Height * pt.Y / drawView.Height));
        }

        #endregion


        private void draw(object sender, EventArgs e)
        {
            drawVisile.IsVisible = true;
            eraserVisile.IsVisible = false;

            SKBlendMode = SKBlendMode.SrcOver;
            if (SKColor1 != SKColors.Black)
                SKColor = SKColor1;
            else
                SKColor = SKColors.Black;
        }

        private async void colorPicker(object sender, EventArgs e)
        {
            drawVisile.IsVisible = true;
            eraserVisile.IsVisible = false;
            var colorPickerPopup = new ColorPickerPopup(true);
            await colorPickerPopup.Show(
                acceptComamnd: new Command(async () =>
                {
                    SKColor1 = colorPickerPopup.Color.ToSKColor();
                    SKColor = SKColor1;
                    color.BackgroundColor = SKColor.ToFormsColor();
                })
                );
            //await ColorPickerPopup.Instance.Show(acceptComamnd:
            //   new Command(async () =>
            //   {
            //       SKColor = ColorPickerPopup.Instance.GetColor();
            //   })
            //   );
        }

        private void erase(object sender, EventArgs e)
        {
            drawVisile.IsVisible = false;
            eraserVisile.IsVisible = true;
            SKColor = SKColors.Transparent;
            SKBlendMode = SKBlendMode.Src;
        }

        private void Clear_Clicked(object sender, EventArgs e)
        {
            undo.Opacity = 0.3;
            undo.IsEnabled = false;
            redo.Opacity = 0.3;
            redo.IsEnabled = false;
            clear.Opacity = 0.3;
            clear.IsEnabled = false;

            completedPolylines.Clear();
            drawView.InvalidateSurface();
        }

        private void Undo_Clicked(object sender, EventArgs e)
        {
            if (completedPolylines.Count() > 0)
            {
                completedUndoPolylines.Add(completedPolylines.ElementAt(completedPolylines.Count() - 1));
                completedPolylines.Remove(completedPolylines.ElementAt(completedPolylines.Count() - 1));
                drawView.InvalidateSurface();

                redo.Opacity = 1;
                redo.IsEnabled = true;
            }

            if (completedPolylines.Count() == 0)
            {
                undo.Opacity = 0.3;
                undo.IsEnabled = false;
            }
        }

        private void Redo_Clicked(object sender, EventArgs e)
        {
            if (completedUndoPolylines.Count() > 0)
            {
                completedPolylines.Add(completedUndoPolylines.ElementAt(completedUndoPolylines.Count() - 1));
                completedUndoPolylines.Remove(completedUndoPolylines.ElementAt(completedUndoPolylines.Count() - 1));
                drawView.InvalidateSurface();

                undo.Opacity = 1;
                undo.IsEnabled = true;
            }
            if (completedUndoPolylines.Count() == 0)
            {
                redo.Opacity = 0.3;
                redo.IsEnabled = false;
            }
        }

        private void PenSizeClicked(object sender, EventArgs e)
        {
            var value = (int)PenSize.Value;
            PenSizeValue.Text = value.ToString();
        }

        #endregion

        #region Add Text

        #region Properties
        public bool ConfigVisible { get; set; }
        public ImageEditorConfig Config { get; set; } = new ImageEditorConfig();
        public bool CanAddStickers { get; set; } = true;
        public int? OutImageHeight { get; set; } = null;
        public int? OutImageWidht { get; set; } = null;
        public bool UseSampleImage { get; set; } = true;

        private readonly Assembly assembly;
        private List<SKBitmapImageSource> stickers;
        private List<SKBitmapImageSource> signatures;
        private int stickersCount = 100;

        public List<BBAspect> Aspects { get; } = new List<BBAspect> { BBAspect.Auto, BBAspect.AspectFill, BBAspect.AspectFit, BBAspect.Fill };


        public List<BackgroundType> BackgroundTypes { get; } = new List<BackgroundType> { BackgroundType.Transparent, BackgroundType.StretchedImage, BackgroundType.Color };
        public List<SKColor> Colors { get; } = new List<SKColor> { SKColors.Red, SKColors.Green, SKColors.Blue };
        #endregion
        private void GetBitmaps(int maxCount)
        {
            List<SKBitmapImageSource> _stickers = null;

            var applicationTypeInfo = Application.Current.GetType().GetTypeInfo();
            string[] resourceIDs = assembly.GetManifestResourceNames();
            int i = 0;
            foreach (string resourceID in resourceIDs)
            {
                if (resourceID.Contains("sticker") && resourceID.EndsWith(".png"))
                {
                    if (_stickers == null)
                        _stickers = new List<SKBitmapImageSource>();

                    using (Stream stream = assembly.GetManifestResourceStream(resourceID))
                    {
                        _stickers.Add(SKBitmap.Decode(stream));
                    }
                }
                i++;
                if (i > maxCount)
                    break;
            }
            stickers = _stickers;
        }

        private SKBitmap MergeBitmap(SKBitmap sKBitmap, SKBitmap textBitmap)
        {
            SKBitmap newBitmap = new SKBitmap(sKBitmap.Width, sKBitmap.Height);
            using (SKCanvas newCanvas = new SKCanvas(newBitmap))
            {
                newCanvas.Clear();
                newCanvas.DrawBitmap(bitmap: sKBitmap, 0, 0);
            }
            var dstInfo1 = new SKImageInfo(newBitmap.Width, newBitmap.Height);
            resizedBitmap = textBitmap.Resize(dstInfo1, SKBitmapResizeMethod.Hamming);

            if (newBitmap != null)
            {
                int offset = newBitmap.Width / 2 - resizedBitmap.Width / 2;
                int offsetTop = newBitmap.Height / 2 - resizedBitmap.Height / 2;

                using (SKCanvas canvas = new SKCanvas(newBitmap))
                {
                    canvas.DrawBitmap(resizedBitmap, SKRect.Create(offset, offsetTop, resizedBitmap.Width, resizedBitmap.Height));
                }
            }
            return newBitmap;
        }
        #endregion

        #region Crop

        private async void Crop_Click(object sender, EventArgs e)
        {

            var display = DeviceDisplay.MainDisplayInfo;
            if (!(Config?.Stickers?.Count > 0) && CanAddStickers)
                GetBitmaps(stickersCount);
            Config = new ImageEditorConfig(backgroundType: BackgroundType.StretchedImage, outImageHeight: (int)display.Height, outImageWidht: (int)display.Width, aspect: BBAspect.Auto, canTransformMainBitmap: false, stickers: stickers);
            try
            {
                Indicator.IsRunning = true;
                EditGrid.IsEnabled = false;
                await Task.Run(() =>
                {
                    Config.Stickers = CanAddStickers ? stickers : null;
                    Config.SetOutImageSize(OutImageHeight, OutImageWidht);

                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        Indicator.IsRunning = false;
                        EditGrid.IsEnabled = true;
                        var skBitmap = GetBitmap();
                        var data = await ImageEditor.Instance.GetCropperImage(skBitmap, Config);
                        if (data != null)
                        {
                            var newBitmap = SKBitmap.Decode(data);
                            //var sKBitmap = MergeBitmap(skBitmap, newBitmap);
                            originalBitmap = newBitmap;
                            bitmap = newBitmap;

                            completedPolylines.Clear();
                            drawView.InvalidateSurface();

                            brightness = DEFAULT_BRIGHTNESS;
                            contrast = DEFAULT_CONTRAST;
                            whitebalance = DEFAULT_WHITEBALANCE;
                            currentFilterOption = new FilterOption
                            {
                                ColorContrast = contrast,
                                ColorBrightness = brightness,
                                ColorWhiteBalance = whitebalance,
                                GrayContrast = grayscale_contrast,
                                GrayBrightness = grayscale_brightness,
                                BlackWhiteContrast = blackwhite_contrast
                            };
                            paint.ColorFilter = FilterMatrix.GetColorFilterMatrix(currentFilterOption);
                            canvasView.InvalidateSurface();
                            data = null;
                        }
                    });
                });

                #region Cropper
                //new ImageCropper()
                //{
                //    PageTitle = "Classic",
                //    Success = (croppedPath) =>
                //    {
                //        Device.BeginInvokeOnMainThread(() =>
                //        {
                //            CropPath = croppedPath;
                //            using (Stream stream = File.OpenRead(CropPath))
                //            {
                //                bitmap = SKBitmap.Decode(stream);
                //                originalBitmap = bitmap;
                //                var orientation = CheckOrigintation(Path);
                //                if (orientation == SKEncodedOrigin.BottomRight || orientation == SKEncodedOrigin.RightTop || orientation == SKEncodedOrigin.LeftBottom)
                //                {
                //                    SKBitmap orientedWExif = HandleOrientation(bitmap, orientation);

                //                    SKBitmap newBitmap = new SKBitmap(orientedWExif.Width, orientedWExif.Height);
                //                    using (SKCanvas newCanvas = new SKCanvas(newBitmap))
                //                    {
                //                        newCanvas.Clear();
                //                        newCanvas.DrawBitmap(bitmap: orientedWExif, 0, 0);
                //                    }
                //                    bitmap = newBitmap;
                //                    originalBitmap = newBitmap;
                //                }
                //            }
                //            canvasView.InvalidateSurface();
                //        });
                //    }
                //}.Show(this, Path);
                #endregion
            }
            catch (Exception ex)
            {

            }
        }
        #endregion

        #region Save Filter
        private async void LoadFilter_Tapped(object sender, EventArgs e)
        {
            //Database database = new Database();
            //EditImageModels = (List<EditImageModel>)database.GetAll();
            //await ImportSettingPopup.Instance.Show(acceptCommand: new Command(async () =>
            //{
            //    Indicator.IsRunning = true;
            //    EditGrid.IsEnabled = false;
            //    await Task.Delay(500);
            //    EditImageModel = ImportSettingPopup.Instance.GetFilter();
            //    if (EditImageModel != null)
            //    {
            //        filterTypeChosen = EditImageModel.FilterTypeChosen;
            //        Contrast.Value = EditImageModel.ColorContrast * 100;
            //        Brightness.Value = EditImageModel.ColorBrightness;
            //        WhiteBalance.Value = EditImageModel.ColorWhiteBalance * 100;
            //        ContrastValue.Text = ((int)Contrast.Value).ToString();
            //        BrightnessValue.Text = ((int)Brightness.Value).ToString();
            //        WhiteBalanceValue.Text = ((int)WhiteBalance.Value).ToString();
            //        var currentFilterOption = new FilterOption
            //        {
            //            ColorContrast = EditImageModel.ColorContrast,
            //            ColorBrightness = EditImageModel.ColorBrightness,
            //            ColorWhiteBalance = EditImageModel.ColorWhiteBalance,
            //            GrayContrast = EditImageModel.GrayContrast,
            //            GrayBrightness = EditImageModel.GrayBrightness,
            //            BlackWhiteContrast = EditImageModel.BlackWhiteContrast
            //        };
            //        switch (EditImageModel.FilterTypeChosen)
            //        {
            //            case FilterType.BlackAndWhiteFilter:
            //                paint.ColorFilter = FilterMatrix.GetBlackAndWhiteFilterMatrix(currentFilterOption);
            //                canvasView.InvalidateSurface();
            //                Indicator.IsRunning = false;
            //                EditGrid.IsEnabled = true;
            //                return;
            //            case FilterType.GrayScaleFilter:
            //                paint.ColorFilter = FilterMatrix.GetGrayScaleFilterMatrix(currentFilterOption);
            //                canvasView.InvalidateSurface();
            //                Indicator.IsRunning = false;
            //                EditGrid.IsEnabled = true;
            //                return;
            //            case FilterType.InvertFilter:
            //                paint.ColorFilter = FilterMatrix.GetInvertFilterMatrix(currentFilterOption);
            //                canvasView.InvalidateSurface();
            //                Indicator.IsRunning = false;
            //                EditGrid.IsEnabled = true;
            //                return;
            //            case FilterType.SwapFilter:
            //                paint.ColorFilter = FilterMatrix.GetSwapFilterMatrix(currentFilterOption);
            //                canvasView.InvalidateSurface();
            //                Indicator.IsRunning = false;
            //                EditGrid.IsEnabled = true;
            //                return;
            //            case FilterType.SepiaFilter:
            //                paint.ColorFilter = FilterMatrix.GetSepiaFilterMatrix(currentFilterOption);
            //                canvasView.InvalidateSurface();
            //                Indicator.IsRunning = false;
            //                EditGrid.IsEnabled = true;
            //                return;
            //            case FilterType.PolaroidFilter:
            //                paint.ColorFilter = FilterMatrix.GetPolaroidFilterMatrix(currentFilterOption);
            //                canvasView.InvalidateSurface();
            //                Indicator.IsRunning = false;
            //                EditGrid.IsEnabled = true;
            //                return;
            //            case FilterType.Sunset:
            //                paint.ColorFilter = FilterMatrix.GetSunsetFilterMatrix(currentFilterOption);
            //                canvasView.InvalidateSurface();
            //                Indicator.IsRunning = false;
            //                EditGrid.IsEnabled = true;
            //                return;
            //            case FilterType.GrayYellow:
            //                paint.ColorFilter = FilterMatrix.GetGrayYellowFilterMatrix(currentFilterOption);
            //                canvasView.InvalidateSurface();
            //                Indicator.IsRunning = false;
            //                EditGrid.IsEnabled = true;
            //                return;
            //            case FilterType.GrayYellow1:
            //                paint.ColorFilter = FilterMatrix.GetGrayYellowFilterMatrix1(currentFilterOption);
            //                canvasView.InvalidateSurface();
            //                Indicator.IsRunning = false;
            //                EditGrid.IsEnabled = true;
            //                return;
            //            case FilterType.RedBlue:
            //                paint.ColorFilter = FilterMatrix.GetRedBlueFilterMatrix(currentFilterOption);
            //                canvasView.InvalidateSurface();
            //                Indicator.IsRunning = false;
            //                EditGrid.IsEnabled = true;
            //                return;
            //            case FilterType.RedBlue1:
            //                paint.ColorFilter = FilterMatrix.GetRedBlueFilterMatrix1(currentFilterOption);
            //                canvasView.InvalidateSurface();
            //                Indicator.IsRunning = false;
            //                EditGrid.IsEnabled = true;
            //                return;
            //            case FilterType.SkyBlue:
            //                paint.ColorFilter = FilterMatrix.GetSkyBlueFilterMatrix(currentFilterOption);
            //                canvasView.InvalidateSurface();
            //                Indicator.IsRunning = false;
            //                EditGrid.IsEnabled = true;
            //                return;
            //            case FilterType.Blue:
            //                paint.ColorFilter = FilterMatrix.GetBlueFilterMatrix(currentFilterOption);
            //                canvasView.InvalidateSurface();
            //                Indicator.IsRunning = false;
            //                EditGrid.IsEnabled = true;
            //                return;
            //            case FilterType.Blue1:
            //                paint.ColorFilter = FilterMatrix.GetBlueFilterMatrix1(currentFilterOption);
            //                canvasView.InvalidateSurface();
            //                Indicator.IsRunning = false;
            //                EditGrid.IsEnabled = true;
            //                return;
            //            case FilterType.BlueViolet:
            //                paint.ColorFilter = FilterMatrix.GetBlueVioletFilterMatrix(currentFilterOption);
            //                canvasView.InvalidateSurface();
            //                Indicator.IsRunning = false;
            //                EditGrid.IsEnabled = true;
            //                return;
            //            case FilterType.RedBrow:
            //                paint.ColorFilter = FilterMatrix.GetRedBrowFilterMatrix(currentFilterOption);
            //                canvasView.InvalidateSurface();
            //                Indicator.IsRunning = false;
            //                EditGrid.IsEnabled = true;
            //                return;
            //            case FilterType.Classic:
            //                paint.ColorFilter = FilterMatrix.GetClassicFilterMatrix(currentFilterOption);
            //                canvasView.InvalidateSurface();
            //                Indicator.IsRunning = false;
            //                EditGrid.IsEnabled = true;
            //                return;
            //            case FilterType.BlueClassic:
            //                paint.ColorFilter = FilterMatrix.GetBlueClassicFilterMatrix(currentFilterOption);
            //                canvasView.InvalidateSurface();
            //                Indicator.IsRunning = false;
            //                EditGrid.IsEnabled = true;
            //                return;
            //            case FilterType.BlueClassic1:
            //                paint.ColorFilter = FilterMatrix.GetBlueClassicFilterMatrix1(currentFilterOption);
            //                canvasView.InvalidateSurface();
            //                Indicator.IsRunning = false;
            //                EditGrid.IsEnabled = true;
            //                return;
            //            case FilterType.BlueClassic2:
            //                paint.ColorFilter = FilterMatrix.GetBlueClassicFilterMatrix2(currentFilterOption);
            //                canvasView.InvalidateSurface();
            //                Indicator.IsRunning = false;
            //                EditGrid.IsEnabled = true;
            //                return;
            //            case FilterType.YellowClassic:
            //                paint.ColorFilter = FilterMatrix.GetYellowClassicFilterMatrix(currentFilterOption);
            //                canvasView.InvalidateSurface();
            //                Indicator.IsRunning = false;
            //                EditGrid.IsEnabled = true;
            //                return;
            //            case FilterType.YellowClassic1:
            //                paint.ColorFilter = FilterMatrix.GetYellowClassicFilterMatrix1(currentFilterOption);
            //                canvasView.InvalidateSurface();
            //                Indicator.IsRunning = false;
            //                EditGrid.IsEnabled = true;
            //                return;
            //            case FilterType.YellowClassic2:
            //                paint.ColorFilter = FilterMatrix.GetYellowClassicFilterMatrix2(currentFilterOption);
            //                canvasView.InvalidateSurface();
            //                Indicator.IsRunning = false;
            //                EditGrid.IsEnabled = true;
            //                return;
            //            default:
            //                paint.ColorFilter = FilterMatrix.GetColorFilterMatrix(currentFilterOption);
            //                canvasView.InvalidateSurface();
            //                Indicator.IsRunning = false;
            //                EditGrid.IsEnabled = true;
            //                return;
            //        }
            //    }

            //}));
        }

        private async void SaveFilter_Tapped(object sender, EventArgs e)
        {
            //await ExportSettingPopup.Instance.Show(editImageModel: EditImageModel);
        }
        #endregion

        private void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            SKImageInfo info = e.Info;
            SKSurface surface = e.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear();

            Path = ReviewPageViewModel.Instance.GetPath();
            if (bitmap == null)
            {
                using (Stream stream = File.OpenRead(Path))
                {
                    bitmap = SKBitmap.Decode(stream);
                    originalBitmap = bitmap;
                    var orientation = CheckOrigintation(Path);
                    if (orientation == SKEncodedOrigin.BottomRight || orientation == SKEncodedOrigin.RightTop || orientation == SKEncodedOrigin.LeftBottom)
                    {
                        SKBitmap orientedWExif = HandleOrientation(bitmap, orientation);

                        SKBitmap newBitmap = new SKBitmap(orientedWExif.Width, orientedWExif.Height);
                        using (SKCanvas newCanvas = new SKCanvas(newBitmap))
                        {
                            newCanvas.Clear();
                            newCanvas.DrawBitmap(bitmap: orientedWExif, 0, 0);
                        }
                        currentBitmap = newBitmap;
                        bitmap = newBitmap;
                        originalBitmap = newBitmap;
                    }
                }
            }
            switch (filterTypeChosen)
            {
                case FilterType.ColorFilter:
                    canvas.DrawBitmap(originalBitmap, info.Rect, BitmapStretch.Uniform, paint: paint);
                    break;
                default:
                    canvas.DrawBitmap(bitmap, info.Rect, BitmapStretch.Uniform, paint: paint);
                    break;
            }
            EditImageModel.ColorContrast = contrast;
            EditImageModel.ColorBrightness = brightness;
            EditImageModel.ColorWhiteBalance = whitebalance;
            EditImageModel.GrayBrightness = grayscale_brightness;
            EditImageModel.GrayContrast = grayscale_brightness;
            EditImageModel.BlackWhiteContrast = blackwhite_contrast;
            EditImageModel.FilterTypeChosen = filterType;
        }

        private void OnDrawViewPaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            SKImageInfo info = e.Info;
            SKSurface surface = e.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear();
            foreach (FingerPaintPolyline polyline in completedPolylines)
            {
                drawpaint.Color = polyline.StrokeColor;
                drawpaint.BlendMode = polyline.BlendMode;
                drawpaint.StrokeWidth = polyline.StrokeWidth;
                canvas.DrawPath(polyline.Path, drawpaint);
            }

            foreach (FingerPaintPolyline polyline in inProgressPolylines.Values)
            {
                drawpaint.Color = polyline.StrokeColor;
                drawpaint.BlendMode = polyline.BlendMode;
                drawpaint.StrokeWidth = polyline.StrokeWidth;
                canvas.DrawPath(polyline.Path, drawpaint);
            }
        }
    }
}