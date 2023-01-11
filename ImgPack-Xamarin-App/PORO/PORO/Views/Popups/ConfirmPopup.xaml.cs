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
    public partial class ConfirmPopup : PopupBasePage
    {
        public ConfirmPopup()
        {
            InitializeComponent();
        }
        #region Instance

        private static ConfirmPopup _instance;

        public static ConfirmPopup Instance => _instance ?? (_instance = new ConfirmPopup() { IsClosed = true });

        public async Task<ConfirmPopup> Show(string message = null, string closeButtonText = null, ICommand acceptCommand = null,
            ICommand closeCommand = null, object closeCommandParameter = null,
            bool isAutoClose = false, uint duration = 2000)
        {
            // Close Loading Popup if it is showing
            //await LoadingPopup.Instance.Hide();

            await DeviceExtension.BeginInvokeOnMainThreadAsync(() =>
            {
                if (message != null)
                    LabelMessageContent.Text = message;

                ClosedPopupCommand = closeCommand;
                ClosedPopupCommandParameter = closeCommandParameter;
                AcceptCommand = acceptCommand;
                CancelCommand = closeCommand;

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

        private async void Cancel_Clicked(object sender, EventArgs e)
        {
            await DeviceExtension.BeginInvokeOnMainThreadAsync(async () =>
            {
                await Navigation.PopPopupAsync();
            });

            await Task.Delay(300);

            CancelCommand?.Execute(CancelCommandParameter);

            IsClosed = true;
        }

        private async void Accept_Clicked(object sender, EventArgs e)
        {
            await DeviceExtension.BeginInvokeOnMainThreadAsync(async () =>
            {
                await Navigation.PopPopupAsync();
            });

            await Task.Delay(300);

            AcceptCommand?.Execute(AcceptCommandParameter);

            IsClosed = true;
        }
        #endregion

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

        #region CancelCommand

        public static readonly BindableProperty CancelCommandProperty =
            BindableProperty.Create(nameof(CancelCommand),
                typeof(ICommand),
                typeof(ConfirmPopup),
                null,
                BindingMode.TwoWay);

        public ICommand CancelCommand
        {
            get => (ICommand)GetValue(CancelCommandProperty);
            set => SetValue(CancelCommandProperty, value);
        }

        public static readonly BindableProperty CancelCommandParameterProperty =
            BindableProperty.Create(nameof(CancelCommandParameter),
                typeof(object),
                typeof(ConfirmPopup),
                null,
                BindingMode.TwoWay);

        public object CancelCommandParameter
        {
            get => GetValue(CancelCommandParameterProperty);
            set => SetValue(CancelCommandParameterProperty, value);
        }

        #endregion
    }
}