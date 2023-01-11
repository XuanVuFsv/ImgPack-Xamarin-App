using PORO.Controls;
using PORO.Untilities;
using Rg.Plugins.Popup.Extensions;
using SkiaSharp;
using SkiaSharp.Views.Forms;
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
    public partial class ColorPickerPopup : PopupBasePage
    {
        public Color Color { get; set; }
        public ColorPickerPopup(bool isClosed)
        {
            InitializeComponent();
            IsClosed = isClosed;
        }

        #region Instance

        private static ColorPickerPopup _instance;
        private TaskCompletionSource<Color> Proccess;
        public static ColorPickerPopup Instance => _instance ?? (_instance = new ColorPickerPopup(true));

        public async Task<ColorPickerPopup> Show(ICommand acceptComamnd = null, object acceptCommandParameter = null,
            bool isAutoClose = false, uint duration = 2000)
        {
            //Proccess = new TaskCompletionSource<Color>();
            //await DeviceExtension.BeginInvokeOnMainThreadAsync(() =>
            //{
            //    AcceptCommand = acceptComamnd;
            //    AcceptCommandParameter = acceptCommandParameter;

            //    IsAutoClose = isAutoClose;
            //    Duration = duration;
            //    Color = Color.Black;
            //});

            AcceptCommand = acceptComamnd;
            AcceptCommandParameter = acceptCommandParameter;

            IsAutoClose = isAutoClose;
            Duration = duration;
            Color = Color.Black;

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

        public Task<Color> GetResult()
        {
            if (Proccess != null)
            {
                return Proccess.Task;
            }
            return null;
        }

        private async void Accep_Clicked(object sender, EventArgs e)
        {
            await DeviceExtension.BeginInvokeOnMainThreadAsync(async () =>
            {
                await Navigation.PopPopupAsync();
            });

            await Task.Delay(300);

            AcceptCommand?.Execute(AcceptCommandParameter);

            IsClosed = true;
        }
        #region AcceptCommand

        public static readonly BindableProperty AcceptCommandProperty =
            BindableProperty.Create(nameof(AcceptCommand),
                typeof(ICommand),
                typeof(ConfirmPopup),
                null,
                BindingMode.TwoWay);

        public ICommand AcceptCommand
        {
            get => (ICommand)GetValue(AcceptCommandProperty);
            set => SetValue(AcceptCommandProperty, value);
        }

        public static readonly BindableProperty AcceptCommandParameterProperty =
            BindableProperty.Create(nameof(AcceptCommandParameter),
                typeof(object),
                typeof(ConfirmPopup),
                null,
                BindingMode.TwoWay);

        public object AcceptCommandParameter
        {
            get => GetValue(AcceptCommandParameterProperty);
            set => SetValue(AcceptCommandParameterProperty, value);
        }

        #endregion
        private void select_color(object sender, ColorPicker.BaseClasses.ColorPickerEventArgs.ColorChangedEventArgs e)
        {
            Color = e.NewColor;
        }
        public SKColor GetColor()
        {
            if (Color != null)
            {
                return Color.ToSKColor();
            }
            else
                return Color.Black.ToSKColor();
        }
    }
}