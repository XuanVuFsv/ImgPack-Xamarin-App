using PORO.Enums;
using PORO.Untilities;
using PORO.ViewModels.Base;
using PORO.Views.Popups;
using Prism.Navigation;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;

namespace PORO.ViewModels
{
    public class CloudPageViewModel : BaseViewModel
    {
        #region Properties
        private string _itemSellLink;
        public string ItemSellLink
        {
            get => _itemSellLink;
            set => SetProperty(ref _itemSellLink, value);
        }
        #endregion
        #region Constructors
        public CloudPageViewModel(INavigationService navigationService) : base(navigationService)
        {
            BackCommandReview = new Command(ExcuteBackReview);
        }
        #endregion

        #region Navigation
        public async override void OnNavigatedTo(INavigationParameters parameters)
        {
            ItemSellLink = "https://drive.google.com/drive/u/0/my-drive";
        }
        #endregion
        public ICommand BackCommandReview { get; set; }
        public async void ExcuteBackReview()
        {
            await Navigation.NavigateAsync($"/{ManagerPage.HomePage}");
        }
    }
}
