using BitooBitImageEditor.TouchTracking;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BitooBitImageEditor.EditorPage
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ImageCropPage : ContentPage
    {
        private readonly ImageEditorViewModel viewModel;
        internal ImageCropPage(SKBitmap bitmap, ImageEditorConfig config)
        {
            InitializeComponent();

            if (Device.RuntimePlatform == Device.iOS)
            {
                Header.HeightRequest = 60;
                CropHeader.HeightRequest = 60;
            }
            else if (Device.RuntimePlatform == Device.Android)
            {
                Header.HeightRequest = 40;
                CropHeader.HeightRequest = 40;
            }

            viewModel = new ImageEditorViewModel(bitmap, config, 1);
            this.BindingContext = viewModel;
            canvasCropViewHost.Children.Add(viewModel.cropperCanvas, 0, 0);
            canvasMainViewHost.Children.Add(viewModel.mainCanvas, 0, 0);
            viewModel.PropertyChanged += ViewModel_PropertyChanged;
        }

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ImageEditorViewModel.TextVisible))
            {
            }
            if (e.PropertyName == nameof(ImageEditorViewModel.CurrentTextIsFill))
            {
            }
        }
        protected override void OnDisappearing()
        {
            ImageEditor.Instance.SetImage();
            base.OnDisappearing();
        }

        private void TouchEffect_TouchAction(object sender, TouchActionEventArgs args) => viewModel.OnTouchEffectTouchAction(sender, args);

        internal void Dispose()
        {
            viewModel.Dispose();
        }

        #region BackExecute
        protected override bool OnBackButtonPressed()
        {
            viewModel.OnBackPressed();
            return true;
        }


        public void OnSoftBackButtonPressed()
        {

        }
        #endregion
    }
}