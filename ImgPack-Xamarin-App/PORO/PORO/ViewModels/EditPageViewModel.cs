using PORO.Enums;
using PORO.Models;
using PORO.Services;
using PORO.Services.Database;
using PORO.Untilities;
using PORO.ViewModels.Base;
using PORO.Views;
using PORO.Views.Popups;
using Prism.Navigation;
using SkiaSharp;
using Stormlion.ImageCropper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;
using static Stormlion.ImageCropper.ImageCropper;

namespace PORO.ViewModels
{
    public class EditPageViewModel : BaseViewModel
    {
        //#region Properties
        //private ImageSource _imageReview;
        //public ImageSource ImageEditSource
        //{
        //    get => _imageReview;
        //    set => SetProperty(ref _imageReview, value);
        //}
        //private ImageSource _imageEdited;
        //public ImageSource ImageEdited
        //{
        //    get => _imageEdited;
        //    set => SetProperty(ref _imageEdited, value);
        //}
        //public string path { get; set; }
        //private string cropPath { get; set; }
        //public string pathOri { get; set; }

        //#region Visible
        //private bool _filterVisile;
        //public bool FilterVisible
        //{
        //    get => _filterVisile;
        //    set => SetProperty(ref _filterVisile, value);
        //}
        //private bool _colorVisible;
        //public bool ColorVisible
        //{
        //    get => _colorVisible;
        //    set => SetProperty(ref _colorVisible, value);
        //}
        //private bool _cropVisible;
        //public bool CropVisible
        //{
        //    get => _cropVisible;
        //    set => SetProperty(ref _cropVisible, value);
        //}
        //private bool _iconVisible;
        //public bool IconVisible
        //{
        //    get => _iconVisible;
        //    set => SetProperty(ref _iconVisible, value);
        //}
        //private bool _contrastEnable;
        //public bool ContrastEnable
        //{
        //    get => _contrastEnable;
        //    set => SetProperty(ref _contrastEnable, value);
        //}
        //#endregion

        //private int _whiteBalanceValue = 0;
        //public int WhiteBalanceValue
        //{
        //    get => _whiteBalanceValue;
        //    set
        //    {
        //        SetProperty(ref _whiteBalanceValue, value);
        //        if (path == null)
        //        {
        //            WhiteBalanceValueText = WhiteBalanceValue.ToString();
        //        }
        //        else
        //        {
        //            WhiteBalanceValueText = WhiteBalanceValue.ToString();
        //        }
        //    }
        //}
        //private int _contrastValue = 0;
        //public int ContrastValue
        //{
        //    get => _contrastValue;
        //    set
        //    {
        //        SetProperty(ref _contrastValue, value);
        //        if (path == null)
        //        {
        //            ContrastValueText = ContrastValue.ToString();
        //        }
        //        else
        //        {
        //            ContrastValueText = ContrastValue.ToString();
        //        }
        //    }
        //}
        //private int _colorValue = 0;
        //public int ColorValue
        //{
        //    get => _colorValue;
        //    set
        //    {
        //        SetProperty(ref _colorValue, value);
        //        if (path == null)
        //        {
        //            ColorValueText = ColorValue.ToString();
        //        }
        //        else
        //        {
        //            ColorValueText = ColorValue.ToString();
        //        }

        //    }
        //}
        //private string _whiteBalanceValueText;
        //public string WhiteBalanceValueText
        //{
        //    get => _whiteBalanceValueText;
        //    set => SetProperty(ref _whiteBalanceValueText, value);
        //}
        //private string _colorValueText;
        //public string ColorValueText
        //{
        //    get => _colorValueText;
        //    set => SetProperty(ref _colorValueText, value);
        //}
        //private string _contrastValueText;
        //public string ContrastValueText
        //{
        //    get => _contrastValueText;
        //    set => SetProperty(ref _contrastValueText, value);
        //}
        //private DatabaseModel dataModel = new DatabaseModel();
        //private bool _imageConvert;
        //public bool ImageConvert
        //{
        //    get => _imageConvert;
        //    set => SetProperty(ref _imageConvert, value);
        //}
        //private bool _checkStart = true;
        //public bool CheckStart
        //{
        //    get => _checkStart;
        //    set => SetProperty(ref _checkStart, value);
        //}
        //IEditService editService = DependencyService.Get<IEditService>();
        //#endregion
        //public EditPageViewModel(INavigationService navigationService)
        //    : base(navigationService)
        //{
        //    FilterCommand = new Command(ExcuteCanvas);
        //    ColorCommand = new Command(ExcuteFilter);
        //    CropCommand = new Command(ExcuteCrop);

        //    BlackWhiteCommand = new Command(ExcuteBlackWhite);
        //    OriginalCommand = new Command(ExcuteOriginal);
        //    GrayScaleCommand = new Command(ExcuteGrayScale);
        //    InvertCommand = new Command(ExcuteInvert);
        //    SwapCommand = new Command(ExcuteSwap);
        //    SepiaCommand = new Command(ExcuteSepia);
        //    PolaroidCommand = new Command(ExcutePolaroid);
        //    SunsetCommand = new Command(ExcuteSunset);

        //    WhiteBalanceValueText = WhiteBalanceValue.ToString();
        //    WhiteBalanceStopCommand = new Command(ExcuteWhiteBalanceStop);
        //    ContrastValueText = ContrastValue.ToString();
        //    ContrastStopCommand = new Command(ExcuteContrastStop);
        //    ColorValueText = ColorValue.ToString();
        //    BrightnessStopCommand = new Command(ExcuteBrightnessStop);

        //    DoneCommand = new Command(ExcuteDone);
        //    CancelCommand = new Command(ExcuteCancel);
        //}

        //#region Navigation
        //public async override void OnNavigatedTo(INavigationParameters parameters)
        //{
        //    ContrastEnable = true;
        //    ColorVisible = true;
        //    FilterVisible = false;
        //    CropVisible = false;
        //    IconVisible = true;
        //    ImageConvert = true;
        //    var croppedPath = Preferences.Get("croppedPath", null);
        //    if (croppedPath != null)
        //    {
        //        Preferences.Set("croppedPath", null);
        //    }
        //    if (parameters != null)
        //    {
        //        if (parameters.ContainsKey(ParamKeys.ImageToEdit.ToString()))
        //        {
        //            dataModel = (DatabaseModel)parameters[ParamKeys.ImageToEdit.ToString()];
        //            path = dataModel.filepath;
        //            ImageEditSource = ImageSource.FromFile(path);
        //            CheckStart = false;
        //        }
        //    }
        //    //if (path != null)
        //    //{
        //    //    Reset();
        //    //}
        //}
        //#endregion
        //#region Filter
        //public ICommand FilterCommand { get; set; }
        //public void ExcuteCanvas()
        //{
        //    ColorVisible = false;
        //    FilterVisible = true;
        //    CropVisible = false;
        //    cropPath = Preferences.Get("croppedPath", null);
        //    if (cropPath != null)
        //    {
        //        path = cropPath;
        //    }
        //}
        //public ICommand ColorCommand { get; set; }
        //public void ExcuteFilter()
        //{
        //    ColorVisible = true;
        //    FilterVisible = false;
        //    CropVisible = false;
        //    cropPath = Preferences.Get("croppedPath", null);
        //    if (cropPath != null)
        //    {
        //        path = cropPath;
        //    }
        //}
        //public ICommand CropCommand { get; set; }
        //public async void ExcuteCrop()
        //{
        //    ColorVisible = false;
        //    FilterVisible = false;
        //    CropVisible = true;

        //    editService.Crop();
        //    if (pathOri != null)
        //    {
        //        new ImageCropper()
        //        {
        //            PageTitle = "Classic",
        //            Success = (croppedPath) =>
        //            {
        //                Device.BeginInvokeOnMainThread(() =>
        //                {
        //                    ContrastValue = 0;
        //                    WhiteBalanceValue = 0;
        //                    ColorValue = 0;
        //                    ImageConvert = false;
        //                    ImageEditSource = ImageSource.FromFile(croppedPath);
        //                    Preferences.Set("croppedPath", croppedPath);
        //                    cropPath = croppedPath;
        //                    pathOri = null;
        //                });
        //            }
        //        }.Show(EditPage.Instance, pathOri);
        //    }
        //    else
        //    {
        //        new ImageCropper()
        //        {
        //            PageTitle = "Classic",
        //            Success = (croppedPath) =>
        //            {
        //                Device.BeginInvokeOnMainThread(() =>
        //                {
        //                    ContrastValue = 0;
        //                    WhiteBalanceValue = 0;
        //                    ColorValue = 0;
        //                    ImageConvert = false;
        //                    ImageEditSource = ImageSource.FromFile(croppedPath);
        //                    Preferences.Set("croppedPath", croppedPath);
        //                    cropPath = croppedPath;
        //                });
        //            },
        //            Faiure = (ResultErrorType resultErrorType) =>
        //            {
        //                Console.WriteLine("Path error");
        //            }
        //        }.Show(EditPage.Instance, path);
        //    }
        //}
        //#endregion

        //#region Original
        //public ICommand OriginalCommand { get; set; }
        //public void ExcuteOriginal()
        //{
        //    ContrastValue = 0;
        //    WhiteBalanceValue = 0;
        //    ColorValue = 0;
        //    ImageConvert = false;
        //    editService.Original(cropPath);
        //    ImageEdited = ImageSource.FromFile(path);
        //    pathOri = path;
        //    cropPath = null;
        //}
        //#endregion

        //#region BackWhite
        //public ICommand BlackWhiteCommand { get; set; }
        //public void ExcuteBlackWhite()
        //{
        //    ImageConvert = false;
        //    var value = editService.BlackWhite(path);
        //    pathOri = value;
        //    ImageEditSource = ImageSource.FromFile(pathOri);
        //    cropPath = null;
        //}
        //#endregion

        //#region GrayScale
        //public ICommand GrayScaleCommand { get; set; }
        //public void ExcuteGrayScale()
        //{
        //    ContrastValue = 0;
        //    WhiteBalanceValue = 0;
        //    ColorValue = 0;
        //    ImageConvert = false;
        //    var value = editService.GreyScale(path);
        //    pathOri = value;
        //    ImageEditSource = ImageSource.FromFile(pathOri);
        //    cropPath = null;
        //}
        //#endregion

        //#region Invert
        //public ICommand InvertCommand { get; set; }
        //public void ExcuteInvert()
        //{
        //    ContrastValue = 0;
        //    WhiteBalanceValue = 0;
        //    ColorValue = 0;
        //    ImageConvert = false;
        //    var value = editService.Invert(path);
        //    pathOri = value;
        //    ImageEditSource = ImageSource.FromFile(pathOri);
        //    cropPath = null;
        //}
        //#endregion

        //#region Swap
        //public ICommand SwapCommand { get; set; }
        //public void ExcuteSwap()
        //{
        //    ContrastValue = 0;
        //    WhiteBalanceValue = 0;
        //    ColorValue = 0;
        //    ImageConvert = false;
        //    var value = editService.Swap(path);
        //    pathOri = value;
        //    ImageEditSource = ImageSource.FromFile(pathOri);
        //    cropPath = null;
        //}
        //#endregion

        //#region Sepia
        //public ICommand SepiaCommand { get; set; }
        //public void ExcuteSepia()
        //{
        //    ContrastValue = 0;
        //    WhiteBalanceValue = 0;
        //    ColorValue = 0;
        //    ImageConvert = false;
        //    var value = editService.Sepia(path);
        //    pathOri = value;
        //    ImageEditSource = ImageSource.FromFile(pathOri);
        //    cropPath = null;
        //}
        //#endregion

        //#region Polaroid
        //public ICommand PolaroidCommand { get; set; }
        //public void ExcutePolaroid()
        //{
        //    ContrastValue = 0;
        //    WhiteBalanceValue = 0;
        //    ColorValue = 0;
        //    ImageConvert = false;
        //    var value = editService.Polaroid(path);
        //    pathOri = value;
        //    ImageEditSource = ImageSource.FromFile(pathOri);
        //    cropPath = null;
        //}
        //#endregion

        //#region Sunset
        //public ICommand SunsetCommand { get; set; }
        //public void ExcuteSunset()
        //{
        //    ContrastValue = 0;
        //    WhiteBalanceValue = 0;
        //    ColorValue = 0;
        //    ImageConvert = false;
        //    var value = editService.Sunset(path);
        //    pathOri = value;
        //    ImageEditSource = ImageSource.FromFile(pathOri);
        //    cropPath = null;
        //}
        //#endregion

        //#region ImageEditColection
        //public void ImageEditContrast()
        //{
        //    ImageConvert = false;
        //    ContrastValueText = ContrastValue.ToString();
        //    pathOri = editService.ContrastChange(ContrastValue, path);
        //    ImageEditSource = ImageSource.FromFile(pathOri);
        //    cropPath = null;
        //}
        //public ICommand ContrastStopCommand { get; set; }
        //public void ExcuteContrastStop()
        //{
        //    ImageEditContrast();
        //}

        //public ICommand BrightnessStopCommand { get; set; }
        //public void ExcuteBrightnessStop()
        //{
        //    ImageEditBrightness();
        //}
        //public void ImageEditBrightness()
        //{
        //    ImageConvert = false;
        //    ColorValueText = ColorValue.ToString();
        //    pathOri = editService.Brightness(ColorValue, path);
        //    ImageEditSource = ImageSource.FromFile(pathOri);
        //    cropPath = null;
        //}

        //public ICommand WhiteBalanceStopCommand { get; set; }
        //public void ExcuteWhiteBalanceStop()
        //{
        //    ImageEditWhiteBalance();
        //}
        //public void ImageEditWhiteBalance()
        //{
        //    ImageConvert = false;
        //    WhiteBalanceValueText = WhiteBalanceValue.ToString();
        //    pathOri = editService.Whitebalance(WhiteBalanceValue, path);
        //    ImageEditSource = ImageSource.FromFile(pathOri);
        //    cropPath = null;
        //}
        //#endregion

        //#region Reset
        //public void Reset()
        //{
        //    editService.ResetService(path);
        //}
        //#endregion

        //#region Cancel
        //public ICommand CancelCommand { get; set; }
        //public async void ExcuteCancel()
        //{
        //    await Navigation.GoBackAsync();
        //}
        //#endregion

        //#region Done 
        //public ICommand DoneCommand { get; set; }
        //public async void ExcuteDone()
        //{
        //    var filePath = Preferences.Get("croppedPath", null);
        //    if (pathOri != null)
        //    {
        //        path = editService.SavePhoto(path);
        //    }
        //    else if (cropPath != null)
        //    {
        //        path = cropPath;
        //    }
        //    else
        //    {
        //        path = dataModel.filepath;
        //    }
        //    dataModel.filepath = path;
        //    //Database database = new Database();
        //    //database.Update(dataModel);

        //    NavigationParameters param = new NavigationParameters
        //    {
        //        {ParamKeys.DataModel.ToString(), dataModel}
        //    };
        //    await Navigation.GoBackAsync(param);
        //}
        //#endregion
        #region Properties
        public static EditPageViewModel Instance { get; private set; }
        IEditService editService = DependencyService.Get<IEditService>();
        IFileService fileService = DependencyService.Get<IFileService>();
        private ImageSource _imageEdited;
        public ImageSource ImageEdited
        {
            get => _imageEdited;
            set => SetProperty(ref _imageEdited, value);
        }
        private ImageSource _imageCrop;
        public ImageSource ImageCrop
        {
            get => _imageCrop;
            set => SetProperty(ref _imageCrop, value);
        }
        public string path { get; set; }
        private string cropPath { get; set; }
        private string drawPath { get; set; }
        public string pathOri { get; set; }
        public byte[] ImageFromFile { get; set; }
        public byte[] ImageCurrentSize { get; set; }

        #region Visible
        private bool _canvasVisile;
        public bool CanvasVisible
        {
            get => _canvasVisile;
            set => SetProperty(ref _canvasVisile, value);
        }
        private bool _studioVisible;
        public bool StudioVisible
        {
            get => _studioVisible;
            set => SetProperty(ref _studioVisible, value);
        }
        private bool _cropVisible;
        public bool CropVisible
        {
            get => _cropVisible;
            set => SetProperty(ref _cropVisible, value);
        }
        private bool _signatureVisible;
        public bool SignatureVisible
        {
            get => _signatureVisible;
            set => SetProperty(ref _signatureVisible, value);
        }
        private bool _addTextVisible;
        public bool AddTextVisible
        {
            get => _addTextVisible;
            set => SetProperty(ref _addTextVisible, value);
        }
        private bool _drawVisible;
        public bool DrawVisible
        {
            get => _drawVisible;
            set => SetProperty(ref _drawVisible, value);
        }
        private bool _iconVisible;
        public bool IconVisible
        {
            get => _iconVisible;
            set => SetProperty(ref _iconVisible, value);
        }
        private bool _contrastEnable;
        public bool ContrastEnable
        {
            get => _contrastEnable;
            set => SetProperty(ref _contrastEnable, value);
        }
        #endregion

        #region Filter Value
        private int _whiteBalanceValue = 0;
        public int WhiteBalanceValue
        {
            get => _whiteBalanceValue;
            set
            {
                SetProperty(ref _whiteBalanceValue, value);
                if (path == null)
                {
                    WhiteBalanceValueText = WhiteBalanceValue.ToString();
                }
                else
                {
                    WhiteBalanceValueText = WhiteBalanceValue.ToString();
                }
            }
        }
        private int _contrastValue = 0;
        public int ContrastValue
        {
            get => _contrastValue;
            set
            {
                SetProperty(ref _contrastValue, value);
                if (path == null)
                {
                    ContrastValueText = ContrastValue.ToString();
                }
                else
                {
                    ContrastValueText = ContrastValue.ToString();
                }
            }
        }
        private int _colorValue = 0;
        public int ColorValue
        {
            get => _colorValue;
            set
            {
                SetProperty(ref _colorValue, value);
                if (path == null)
                {
                    ColorValueText = ColorValue.ToString();
                }
                else
                {
                    ColorValueText = ColorValue.ToString();
                }

            }
        }
        private string _whiteBalanceValueText;
        public string WhiteBalanceValueText
        {
            get => _whiteBalanceValueText;
            set => SetProperty(ref _whiteBalanceValueText, value);
        }
        private string _colorValueText;
        public string ColorValueText
        {
            get => _colorValueText;
            set => SetProperty(ref _colorValueText, value);
        }
        private string _contrastValueText;
        public string ContrastValueText
        {
            get => _contrastValueText;
            set => SetProperty(ref _contrastValueText, value);
        }
        #endregion

        private bool _checkStart = true;
        public bool CheckStart
        {
            get => _checkStart;
            set => SetProperty(ref _checkStart, value);
        }
        private bool isRunning;
        public bool IsRunning
        {
            get => isRunning;
            set => SetProperty(ref isRunning, value);
        }
        private EditImageModel _editImageModel;
        public EditImageModel EditImageModel
        {
            get => _editImageModel;
            set => SetProperty(ref _editImageModel, value);
        }
        private List<EditImageModel> editImageModels;
        public List<EditImageModel> EditImageModels
        {
            get => editImageModels;
            set => SetProperty(ref editImageModels, value);
        }
        public SKBitmap SKBitmap { get; set; }
        private DatabaseModel dataModel = new DatabaseModel();

        public string Tag { get; set; }
        #endregion

        #region Contructors

        public EditPageViewModel(INavigationService navigationService)
            : base(navigationService)
        {
            Instance = this;
            EditImageModel = new EditImageModel();
            EditImageModels = new List<EditImageModel>();

            CanvasCommand = new Command(ExcuteCanvas);
            FilterCommand = new Command(ExcuteFilter);
            DrawCommand = new Command(ExcuteDraw);
            //AddTextCommand = new Command(ExcuteAddText);
            CropCommand = new Command(ExcuteCrop);

            WhiteBalanceValueText = WhiteBalanceValue.ToString();
            WhiteBalanceStopCommand = new Command(ExcuteWhiteBalanceStop);
            ContrastValueText = ContrastValue.ToString();
            ContrastStopCommand = new Command(ExcuteContrastStop);
            ColorValueText = ColorValue.ToString();
            BrightnessStopCommand = new Command(ExcuteBrightnessStop);

            DoneCommand = new Command(ExcuteDone);
            CancelCommand = new Command(ExcuteCancel);
        }
        #endregion

        #region Navigation
        public async override void OnNavigatedTo(INavigationParameters parameters)
        {
            ContrastEnable = true;
            CanvasVisible = true;
            StudioVisible = false;
            CropVisible = false;
            SignatureVisible = false;
            IconVisible = true;
            DrawVisible = false;
            AddTextVisible = false;
            IsRunning = false;
            //await LoadingPopup.Instance.Show();
            if (parameters != null)
            {
                if (parameters.ContainsKey(ParamKeys.ImageToEdit.ToString()))
                {
                    path = (string)parameters[ParamKeys.ImageToEdit.ToString()];
                    //ImageFromFile = File.ReadAllBytes(path);
                    //ImageEdited = ImageSource.FromFile(path);
                    //CheckStart = false;
                }
                if (parameters.ContainsKey(ParamKeys.SKBitmap.ToString()))
                {
                    SKBitmap = (SKBitmap)parameters[ParamKeys.SKBitmap.ToString()];
                }
            }
        }
        #endregion

        #region Filter
        public ICommand CanvasCommand { get; set; }
        public void ExcuteCanvas()
        {
            StudioVisible = false;
            CanvasVisible = true;
            CropVisible = false;
            DrawVisible = false;
            AddTextVisible = false;
            cropPath = Preferences.Get("croppedPath", null);
            if (cropPath != null)
            {
                path = cropPath;
            }
        }
        public ICommand FilterCommand { get; set; }
        public void ExcuteFilter()
        {
            StudioVisible = true;
            CanvasVisible = false;
            CropVisible = false;
            DrawVisible = false;
            AddTextVisible = false;
            cropPath = Preferences.Get("croppedPath", null);
            if (cropPath != null)
            {
                path = cropPath;
            }
        }

        #region Draw and paint
        public ICommand DrawCommand { get; set; }
        public async void ExcuteDraw()
        {
            StudioVisible = false;
            CanvasVisible = false;
            CropVisible = false;
            DrawVisible = true;
            AddTextVisible = false;
            cropPath = Preferences.Get("croppedPath", null);
            if (cropPath != null)
            {
                path = cropPath;
            }

        }
        #endregion

        #region Crop
        public ICommand CropCommand { get; set; }
        public async void ExcuteCrop()
        {
            StudioVisible = false;
            CanvasVisible = false;
            CropVisible = true;
            DrawVisible = false;
            AddTextVisible = false;
        }
        #endregion

        #endregion


        #region ImageEditColection
        public void ImageEditContrast()
        {
            ContrastValueText = ContrastValue.ToString();
            pathOri = editService.ContrastChange(ContrastValue, path);
            ImageEdited = ImageSource.FromFile(pathOri);
            cropPath = null;
        }
        public ICommand ContrastStopCommand { get; set; }
        public void ExcuteContrastStop()
        {
            ImageEditContrast();
        }

        public ICommand BrightnessStopCommand { get; set; }
        public void ExcuteBrightnessStop()
        {
            ImageEditBrightness();
        }
        public void ImageEditBrightness()
        {
            ColorValueText = ColorValue.ToString();
            pathOri = editService.Brightness(ColorValue, path);
            ImageEdited = ImageSource.FromFile(pathOri);
            cropPath = null;
        }

        public ICommand WhiteBalanceStopCommand { get; set; }
        public void ExcuteWhiteBalanceStop()
        {
            ImageEditWhiteBalance();
        }
        public void ImageEditWhiteBalance()
        {
            WhiteBalanceValueText = WhiteBalanceValue.ToString();
            pathOri = editService.Whitebalance(WhiteBalanceValue, path);
            ImageEdited = ImageSource.FromFile(pathOri);
            cropPath = null;
        }
        #endregion

        #region Reset
        public void Reset()
        {
            editService.ResetService(path);
        }
        #endregion
        
        #region Cancel
        public ICommand CancelCommand { get; set; }
        public async void ExcuteCancel()
        {
            await Navigation.GoBackAsync(animated: false);
        }
        #endregion

        #region Done 
        public ICommand DoneCommand { get; set; }
        public async void ExcuteDone()
        {
            path = EditPage.Instance.Done();
            SaveToDevice(path);
            Database database = new Database();
            dataModel.filepath = path;
            NavigationParameters param = new NavigationParameters
                {
                   {ParamKeys.DataModel.ToString(), dataModel}
                };

            await Navigation.NavigateAsync(ManagerPage.ReviewPage, param, animated: false);
        }
        #endregion

        #region Save To Gallery
        private async void SaveToDevice(string path)
        {
            SKBitmap bitmap = null;
            byte[] data;
            if (bitmap == null)
            {
                Stream stream = File.OpenRead(path);
                using (var memoryStream = new MemoryStream())
                {
                    stream.CopyTo(memoryStream);
                    data = memoryStream.ToArray();
                }
                if (data != null)
                {

                    await fileService.SaveImageAsync(data, $"ImgPack{DateTime.Now:dd.MM.yyyy HH-mm-ss}.jpeg", "ImgPack");
                }
                data = null;
            }
        }
        #endregion

        #region Get Path
        public string GetPath()
        {
            if (pathOri != null)
            {
                return pathOri;
            }
            else
                return path;
        }
        #endregion
    }
}
