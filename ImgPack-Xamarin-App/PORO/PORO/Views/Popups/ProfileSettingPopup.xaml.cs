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
    public partial class ProfileSettingPopup : PopupBasePage
    {
        public ProfileSettingPopup()
        {
            InitializeComponent();
        }
        #region Instance

        private static ProfileSettingPopup _instance;

        public static ProfileSettingPopup Instance => _instance ?? (_instance = new ProfileSettingPopup() { IsClosed = true });

        public async Task<ProfileSettingPopup> Show(string closeButtonText = null, ICommand editProfileCommand = null,
            ICommand logoutCommand = null, ICommand cloudCommand = null, object closeCommandParameter = null,
            bool isAutoClose = false, uint duration = 2000)
        {
            // Close Loading Popup if it is showing
            //await LoadingPopup.Instance.Hide();

            await DeviceExtension.BeginInvokeOnMainThreadAsync(() =>
            {
                if (editProfileCommand != null)
                {
                    EditProfileCommand = editProfileCommand;
                }
                if (logoutCommand != null)
                {
                    LogoutCommand = logoutCommand;
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
        private async void edit_tapped(object sender, EventArgs e)
        {
            await DeviceExtension.BeginInvokeOnMainThreadAsync(async () =>
            {
                await Navigation.PopPopupAsync();
            });

            await Task.Delay(300);

            EditProfileCommand?.Execute(EditProfileCommandParameter);

            IsClosed = true;
        }

        private async void logout_tapped(object sender, EventArgs e)
        {
            await DeviceExtension.BeginInvokeOnMainThreadAsync(async () =>
            {
                await Navigation.PopPopupAsync();
            });

            await Task.Delay(300);

            LogoutCommand?.Execute(LogoutCommandParameter);

            IsClosed = true;
        }

        #region EditProfileCommand

        public static readonly BindableProperty EditProfileCommandProperty =
            BindableProperty.Create(nameof(EditProfileCommand),
                typeof(ICommand),
                typeof(ProfileSettingPopup),
                null,
                BindingMode.TwoWay);

        public ICommand EditProfileCommand
        {
            get => (ICommand)GetValue(EditProfileCommandProperty);
            set => SetValue(EditProfileCommandProperty, value);
        }

        public static readonly BindableProperty EditProfileCommandParameterProperty =
            BindableProperty.Create(nameof(EditProfileCommandParameter),
                typeof(object),
                typeof(ProfileSettingPopup),
                null,
                BindingMode.TwoWay);

        public object EditProfileCommandParameter
        {
            get => GetValue(EditProfileCommandParameterProperty);
            set => SetValue(EditProfileCommandParameterProperty, value);
        }

        #endregion

        #region LogoutCommand

        public static readonly BindableProperty LogoutCommandProperty =
            BindableProperty.Create(nameof(LogoutCommand),
                typeof(ICommand),
                typeof(ProfileSettingPopup),
                null,
                BindingMode.TwoWay);

        public ICommand LogoutCommand
        {
            get => (ICommand)GetValue(LogoutCommandProperty);
            set => SetValue(LogoutCommandProperty, value);
        }

        public static readonly BindableProperty LogoutCommandParameterProperty =
            BindableProperty.Create(nameof(LogoutCommandParameter),
                typeof(object),
                typeof(ProfileSettingPopup),
                null,
                BindingMode.TwoWay);

        public object LogoutCommandParameter
        {
            get => GetValue(LogoutCommandParameterProperty);
            set => SetValue(LogoutCommandParameterProperty, value);
        }

        #endregion
    }
}