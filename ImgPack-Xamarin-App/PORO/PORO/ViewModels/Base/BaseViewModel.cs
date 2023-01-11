using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using Prism.Navigation.Xaml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PORO.ViewModels.Base
{
    public class BaseViewModel : BindableBase, INavigationAware
    {
        #region Properties
        public INavigationService Navigation { get; private set; }
        #endregion
        public BaseViewModel(INavigationService navigationService)
        {
            if (navigationService != null) Navigation = navigationService;
            BackCommand = new DelegateCommand(async () => await BackExecute());
            //DiscoverCommand = new Command(ExcuteDiscover);
            //FavoriteCommand = new Command(ExcuteFavorite);
            //ProfileCommand = new DelegateCommand(ExcuteProfile);
            //HomeCommand = new DelegateCommand(ExcuteHome);
        }

        #region Navigate
        public virtual void OnNavigatedFrom(INavigationParameters parameters)
        {
#if DEBUG
            Debug.WriteLine("Navigated from");
#endif
        }

        public virtual void OnNavigatingTo(INavigationParameters parameters)
        {
#if DEBUG
            Debug.WriteLine("Navigating to");
#endif
        }

        public virtual void OnNavigatedTo(INavigationParameters parameters)
        {
#if DEBUG
            Debug.WriteLine("Navigated to");
#endif 

            if (parameters != null)
            {
                var navMode = parameters.GetNavigationMode();
                switch (navMode)
                {
                    case NavigationMode.New: OnNavigatedNewTo(parameters); break;
                    case NavigationMode.Back: OnNavigatedBackTo(parameters); break;
                }
            }

        }

        public virtual void OnNavigatedNewTo(INavigationParameters parameters)
        {
#if DEBUG
            Debug.WriteLine("Navigate new to");
#endif
        }

        public virtual void OnNavigatedBackTo(INavigationParameters parameters)
        {
#if DEBUG
            Debug.WriteLine("Navigate back to");
#endif
        }
        #endregion

        #region OnAppear / Disappear

        public virtual void OnAppear()
        {

        }

        public virtual void OnFirstTimeAppear()
        {

        }

        public virtual void OnDisappear()
        {

        }

        #endregion

        #region BackCommand

        public ICommand BackCommand { get; }

        protected virtual async Task BackExecute()
        {  
            //await Navigation.ClearPopupStackAsync();
            await Navigation.GoBackAsync();
        }

        #endregion

        #region BackButtonPress

        /// <summary>
        /// //false is default value when system call back press
        /// </summary>
        /// <returns></returns>
        public virtual bool OnBackButtonPressed()
        {
            //false is default value when system call back press
            //return false;
            BackExecute();

            return true;

        }
        #endregion
    }
}
