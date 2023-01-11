using PORO.Enums;
using PORO.Models;
using PORO.Services;
using PORO.Untilities;
using PORO.ViewModels.Base;
using PORO.Views.Popups;
using Prism.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace PORO.ViewModels
{
    public class ChangePageViewModel : BaseViewModel
    {
        #region Properties
        public ImageSource _imageReview;
        public ImageSource ImageReview
        {
            get => _imageReview;
            set => SetProperty(ref _imageReview, value);
        }
        public byte[] ImageFromFile { get; set; }
        public string path { get; set; }
        private PublishModel _publishModdel;
        public PublishModel PublishModels
        {
            get => _publishModdel;
            set => SetProperty(ref _publishModdel, value);
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
        private string _id;
        public string Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }
        IDevice device = DependencyService.Get<IDevice>();
        #endregion

        #region Constructors
        public ChangePageViewModel(INavigationService navigationService)
            : base(navigationService)
        {
            PublishModels = new PublishModel();
            PostCommand = new Command(ExcutePost);
        }
        #endregion

        #region Navigation
        public async override void OnNavigatedTo(INavigationParameters parameters)
        {
            if (parameters != null)
            {
                if (parameters.ContainsKey(ParamKeys.ImageToChange.ToString()))
                {
                    PublishModels = (PublishModel)parameters[ParamKeys.ImageToChange.ToString()];
                    Id = PublishModels.Id;
                    path = PublishModels.Image;
                    Description = PublishModels.Description;
                    ImageReview = ImageSource.FromFile(PublishModels.Image);
                }
            }
        }
        #endregion
        #region PostImage
        public ICommand PostCommand { get; set; }
        public async void ExcutePost()
        {
            var userId = Preferences.Get("userId", 0);
            #region CheckEmpty
            if (string.IsNullOrEmpty(Description))
            {
                await MessagePopup.Instance.Show("Please Enter Description");
                return;
            }
            #endregion
            PublishModels = new PublishModel()
            {
                Id = Id,
                //User = userId,
                Description = Description,
                Image = path,
                Name = Date
            };
            await ConfirmPopup.Instance.Show(message: "Confirm Change",
                      acceptCommand: new Command(async () =>
                      {
                          PublishPhoto();
                      }));
        }
        private async void PublishPhoto()
        {
            await LoadingPopup.Instance.Show();
            var url = ApiUrl.ChangePhoto(Id.ToString());

            var param = PublishModels;
            HttpClientHandler clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };

            HttpClient client = new HttpClient(clientHandler);
            var httpContent = param.ObjectToStringContent();
            if (httpContent != null)
            {
                var response = await client.PutAsync(requestUri: url, content: httpContent);
                await LoadingPopup.Instance.Hide();
                PostSaleResponse(response);
            }
            await LoadingPopup.Instance.Hide();
        }
        private async void PostSaleResponse(HttpResponseMessage response)
        {
            if (response == null)
            {
                await MessagePopup.Instance.Show("Check Internet");
            }
            else if (response.IsSuccessStatusCode)
            {
                //Database database = new Database();
                //dataModel.sellLink = response.Result;
                //database.Update(dataModel);
                await MessagePopup.Instance.Show(("Change Successfully"),
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
    }
}
