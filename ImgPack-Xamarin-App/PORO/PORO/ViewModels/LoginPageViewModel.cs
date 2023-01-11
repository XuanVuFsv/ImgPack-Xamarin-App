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
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace PORO.ViewModels
{
    public class LoginPageViewModel : BaseViewModel
    {

        #region Properties
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
        private UserModel _userModel;
        public UserModel UserModels
        {
            get => _userModel;
            set => SetProperty(ref _userModel, value);
        }
        #endregion

        #region Contructors
        public LoginPageViewModel(INavigationService navigationService) : base(navigationService)
        {
            SignUpCommand = new Command(ExcuteSignUp);
            SignInCommand = new Command(ExcuteSignIn);
            UserModels = new UserModel();
        }
        #endregion

        #region SignUp
        public ICommand SignUpCommand { get; set; }
        public async void ExcuteSignUp()
        {
            await Navigation.NavigateAsync(ManagerPage.RegisterPage, animated: false);
        }
        #endregion

        #region SignIn
        public ICommand SignInCommand { get; set; }
        public async void ExcuteSignIn()
        {
            if (string.IsNullOrEmpty(EmailAddress))
            {
                await MessagePopup.Instance.Show("Please Enter Emaill Address");
                return;
            }
            if (string.IsNullOrEmpty(Password))
            {
                await MessagePopup.Instance.Show("Please Enter Password");
                return;
            }

            await LoadingPopup.Instance.Show();
            var url = ApiUrl.Register();
            HttpClientHandler clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };

            HttpClient client = new HttpClient();
           
            try
            {
                var response = await client.GetAsync(requestUri: url);
                SignInResponse(response);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(@"ERROR {0}", ex.Message);
            }
            await LoadingPopup.Instance.Hide();
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
                for (int i = 0; i < n; i++)
                {
                    if (EmailAddress == list[i].Email && Password == list[i].Password)
                    {
                        Database database = new Database();
                        var session = database.Get(list[i].Id);
                        if (session == null)
                        {
                            UserModels = new UserModel()
                            {
                                Id = list[i].Id,
                                UserName = list[i].UserName,
                                Avatar = list[i].Avatar,
                                Email = list[i].Email,
                                Password = list[i].Password
                            };
                            database.Insert(UserModels);
                        }
                        else
                        {
                            UserModels.Id = list[i].Id;
                            UserModels.UserName = list[i].UserName;
                            UserModels.Avatar = list[i].Avatar;
                            UserModels.Email = list[i].Email;
                            UserModels.Password = list[i].Password;
                        }
                        Preferences.Set("userId", list[i].Id);
                        Preferences.Set("email", list[i].Email);
                        Preferences.Set("password", list[i].Password);
                        await Navigation.NavigateAsync($"/{ManagerPage.HomePage}", animated: false);
                        return;
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
        #endregion
    }
}
