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
using System.Text.RegularExpressions;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace PORO.ViewModels
{
    public class SharePageViewModel : BaseViewModel
    {
        #region Properties
        public ImageSource _imageReview;
        public ImageSource ImageReview
        {
            get => _imageReview;
            set => SetProperty(ref _imageReview, value);
        }
        private PublishModel _publishModdel;
        public PublishModel PublishModels
        {
            get => _publishModdel;
            set => SetProperty(ref _publishModdel, value);
        }
        private string _avatar;
        public string Avatar
        {
            get => _avatar;
            set => SetProperty(ref _avatar, value);
        }
        private string _userName;
        public string UserName
        {
            get => _userName;
            set => SetProperty(ref _userName, value);
        }
        private string _name;
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }
        private string _itemDescription;
        public string Description
        {
            get => _itemDescription;
            set => SetProperty(ref _itemDescription, value);
        }
        private string _date;
        public string Date
        {
            get => _date;
            set => SetProperty(ref _date, value);
        }
        public string path { get; set; }
        #endregion

        #region Constructors
        public SharePageViewModel(INavigationService navigationService)
            : base(navigationService)
        {
            PublishModels = new PublishModel();
            ShareCommand = new Command(ExcuteShare);
            ChangeCommand = new Command(ExcuteChange);
            DeleteCommand = new Command(ExcuteDelete);
            BackCommand = new Command(ExcuteBack);
        }
        #endregion

        #region Navigation
        public async override void OnNavigatedTo(INavigationParameters parameters)
        {
            if (parameters != null)
            {
                if (parameters.ContainsKey(ParamKeys.Share.ToString()))
                {
                    PublishModels = (PublishModel)parameters[ParamKeys.Share.ToString()];
                    path = PublishModels.Image;
                    ImageReview = path;
                    Description = PublishModels.Description;
                    Avatar = PublishModels.User.Avatar;
                    UserName = PublishModels.User.UserName;
                    Name = PublishModels.Name;
                }
            }
        }
        #endregion

        #region Share
        public ICommand ShareCommand { get; set; }
        public async void ExcuteShare()
        {
            if (path == null)
                return;
            if (IsValidUrl(path))
            {
                await Share.RequestAsync(new ShareTextRequest
                {
                    Uri = path,
                    Title = "Share "
                });
            }
            else
            {
                await Share.RequestAsync(new ShareFileRequest
                {
                    Title = "Share",
                    File = new ShareFile(path)
                });
            }
            
        }
        private bool IsValidUrl(string url)
        {
            return (Uri.IsWellFormedUriString(url, UriKind.Absolute));
        }
        #endregion

        #region Share
        public ICommand ChangeCommand { get; set; }
        public async void ExcuteChange()
        {
            NavigationParameters param = new NavigationParameters
                {
                    {ParamKeys.ImageToChange.ToString(), PublishModels}
                };
            await Navigation.NavigateAsync(ManagerPage.ChangePage, param);
        }
        #endregion

        #region Share
        public ICommand DeleteCommand { get; set; }
        public async void ExcuteDelete()
        {
            await ConfirmPopup.Instance.Show(message: "Confirm Delete",
                acceptCommand: new Command(async () =>
                {
                    await LoadingPopup.Instance.Show();
                    var url = ApiUrl.ChangePhoto(PublishModels.Id.ToString());

                    var param = PublishModels;
                    HttpClientHandler clientHandler = new HttpClientHandler();
                    clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };

                    HttpClient client = new HttpClient(clientHandler);
                    var response = await client.DeleteAsync(requestUri: url);
                    await LoadingPopup.Instance.Hide();
                    DeleteResponse(response);
                }));
        }
        private async void DeleteResponse(HttpResponseMessage response)
        {
            if (response == null)
            {
                await MessagePopup.Instance.Show("Check Internet");
            }
            else if (response.IsSuccessStatusCode)
            {
                await MessagePopup.Instance.Show(("Delete Successfully"),
                    closeCommand: new Command(async () =>
                    {
                        await Navigation.NavigateAsync($"/{ManagerPage.HomePage}", animated: false);
                    }));
            }
            else
            {
                await MessagePopup.Instance.Show("Publish Fail");
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
    }
}
