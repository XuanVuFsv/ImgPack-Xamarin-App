using PORO.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Xamarin.Forms;

namespace PORO.Untilities
{
    public class ManagerPage
    {
        #region Properties
        public static readonly string MainPage = "MainPage";
        public static readonly string HomePage = "HomePage";
        public static readonly string PublishPage = "PublishPage";
        public static readonly string ReviewPage = "ReviewPage";
        public static readonly string EditPage = "EditPage"; 
        public static readonly string SharePage = "SharePage";
        public static readonly string ChangePage = "ChangePage";
        public static readonly string RegisterPage = "RegisterPage";
        public static readonly string LoginPage = "LoginPage";
        public static readonly string CloudPage = "CloudPage";
        public static readonly string UserPage = "UserPage";
        public static readonly string ProfilePage = "ProfilePage";
        public static readonly string Register2Page = "Register2Page";
        public static readonly string EditProfilePage = "EditProfilePage";
        #endregion

        #region MultiplePages

        public static string MultiplePages(string[] pages)
        {
            var path = "";
            if (pages == null) return "";
            if (pages.Length < 1) return "";
            for (var i = 0; i < pages.Length; i++)
            {
                path += i == 0 ? pages[i] : "/" + pages[i];
            }
            return path;
        }

        #endregion

        #region GetCurrentPage

        public static Page GetCurrentPage()
        {
            var mainPage = Application.Current.MainPage;
            var navStack = mainPage.Navigation.NavigationStack;

            if (navStack == null)
                return mainPage;

            if (navStack.Count < 1)
                return mainPage;

            return navStack.Last();
        }

        public static Page GetCurrentPage(bool withModal)
        {

            if (!withModal) return GetCurrentPage();
            try
            {
                var navPage = GetCurrentPage();
                var modalPage = navPage.Navigation.ModalStack.LastOrDefault();
                var foundedPage = modalPage ?? navPage;
                return foundedPage;
            }
            catch (Exception e)
            {
#if DEBUG
                Debug.WriteLine(e.Message);
#endif
                return null;
            }
        }

        public static BaseViewModel GetCurrentPageBaseViewModel()
        {
            return (BaseViewModel)GetCurrentPage(true).BindingContext;
        }

        #endregion

        #region MyRegion

        public static string GoBack(string page = "", int number = 1)
        {
            var home = "../";

            var mainPage = Application.Current.MainPage;

            var navStack = mainPage.Navigation.NavigationStack;
            if (number < 1 || number >= navStack.Count)
                return "";


            for (; number < navStack.Count; number++)
            {
                home += "../";
            }

            return $"{home}{page}";
        }

        #endregion
    }
}
