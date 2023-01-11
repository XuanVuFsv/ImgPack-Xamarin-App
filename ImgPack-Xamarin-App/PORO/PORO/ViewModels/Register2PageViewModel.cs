using Plugin.Media;
using Plugin.Media.Abstractions;
using PORO.Enums;
using PORO.Models;
using PORO.Services;
using PORO.Untilities;
using PORO.ViewModels.Base;
using PORO.Views.Popups;
using Prism.Navigation;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace PORO.ViewModels
{
    public class Register2PageViewModel : BaseViewModel
    {
        #region Properties
        private UserModel _userModel;
        public UserModel UserModels
        {
            get => _userModel;
            set => SetProperty(ref _userModel, value);
        }

        private string _avatar;
        public string Avatar
        {
            get => _avatar;
            set => SetProperty(ref _avatar, value);
        }
        #endregion
        public Register2PageViewModel(INavigationService navigationService) : base(navigationService)
        {
            UserModels = new UserModel();
            SignUpCommand = new Command(ExcuteSignUp);
            SignInCommand = new Command(ExcuteSignIn);
            AddCommand = new Command(TakePhoto);
            BackCommand = new Command(ExcuteBack);
        }

        #region Navigation
        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            if (parameters != null)
            {
                if (parameters.ContainsKey(ParamKeys.UserModel.ToString()))
                {
                    UserModels = (UserModel)parameters[ParamKeys.UserModel.ToString()];
                }
            }
        }
        #endregion

        #region Back
        public ICommand BackCommand { get; set; }
        public async void ExcuteBack()
        {
            await Navigation.GoBackAsync(animated: false);
        }
        #endregion

        #region SignUp
        public ICommand SignUpCommand { get; set; }
        public async void ExcuteSignUp()
        {
            if (string.IsNullOrEmpty(Avatar))
            {
                await MessagePopup.Instance.Show("Please Choose Avatar Image");
                return;
            }
            else
            {
                UserModels.Avatar = Avatar;
                SignUp();
            }
        }

        public async void SignUp()
        {
            await LoadingPopup.Instance.Show();
            var url = ApiUrl.Register();

            var param = UserModels;
            HttpClientHandler clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };

            HttpClient client = new HttpClient(clientHandler);
            var httpContent = param.ObjectToStringContent();
            if (httpContent != null)
            {
                var response = await client.PostAsync(requestUri: url, content: httpContent);
                await LoadingPopup.Instance.Hide();
                RegisterResponse(response);
            }
            await LoadingPopup.Instance.Hide();
        }
        private async void RegisterResponse(HttpResponseMessage response)
        {
            if (response == null)
            {
                await MessagePopup.Instance.Show("Check Internet");
            }
            else if (response.IsSuccessStatusCode)
            {
                await MessagePopup.Instance.Show(("Account successfully created"),
                    closeCommand: new Command(async () =>
                    {
                        await Navigation.NavigateAsync($"/{ManagerPage.LoginPage}", animated: false);
                    }));
            }
            else
            {
                await MessagePopup.Instance.Show("Account creation failed");
            }
        }
        #endregion

        #region SignIn
        public ICommand SignInCommand { get; set; }
        public async void ExcuteSignIn()
        {
            await Navigation.NavigateAsync($"/{ManagerPage.LoginPage}", animated: false);
        }
        #endregion

        #region TakePhoto
        public ICommand AddCommand { get; set; }
        public async void TakePhoto()
        {
            await TakePhotoPopup.Instance.Show(cameraCommand: new Command(async () =>
            {
                var statusCamera = await Permissions.RequestAsync<Permissions.Camera>();
                if (statusCamera != PermissionStatus.Granted)
                {
                    return;
                }
                var statusStorage = await Permissions.RequestAsync<Permissions.StorageWrite>();
                if (statusStorage != PermissionStatus.Granted)
                {
                    return;
                }
                if (!CrossMedia.Current.IsCameraAvailable ||
                    !CrossMedia.Current.IsTakePhotoSupported)
                {
                    await MessagePopup.Instance.Show("No Camera");
                    return;
                }
                var image = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions
                {
                    CompressionQuality = 40,
                    CustomPhotoSize = 35,
                    PhotoSize = PhotoSize.MaxWidthHeight,
                    MaxWidthHeight = 1500,
                    DefaultCamera = CameraDevice.Rear,
                    SaveToAlbum = true
                });

                if (image == null)
                {
                    return;
                }
                var filepath = image.Path;
                if (filepath != null)
                {
                    Avatar = filepath;
                }
            }),
            galleryCommand: new Command(async () =>
            {
                var statusStorage = await Permissions.RequestAsync<Permissions.StorageWrite>();
                if (statusStorage != PermissionStatus.Granted)
                {
                    return;
                }
                var statusStorageRead = await Permissions.RequestAsync<Permissions.StorageRead>();
                if (statusStorageRead != PermissionStatus.Granted)
                {
                    return;
                }
                if (!CrossMedia.Current.IsPickPhotoSupported) { return; }
                var image = await CrossMedia.Current.PickPhotoAsync(new PickMediaOptions
                {
                    PhotoSize = PhotoSize.MaxWidthHeight,
                    MaxWidthHeight = 1500,
                    SaveMetaData = true,
                });

                if (image == null)
                {
                    return;
                }
                var filepath = image.Path;
                if (filepath != null)
                {
                    Avatar = filepath;
                }
            })
            );
        }
        #endregion
    }
}
