using Newtonsoft.Json;
using PORO.Enums;
using PORO.Models;
using PORO.Services;
using PORO.Services.Database;
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
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace PORO.ViewModels
{
    public class MainPageViewModel : BaseViewModel
    {
        private UserModel _userModel;
        public UserModel UserModels
        {
            get => _userModel;
            set => SetProperty(ref _userModel, value);
        }
        private string _email;
        public string EmailAddress
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }
        private string _password;
        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        public MainPageViewModel(INavigationService navigationService) : base(navigationService)
        {
            UserModels = new UserModel();
        }

        #region Navigation
        public async override void OnNavigatedTo(INavigationParameters parameters)
        {
            ExcuteNextPage();
        }
        #endregion
        public async void ExcuteNextPage()
        {
            EmailAddress = Preferences.Get("email", null);
            Password = Preferences.Get("password", null);
            if (string.IsNullOrEmpty(EmailAddress) || string.IsNullOrEmpty(Password))
            {
                await Task.Delay(2000);
                await Navigation.NavigateAsync(ManagerPage.LoginPage);
            }
            else
            {
                await LoadingPopup.Instance.Show();
                var url = ApiUrl.Register();
                HttpClientHandler clientHandler = new HttpClientHandler();
                clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };

                // Pass the handler to httpclient(from you are calling api)
                HttpClient client = new HttpClient(clientHandler);
                var response = await client.GetAsync(requestUri: url);
                SignInResponse(response);
                await LoadingPopup.Instance.Hide();
            }
        }
        private async void SignInResponse(HttpResponseMessage response)
        {
            if (response == null)
            {
                await MessagePopup.Instance.Show("Check Internet");
            }
            else if (response.IsSuccessStatusCode)
            {
                await LoadingPopup.Instance.Hide();
                var content = await response.Content.ReadAsStringAsync();
                var list = JsonConvert.DeserializeObject<ObservableCollection<UserModel>>(content);
                var n = list.Count();
                if(n == 0)
                {
                    //await Task.Delay(2000);
                    //await Navigation.NavigateAsync(ManagerPage.LoginPage);
                }
                else
                {
                    for (int i = 0; i < n; i++)
                    {
                        if (EmailAddress == list[i].Email && Password == list[i].Password)
                        {
                            //Database database = new Database();
                            //var session = database.Get(list[i].Id);
                            //if (session == null)
                            //{
                            //    UserModels = new UserModel()
                            //    {
                            //        Id = list[i].Id,
                            //        UserName = list[i].UserName,
                            //        Avatar = list[i].Avatar,
                            //        Email = list[i].Email,
                            //        Password = list[i].Password
                            //    };
                            //    database.Insert(UserModels);
                            //}
                            //else
                            //{
                            //    UserModels.Id = list[i].Id;
                            //    UserModels.UserName = list[i].UserName;
                            //    UserModels.Avatar = list[i].Avatar;
                            //    UserModels.Email = list[i].Email;
                            //    UserModels.Password = list[i].Password;
                            //    database.Update(UserModels);
                            //}
                            Preferences.Set("userId", list[i].Id);

                            NavigationParameters param = new NavigationParameters
                            {
                                {ParamKeys.UserModel.ToString(), UserModels}
                            };
                            await Navigation.NavigateAsync($"/{ManagerPage.HomePage}", param, animated: false);
                            return;
                        }
                    }
                }
                await Navigation.NavigateAsync(ManagerPage.LoginPage);
            }
            else
            {
                await MessagePopup.Instance.Show("Login failed");
                await Navigation.NavigateAsync(ManagerPage.LoginPage);
            }
        }
    }
}
