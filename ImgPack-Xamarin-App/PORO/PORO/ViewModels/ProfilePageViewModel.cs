using Newtonsoft.Json;
using PORO.Enums;
using PORO.Models;
using PORO.Services;
using PORO.Untilities;
using PORO.ViewModels.Base;
using PORO.Views.Popups;
using Prism.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace PORO.ViewModels
{
    public class ProfilePageViewModel : BaseViewModel
    {
        #region Properties
        private ObservableCollection<PublishModel> _publishModel;
        public ObservableCollection<PublishModel> PublishModels
        {
            get => _publishModel;
            set => SetProperty(ref _publishModel, value);
        }
        private UserModel _user;
        public UserModel User
        {
            get => _user;
            set => SetProperty(ref _user, value);
        }
        private string _avatar;
        public string Avatar
        {
            get => _avatar;
            set => SetProperty(ref _avatar, value);
        }
        private string _name;
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }
        private string _description;
        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }
        private string _userId;
        public string UserId
        {
            get => _userId;
            set => SetProperty(ref _userId, value);
        }
        #endregion
        #region Contructors
        public ProfilePageViewModel(INavigationService navigationService) : base(navigationService)
        {
            PublishModels = new ObservableCollection<PublishModel>();
            User = new UserModel();
            ItemSelectedCommand = new Command(ListSelectedItem);
            MoreCommand = new Command(ExecuteMore);
            BackCommand = new Command(ExcuteBack);
        }
        #endregion

        #region Navigation
        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            UserId = Preferences.Get("userId", null);
            GetUser(UserId);
            GetListTopic(UserId);
        }
        #endregion

        #region Back
        public ICommand BackCommand { get; set; }
        public async void ExcuteBack()
        {
            await Navigation.GoBackAsync(animated: false);
        }

        #endregion

        #region Get User
        public async void GetUser(string userID)
        {
            await LoadingPopup.Instance.Show();
            var url = ApiUrl.GetUser(userID);
            HttpClientHandler clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };

            HttpClient client = new HttpClient(clientHandler);
            var response = await client.GetAsync(requestUri: url);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var user = JsonConvert.DeserializeObject<UserModel>(content);
                User = user;
                Avatar = user.Avatar;
                Name = user.UserName;
                Description = user.UserName;
            }
            await LoadingPopup.Instance.Hide();
        }
        #endregion

        #region GetTopic
        public async void GetListTopic(string userID)
        {
            await LoadingPopup.Instance.Show();
            var url = ApiUrl.UploadPhoto();
            HttpClientHandler clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };

            HttpClient client = new HttpClient(clientHandler);
            var response = await client.GetAsync(requestUri: url);
            if (response.IsSuccessStatusCode)
            {
                PublishModels = new ObservableCollection<PublishModel>();
                var content = await response.Content.ReadAsStringAsync();
                var list = JsonConvert.DeserializeObject<ObservableCollection<PublishModel>>(content);
                var n = list.Count();
                for (int i = n - 1; i >= 0; i--)
                {
                    if (list[i].User.Id == userID)
                    {
                        PublishModels.Add(new PublishModel
                        {
                            Id = list[i].Id,
                            User = list[i].User,
                            Description = list[i].Description,
                            Name = list[i].Name,
                            Image = list[i].Image,
                        });
                    }
                }
            }
            await LoadingPopup.Instance.Hide();
        }
        #endregion

        #region TopicSelected
        private PublishModel _listSelected { get; set; }
        public PublishModel SelectedItemList
        {
            get { return _listSelected; }
            set
            {
                _listSelected = value;
                RaisePropertyChanged();
            }
        }
        public ICommand ItemSelectedCommand { get; set; }
        public async void ListSelectedItem()
        {
            if (SelectedItemList != null)
            {
                var list = SelectedItemList;
                NavigationParameters param = new NavigationParameters
                {
                    {ParamKeys.Share.ToString(), list}
                };
                await Navigation.NavigateAsync(ManagerPage.SharePage, param);
                SelectedItemList = null;
            }
        }
        #endregion

        #region More
        public ICommand MoreCommand { get; set; }
        public async void ExecuteMore()
        {
            await ProfileSettingPopup.Instance.Show(logoutCommand: new Command(async() => 
            {
                await ConfirmPopup.Instance.Show(message: "Confirm LogOut",
                      acceptCommand: new Command(async () =>
                      {
                          Preferences.Set("email", null);
                          Preferences.Set("password", null);
                          Preferences.Set("userId", 0);
                          await Navigation.NavigateAsync(ManagerPage.LoginPage, animated: false);
                      }));
            }), editProfileCommand: new Command(async () => 
            {
                if(User != null)
                {
                    NavigationParameters param = new NavigationParameters
                {
                    {ParamKeys.UserModel.ToString(), User}
                };
                    await Navigation.NavigateAsync(ManagerPage.EditProfilePage, param, animated: false);
                }
            })
            );
                
        }
        #endregion
    }
}
