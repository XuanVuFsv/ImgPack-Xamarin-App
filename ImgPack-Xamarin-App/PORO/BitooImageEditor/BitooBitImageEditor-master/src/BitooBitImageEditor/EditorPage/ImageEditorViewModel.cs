using BitooBitImageEditor.Croping;
using BitooBitImageEditor.Helper;
using BitooBitImageEditor.ManipulationBitmap;
using BitooBitImageEditor.TouchTracking;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Essentials;
using BitooBitImageEditor.Helper.Controls;

namespace BitooBitImageEditor.EditorPage
{
    internal class ImageEditorViewModel : BaseNotifier, IDisposable
    {
        private TouchManipulationBitmap currentTextBitmap = null;
        internal ImageCropperCanvasView cropperCanvas;
        internal TouchManipulationCanvasView mainCanvas;

        public bool IsRunning { get; set; }
        public bool IsEnable { get; set; }

        internal ImageEditorViewModel(SKBitmap bitmap, ImageEditorConfig config, int n)
        {
            Config = config;
            cropperCanvas = new ImageCropperCanvasView(bitmap, config.CropAspectRatio);
            mainCanvas = new TouchManipulationCanvasView(config);
            mainCanvas.AddBitmapToCanvas(bitmap.Copy(), BitmapType.Main);
            mainCanvas.TextBitmapClicked += MainCanvas_TextBitmapClicked;
            mainCanvas.TrashEnabled += MainCanvas_TrashVisebled;
            ColorCollect = SkiaHelper.GetColors();
            CropCollect = CropItem.GetCropItems(config.CanChangeCropAspectRatio);
            Message = config?.LoadingText;
            MessagingCenter.Subscribe<Xamarin.Forms.Application>(this, "BBDroidBackButton", (sender) => OnBackPressed());

            if (n == 1)
            {
                CurrentEditType = ImageEditType.CropRotate;
            }
            else if(n == 2)
            {
                CurrentEditType = ImageEditType.Stickers;
            }
            else
            {
                CurrentEditType = ImageEditType.SelectType;
            }
            IsRunning = false;
            IsEnable = true;
        }

        public void OnBackPressed()
        {
            if (CurrentEditType != ImageEditType.SelectType)
                CurrentEditType = ImageEditType.SelectType;
            else
                EditFinish(false);
        }

        public bool CropVisible => CurrentEditType == ImageEditType.CropRotate;
        public bool MainVisible => !CropVisible;
        public bool TextVisible => CurrentEditType == ImageEditType.Text;
        public bool StickersVisible => CurrentEditType == ImageEditType.Stickers;
        public bool PaintVisible => CurrentEditType == ImageEditType.Paint && !IsMoved;
        public bool InfoVisible => CurrentEditType == ImageEditType.Info;
        public bool ButtonsVisible => CurrentEditType == ImageEditType.SelectType && !IsMoved;
        public bool TrashVisible { get; private set; }
        public bool TrashBigVisible { get; private set; }
        public bool IsMoved { get; private set; }


        public ImageEditorConfig Config { get; private set; }
        public ImageEditType CurrentEditType { private set; get; } = ImageEditType.SelectType;
        public Color CurrentColor { get; set; } = Color.White;
        public Color Color { get; set; } = Color.White;
        public SKBlendMode SKBlendMode { get; set; } = SKBlendMode.SrcOver;
        public Color SKColor { get; set; } = Color.White;
        public string PensixeText { get; set; } = "10";
        public double PensizeValue { get; set; } = 10;
        public string CurrentText { set; get; } = "";
        public bool CurrentTextIsFill { set; get; } = false;
        public string Message { private set; get; } = "";
        public ObservableCollection<Color> ColorCollect { get; private set; }
        public ObservableCollection<CropItem> CropCollect { get; private set; }


        public ICommand ApplyChangesCommand => new Command<string>((value) =>
        {
            if (!string.IsNullOrWhiteSpace(value) && value.ToLower() == "apply")
            {
                switch (CurrentEditType)
                {
                    case ImageEditType.Text:
                        {
                            if (currentTextBitmap == null)
                                mainCanvas.AddBitmapToCanvas(CurrentText, CurrentColor.ToSKColor(), CurrentTextIsFill);
                            else
                            {
                                currentTextBitmap.Bitmap?.Dispose();
                                currentTextBitmap.Bitmap = null;
                                currentTextBitmap.Bitmap = SKBitmapBuilder.FromText(CurrentText, CurrentColor.ToSKColor(), CurrentTextIsFill);
                                currentTextBitmap.Text = CurrentText;
                                currentTextBitmap.IsHide = false;
                                currentTextBitmap.Color = CurrentColor.ToSKColor();
                                mainCanvas?.InvalidateSurface();
                            }

                            currentTextBitmap = null;
                            CurrentText = "";
                        }
                        break;
                    case ImageEditType.CropRotate:
                        mainCanvas.AddBitmapToCanvas(cropperCanvas.CroppedBitmap, BitmapType.Main);
                        break;
                }
            }

            CurrentEditType = ImageEditType.SelectType;
        });

        public ICommand CancelCommand => new Command(() =>
        {
            if (CurrentEditType == ImageEditType.Paint)
                mainCanvas.DeleteEndPath();
        });

        public ICommand SelectItemCommand => new Command<object>((valueObj) =>
        {
            switch (valueObj)
            {
                case ImageEditType value:
                    CurrentEditType = value;
                    break;
                case Color value:
                    CurrentColor = value;
                    break;
                case CropItem value:
                    cropperCanvas.SetAspectRatio(value);
                    break;
                case SKBitmapImageSource value:
                    mainCanvas.AddBitmapToCanvas(value, BitmapType.Stickers);
                    CurrentEditType = ImageEditType.SelectType;
                    break;
                default:
                    CurrentEditType = ImageEditType.SelectType;
                    break;
            }
        });

        private bool lockFinish = false;
        public ICommand EditFinishCommand => new Command<string>((value) => EditFinish(!string.IsNullOrWhiteSpace(value) && value.ToLower() == "save"));

        public ICommand ChangeTextTypeCommand => new Command<string>((value) => { CurrentTextIsFill = !CurrentTextIsFill; });

        public ICommand SaveCommand => new Command<string>(async (value) =>
        {
            CurrentEditType = ImageEditType.Info;

            IsRunning = true;
            IsEnable = false;
            await Task.Run(() =>
            {
                var bitmap = mainCanvas.EditedBitmap;
                Device.BeginInvokeOnMainThread(async () =>
                {
                    bitmap.Dispose();
                    bitmap = null;
                    GC.Collect();

                    EditFinish(true);
                    CurrentEditType = ImageEditType.SelectType;

                    IsRunning = false;
                    IsEnable = true;
                });
            });
      
            

            //if (await ImageEditor.Instance.SaveImage(SkiaHelper.SKBitmapToBytes(bitmap), $"img{DateTime.Now:dd.MM.yyyy HH-mm-ss}.png"))
            //    Message = Config?.SuccessSaveText;
            //else
            //    Message = Config?.ErrorSaveText;
            //bitmap.Dispose();
            //bitmap = null;
            //GC.Collect();

            //int time = (int)(Message?.Length * 75);
            //await Task.Delay(time >= 1500 ? time : 1500);
            //Message = Config?.LoadingText;
            //EditFinish(true);
            //CurrentEditType = ImageEditType.SelectType;
        });

        private void EditFinish(bool isSave)
        {
            if (!lockFinish)
            {
                lockFinish = true;
                ImageEditor.Instance.SetImage(isSave ? mainCanvas.EditedBitmap : null);
            }
        }

        internal void OnTouchEffectTouchAction(object sender, TouchActionEventArgs args)
        {
            IsMoved = Device.RuntimePlatform != Device.UWP && (args.Type == TouchActionType.Moved);

            if (CurrentEditType != ImageEditType.CropRotate)
                mainCanvas?.OnTouchEffectTouchAction(args, CurrentEditType, CurrentColor.ToSKColor());
            else
                cropperCanvas?.OnTouchEffectTouchAction(args);
        }

        private void MainCanvas_TextBitmapClicked(TouchManipulationBitmap value)
        {
            CurrentColor = value?.Color.ToFormsColor() ?? Color.Black;
            CurrentText = value?.Text ?? "";
            CurrentEditType = ImageEditType.Text;
            currentTextBitmap = value;
        }

        private void MainCanvas_TrashVisebled(bool arg1, bool arg2, bool arg3)
        {
            if (CurrentEditType == ImageEditType.SelectType)
            {
                TrashVisible = arg1;
                TrashBigVisible = arg2;
                if (arg3)
                    HapticFeedback.Perform();
            }
        }

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    MessagingCenter.Unsubscribe<Xamarin.Forms.Application>(this, "BBDroidBackButton");
                    Config = null;
                    ColorCollect?.Clear();
                    ColorCollect = null;
                    CurrentText = null;
                }

                if (CropCollect?.Count > 0)
                    for (int i = 0; i < CropCollect.Count; i++)
                        CropCollect[i] = null;

                CropCollect?.Clear();
                CropCollect = null;

                cropperCanvas?.Dispose();
                mainCanvas?.Dispose();
                currentTextBitmap?.Bitmap?.Dispose();
                if (currentTextBitmap != null)
                    currentTextBitmap.Bitmap = null;
                mainCanvas = null;
                cropperCanvas = null;
                currentTextBitmap = null;
                disposedValue = true;
            }
        }

        ~ImageEditorViewModel()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
        //private TouchManipulationBitmap currentTextBitmap = null;
        //internal ImageCropperCanvasView cropperCanvas;
        //internal TouchManipulationCanvasView mainCanvas;

        //internal ImageEditorViewModel(SKBitmap bitmap, ImageEditorConfig config, int n)
        //{
        //    Config = config;
        //    cropperCanvas = new ImageCropperCanvasView(bitmap, config.CropAspectRatio);
        //    mainCanvas = new TouchManipulationCanvasView(config);
        //    mainCanvas.AddBitmapToCanvas(bitmap.Copy(), BitmapType.Main);
        //    mainCanvas.TextBitmapClicked += MainCanvas_TextBitmapClicked;
        //    mainCanvas.TrashEnabled += MainCanvas_TrashVisebled;
        //    ColorCollect = SkiaHelper.GetColors();
        //    CropCollect = CropItem.GetCropItems(config.CanChangeCropAspectRatio);
        //    Message = config?.LoadingText;
        //    MessagingCenter.Subscribe<Xamarin.Forms.Application>(this, "BBDroidBackButton", (sender) => OnBackPressed());
        //    if (n == 1)
        //    {
        //        CurrentEditType = ImageEditType.CropRotate;
        //    }
        //    else
        //    {
        //        CurrentEditType = ImageEditType.SelectType;
        //    }
        //}



        //public bool CropVisible => CurrentEditType == ImageEditType.CropRotate;
        //public bool MainVisible => !CropVisible;
        //public bool TextVisible => CurrentEditType == ImageEditType.Text;
        //public bool StickersVisible => CurrentEditType == ImageEditType.Stickers;
        //public bool PaintVisible => CurrentEditType == ImageEditType.Paint && !IsMoved;
        //public bool InfoVisible => CurrentEditType == ImageEditType.Info;
        //public bool ButtonsVisible => CurrentEditType == ImageEditType.SelectType && !IsMoved;
        //public bool TrashVisible { get; private set; }
        //public bool TrashBigVisible { get; private set; }
        //public bool IsMoved { get; private set; }


        //public ImageEditorConfig Config { get; private set; }
        //public ImageEditType CurrentEditType { private set; get; } = ImageEditType.SelectType;
        //public Color CurrentColor { get; set; } = Color.White;
        //public Color Color { get; set; } = Color.White;
        //public SKBlendMode SKBlendMode { get; set; } = SKBlendMode.SrcOver;
        //public Color SKColor { get; set; } = Color.White;
        //public string PensixeText { get; set; } = "10";
        //public double PensizeValue { get; set; } = 10;
        //public string CurrentText { set; get; } = "";
        //public bool CurrentTextIsFill { set; get; } = false;
        //public string Message { private set; get; } = "";
        //public ObservableCollection<Color> ColorCollect { get; private set; }
        //public ObservableCollection<CropItem> CropCollect { get; private set; }



        //public ICommand ApplyChangesCommand => new Command<string>((value) =>
        //{
        //    if (!string.IsNullOrWhiteSpace(value) && value.ToLower() == "apply")
        //    {
        //        switch (CurrentEditType)
        //        {
        //            case ImageEditType.Text:
        //                {
        //                    if (currentTextBitmap == null)
        //                        mainCanvas.AddBitmapToCanvas(CurrentText, CurrentColor.ToSKColor(), CurrentTextIsFill);
        //                    else
        //                    {
        //                        currentTextBitmap.Bitmap?.Dispose();
        //                        currentTextBitmap.Bitmap = null;
        //                        currentTextBitmap.Bitmap = SKBitmapBuilder.FromText(CurrentText, CurrentColor.ToSKColor(), CurrentTextIsFill);
        //                        currentTextBitmap.Text = CurrentText;
        //                        currentTextBitmap.IsHide = false;
        //                        currentTextBitmap.Color = CurrentColor.ToSKColor();
        //                        mainCanvas?.InvalidateSurface();
        //                    }

        //                    currentTextBitmap = null;
        //                    CurrentText = "";
        //                }
        //                break;
        //            case ImageEditType.CropRotate:
        //                mainCanvas.AddBitmapToCanvas(cropperCanvas.CroppedBitmap, BitmapType.Main);
        //                break;
        //        }
        //    }

        //    CurrentEditType = ImageEditType.SelectType;
        //});

        //public ICommand CancelCommand => new Command(() =>
        //{
        //    if (CurrentEditType == ImageEditType.Paint)
        //        mainCanvas.DeleteEndPath();
        //});

        //public ICommand SelectItemCommand => new Command<object>((valueObj) =>
        //{
        //    switch (valueObj)
        //    {
        //        case ImageEditType value:
        //            CurrentEditType = value;
        //            break;
        //        case Color value:
        //            CurrentColor = value;
        //            break;
        //        case CropItem value:
        //            cropperCanvas.SetAspectRatio(value);
        //            break;
        //        case SKBitmapImageSource value:
        //            mainCanvas.AddBitmapToCanvas(value, BitmapType.Stickers);
        //            CurrentEditType = ImageEditType.SelectType;
        //            break;
        //        default:
        //            CurrentEditType = ImageEditType.SelectType;
        //            break;
        //    }
        //});

        //private bool lockFinish = false;
        //public ICommand EditFinishCommand => new Command<string>((value) => EditFinish(!string.IsNullOrWhiteSpace(value) && value.ToLower() == "save"));

        //public ICommand ChangeTextTypeCommand => new Command<string>((value) => { CurrentTextIsFill = !CurrentTextIsFill; });

        //public ICommand SaveCommand => new Command<string>(async (value) =>
        //{
        //    CurrentEditType = ImageEditType.Info;

        //    var bitmap = mainCanvas.EditedBitmap;

        //    //if (await ImageEditor.Instance.SaveImage(SkiaHelper.SKBitmapToBytes(bitmap), $"img{DateTime.Now:dd.MM.yyyy HH-mm-ss}.png"))
        //    //    Message = Config?.SuccessSaveText;
        //    //else
        //    //    Message = Config?.ErrorSaveText;
        //    bitmap.Dispose();
        //    bitmap = null;
        //    GC.Collect();

        //    //int time = (int)(Message?.Length * 75);
        //    //await Task.Delay(time >= 1500 ? time : 1500);
        //    //Message = Config?.LoadingText;
        //    CurrentEditType = ImageEditType.SelectType;
        //});

        //private void EditFinish(bool isSave)
        //{
        //    if (!lockFinish)
        //    {
        //        lockFinish = true;
        //        ImageEditor.Instance.SetImage(isSave ? mainCanvas.EditedBitmap : null);
        //    }
        //}

        //public void OnBackPressed()
        //{
        //    if (CurrentEditType != ImageEditType.SelectType)
        //        CurrentEditType = ImageEditType.SelectType;
        //    else
        //        EditFinish(false);
        //}

        //internal void OnTouchEffectTouchAction(object sender, TouchActionEventArgs args)
        //{
        //    IsMoved = Device.RuntimePlatform != Device.UWP && (args.Type == TouchActionType.Moved);

        //    if (CurrentEditType != ImageEditType.CropRotate)
        //        mainCanvas?.OnTouchEffectTouchAction(args, CurrentEditType, CurrentColor.ToSKColor());
        //    else
        //        cropperCanvas?.OnTouchEffectTouchAction(args);
        //}

        //private void MainCanvas_TextBitmapClicked(TouchManipulationBitmap value)
        //{
        //    CurrentColor = value?.Color.ToFormsColor() ?? Color.Black;
        //    CurrentText = value?.Text ?? "";
        //    CurrentEditType = ImageEditType.Text;
        //    currentTextBitmap = value;
        //}

        //private void MainCanvas_TrashVisebled(bool arg1, bool arg2, bool arg3)
        //{
        //    if (CurrentEditType == ImageEditType.SelectType)
        //    {
        //        TrashVisible = arg1;
        //        TrashBigVisible = arg2;
        //        if (arg3)
        //            HapticFeedback.Perform();
        //    }
        //}

        //#region IDisposable Support
        //private bool disposedValue = false;

        //protected virtual void Dispose(bool disposing)
        //{
        //    if (!disposedValue)
        //    {
        //        if (disposing)
        //        {
        //            MessagingCenter.Unsubscribe<Xamarin.Forms.Application>(this, "BBDroidBackButton");
        //            Config = null;
        //            ColorCollect?.Clear();
        //            ColorCollect = null;
        //            CurrentText = null;
        //        }

        //        if (CropCollect?.Count > 0)
        //            for (int i = 0; i < CropCollect.Count; i++)
        //                CropCollect[i] = null;

        //        CropCollect?.Clear();
        //        CropCollect = null;

        //        cropperCanvas?.Dispose();
        //        mainCanvas?.Dispose();
        //        currentTextBitmap?.Bitmap?.Dispose();
        //        if (currentTextBitmap != null)
        //            currentTextBitmap.Bitmap = null;
        //        mainCanvas = null;
        //        cropperCanvas = null;
        //        currentTextBitmap = null;
        //        disposedValue = true;
        //    }
        //}

        //~ImageEditorViewModel()
        //{
        //    Dispose(false);
        //}

        //public void Dispose()
        //{
        //    Dispose(true);
        //    GC.SuppressFinalize(this);
        //}
        //#endregion

    }
}
