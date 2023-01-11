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
    public class EditProfileViewModel : BaseViewModel
    {
        #region Properties
        private UserModel _user;
        public UserModel User
        {
            get => _user;
            set => SetProperty(ref _user, value);
        }
        private string _name;
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }
        private string _avatar;
        public string Avatar
        {
            get => _avatar;
            set => SetProperty(ref _avatar, value);
        }
        #endregion

        public EditProfileViewModel(INavigationService navigationService) : base(navigationService)
        {
            User = new UserModel();
            UpdateCommand = new Command(ExcuteUpdate);
            AddCommand = new Command(TakePhoto);
            BackCommand = new Command(ExcuteBack);
        }

        #region Navigation
        public async override void OnNavigatedTo(INavigationParameters parameters)
        {
            if (parameters != null)
            {
                if (parameters.ContainsKey(ParamKeys.UserModel.ToString()))
                {
                    User = (UserModel)parameters[ParamKeys.UserModel.ToString()];
                    Avatar = User.Avatar;
                    Name = User.UserName;
                }
            }
        }
        #endregion

        #region Back
        public ICommand BackCommand { get; set; }

        public async void ExcuteBack()
        {
            await Navigation.GoBackAsync(animated: false) ;
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

        #region UpdateCommand
        public ICommand UpdateCommand { get; set; }
        public async void ExcuteUpdate()
        {
            if (string.IsNullOrEmpty(Avatar))
            {
                await MessagePopup.Instance.Show("Please Choose Avatar Image");
                return;
            }
            else
            {
                User.Avatar = Avatar;
                User.UserName = Name;
                Update();
            }
        }

        public async void Update()
        {
            await LoadingPopup.Instance.Show();
            var url = ApiUrl.UpdateUser(User.Id);

            var param = User;
            HttpClientHandler clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };

            HttpClient client = new HttpClient(clientHandler);
            var httpContent = param.ObjectToStringContent();
            if (httpContent != null)
            {
                var response = await client.PutAsync(requestUri: url, content: httpContent);
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
                await MessagePopup.Instance.Show(("Update profile successfully"),
                    closeCommand: new Command(async () =>
                    {
                        await Navigation.GoBackAsync(animated: false);
                    }));
            }
            else
            {
                await MessagePopup.Instance.Show("Update profile failed");
            }
        }
        #endregion
    }
}
