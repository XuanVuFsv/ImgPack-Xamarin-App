using PORO.Controls;
using PORO.Untilities;
using Rg.Plugins.Popup.Extensions;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace PORO.Views.Popups
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TakePhotoPopup : PopupBasePage
    {
        public TakePhotoPopup()
        {
            InitializeComponent();
        }

        #region Instance

        private static TakePhotoPopup _instance;

        public static TakePhotoPopup Instance => _instance ?? (_instance = new TakePhotoPopup() { IsClosed = true });

        public async Task<TakePhotoPopup> Show(string closeButtonText = null, ICommand galleryCommand = null,
            ICommand cameraCommand = null, ICommand cloudCommand = null, object closeCommandParameter = null,
            bool isAutoClose = false, uint duration = 2000)
        {
            // Close Loading Popup if it is showing
            //await LoadingPopup.Instance.Hide();

            await DeviceExtension.BeginInvokeOnMainThreadAsync(() =>
            {
                if (galleryCommand != null)
                {
                    GalleryCommand = galleryCommand;
                }
                if (cameraCommand != null)
                {
                    CameraCommand = cameraCommand;
                }
                if (cloudCommand != null)
                {
                    CloudCommand = cloudCommand;
                }

                IsAutoClose = isAutoClose;
                Duration = duration;
            });

            if (IsClosed)
            {
                IsClosed = false;

                if (isAutoClose && duration > 0)
                    AutoClosedPopupAfter(duration);

                await DeviceExtension.BeginInvokeOnMainThreadAsync(async () =>
                {
                    await Application.Current.MainPage.Navigation.PushPopupAsync(this);
                });
            }

            return this;
        }

        #endregion

        #region Hide
        private async Task Hide()
        {
            if (PopupNavigation.Instance.PopupStack.Contains(this))
            {
                await PopupNavigation.Instance.PopAsync(true);
            }
        }
        #endregion

        #region Event
        private async void gallery_tapped(object sender, EventArgs e)
        {
            await DeviceExtension.BeginInvokeOnMainThreadAsync(async () =>
            {
                await Navigation.PopPopupAsync();
            });

            await Task.Delay(300);

            GalleryCommand?.Execute(GalleryCommandParameter);

            IsClosed = true;
        }

        private async void camera_tapped(object sender, EventArgs e)
        {
            await DeviceExtension.BeginInvokeOnMainThreadAsync(async () =>
            {
                await Navigation.PopPopupAsync();
            });

            await Task.Delay(300);

            CameraCommand?.Execute(CameraCommandParameter);

            IsClosed = true;
        }

        #endregion

        #region GalleryCommand

        public static readonly BindableProperty GalleryCommandProperty =
            BindableProperty.Create(nameof(GalleryCommand),
                typeof(ICommand),
                typeof(TakePhotoPopup),
                null,
                BindingMode.TwoWay);

        public ICommand GalleryCommand
        {
            get => (ICommand)GetValue(GalleryCommandProperty);
            set => SetValue(GalleryCommandProperty, value);
        }

        public static readonly BindableProperty GalleryCommandParameterProperty =
            BindableProperty.Create(nameof(GalleryCommandParameter),
                typeof(object),
                typeof(TakePhotoPopup),
                null,
                BindingMode.TwoWay);

        public object GalleryCommandParameter
        {
            get => GetValue(GalleryCommandParameterProperty);
            set => SetValue(GalleryCommandParameterProperty, value);
        }

        #endregion

        #region CameraCommand

        public static readonly BindableProperty CameraCommandProperty =
            BindableProperty.Create(nameof(CameraCommand),
                typeof(ICommand),
                typeof(TakePhotoPopup),
                null,
                BindingMode.TwoWay);

        public ICommand CameraCommand
        {
            get => (ICommand)GetValue(CameraCommandProperty);
            set => SetValue(CameraCommandProperty, value);
        }

        public static readonly BindableProperty CameraCommandParameterProperty =
            BindableProperty.Create(nameof(CameraCommandParameter),
                typeof(object),
                typeof(TakePhotoPopup),
                null,
                BindingMode.TwoWay);

        public object CameraCommandParameter
        {
            get => GetValue(CameraCommandParameterProperty);
            set => SetValue(CameraCommandParameterProperty, value);
        }

        #endregion

        #region CloudCommand

        public static readonly BindableProperty CloudCommandProperty =
            BindableProperty.Create(nameof(CloudCommand),
                typeof(ICommand),
                typeof(TakePhotoPopup),
                null,
                BindingMode.TwoWay);

        public ICommand CloudCommand
        {
            get => (ICommand)GetValue(CloudCommandProperty);
            set => SetValue(CloudCommandProperty, value);
        }

        public static readonly BindableProperty CloudCommandParameterProperty =
            BindableProperty.Create(nameof(CloudCommandParameter),
                typeof(object),
                typeof(TakePhotoPopup),
                null,
                BindingMode.TwoWay);

        public object CloudCommandParameter
        {
            get => GetValue(CloudCommandParameterProperty);
            set => SetValue(CloudCommandParameterProperty, value);
        }

        #endregion
    }
}