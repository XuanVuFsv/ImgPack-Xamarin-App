using PORO.Untilities;
using PORO.ViewModels;
using PORO.Views;
using Prism;
using Prism.Ioc;
using Prism.Unity;
using System;
using System.Net.Http.Headers;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace PORO
{
    public partial class App : PrismApplication
    {

        #region Properties
        public static HttpResponseHeaders HttpHeaders;
        #endregion
        #region Constructor

        public App() : this(null) { }

        public App(IPlatformInitializer initializer = null) : base(initializer)
        {
        }

        #endregion
        #region OnInitialized

        protected override async void OnInitialized()
        {
            //InitAppCenter();
            //InitDatabase();
            InitializeComponent();


            //Session = _sqLiteService.Get<SessionModel>(s => s.Wallet.Equals(Define.DefaultUserTokenInLocalDb));

            //if (Session == null)
            //{
            await NavigationService.NavigateAsync(new Uri($"/{ManagerPage.MainPage}"));
            //}
            //else
            //{
            // await NavigationService.NavigateAsync(new Uri($"/{ManagerPage.MainPage}"));
            //}
        }
        #endregion
        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<MainPage, MainPageViewModel>(ManagerPage.MainPage);
            containerRegistry.RegisterForNavigation<HomePage, HomePageViewModel>(ManagerPage.HomePage);
            containerRegistry.RegisterForNavigation<PublishPage, PublishPageViewModel>(ManagerPage.PublishPage);
            containerRegistry.RegisterForNavigation<ReviewPage, ReviewPageViewModel>(ManagerPage.ReviewPage);
            containerRegistry.RegisterForNavigation<EditPage, EditPageViewModel>(ManagerPage.EditPage);
            containerRegistry.RegisterForNavigation<SharePage, SharePageViewModel>(ManagerPage.SharePage);
            containerRegistry.RegisterForNavigation<ChangePage, ChangePageViewModel>(ManagerPage.ChangePage);
            containerRegistry.RegisterForNavigation<RegisterPage, RegisterPageViewModel>(ManagerPage.RegisterPage);
            containerRegistry.RegisterForNavigation<LoginPage, LoginPageViewModel>(ManagerPage.LoginPage);
            containerRegistry.RegisterForNavigation<CloudPage, CloudPageViewModel>(ManagerPage.CloudPage);
            containerRegistry.RegisterForNavigation<UserPage, UserPageViewModel>(ManagerPage.UserPage);
            containerRegistry.RegisterForNavigation<ProfilePage, ProfilePageViewModel>(ManagerPage.ProfilePage);
            containerRegistry.RegisterForNavigation<Register2Page, Register2PageViewModel>(ManagerPage.Register2Page);
            containerRegistry.RegisterForNavigation<EditProfilePage, EditProfileViewModel>(ManagerPage.EditProfilePage);
        }
    }
}
