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
using Xamarin.Forms;

namespace PORO.ViewModels
{
    public class RegisterPageViewModel : BaseViewModel
    {

        #region Properties
        private string _userName;
        public string UserName
        {
            get => _userName;
            set => SetProperty(ref _userName, value);
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
        private string _confirmPassword;
        public string ConfirmPassword
        {
            get => _confirmPassword;
            set => SetProperty(ref _confirmPassword, value);
        }
        private UserModel _userModel;
        public UserModel UserModels
        {
            get => _userModel;
            set => SetProperty(ref _userModel, value);
        }
        #endregion

        #region Contructors
        public RegisterPageViewModel(INavigationService navigationService) : base(navigationService)
        {
            SignUpCommand = new Command(ExcuteSignUp);
            SignInCommand = new Command(ExcuteSignIn);
            BackCommand = new Command(ExcuteBack);
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
            #region CheckEmpty
            if (string.IsNullOrEmpty(UserName))
            {
                await MessagePopup.Instance.Show("Please Enter UserName");
                return;
            }
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
            if (string.IsNullOrEmpty(ConfirmPassword))
            {
                await MessagePopup.Instance.Show("Please Enter Confirm Password");
                return;
            }
            else if (ConfirmPassword != Password)
            {
                await MessagePopup.Instance.Show("Confirm Password Wrong");
                return;
            }
            #endregion

            UserModels = new UserModel()
            {
                UserName = UserName,
                Email = EmailAddress,
                Password = Password
            };
            NavigationParameters param = new NavigationParameters
                {
                    {ParamKeys.UserModel.ToString(), UserModels}
                };
            await Navigation.NavigateAsync(ManagerPage.Register2Page, param, animated: false);
        }
        #endregion

        #region SignIn
        public ICommand SignInCommand { get; set; }
        public async void ExcuteSignIn()
        {
            await Navigation.NavigateAsync($"/{ManagerPage.LoginPage}", animated: false);
        }
        #endregion
    }
}
