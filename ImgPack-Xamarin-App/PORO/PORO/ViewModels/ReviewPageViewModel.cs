using PORO.Enums;
using PORO.Models;
using PORO.Untilities;
using PORO.ViewModels.Base;
using Prism.Navigation;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;

namespace PORO.ViewModels
{
    public class ReviewPageViewModel : BaseViewModel
    {
        public static ReviewPageViewModel Instance { get; private set; }
        public ImageSource _imageReview;
        public ImageSource ImageReview
        {
            get => _imageReview;
            set => SetProperty(ref _imageReview, value);
        }

        public string path { get; set; }

        private DatabaseModel dataModel = new DatabaseModel();

        public ReviewPageViewModel(INavigationService navigationService) : base(navigationService)
        {
            Instance = this;
            PublishCommand = new Command(ExcutePublish);
            EditCommand = new Command(ExcuteEdit);
            BackCommand = new Command(ExcuteBack);
        }

        #region Navigation
        public async override void OnNavigatedTo(INavigationParameters parameters)
        {
            if (parameters != null)
            {
                if (parameters.ContainsKey(ParamKeys.DataModel.ToString()))
                {
                    dataModel = (DatabaseModel)parameters[ParamKeys.DataModel.ToString()];
                    path = dataModel.filepath;
                    ImageReview = ImageSource.FromFile(path);
                }
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

        #region PostImage
        public ICommand PublishCommand { get; set; }
        public async void ExcutePublish()
        {
            NavigationParameters param = new NavigationParameters
                {
                    {ParamKeys.ImageToMint.ToString(), dataModel}
                };
            await Navigation.NavigateAsync(ManagerPage.PublishPage, param, animated: false);
        }
        #endregion
        public string GetPath()
        {
            if (path != null)
            {
                return path;
            }
            else
            {
                return null;
            }
        }

        #region EditImage
        public ICommand EditCommand { get; set; }
        public async void ExcuteEdit()
        {
            NavigationParameters param = new NavigationParameters
                {
                    {ParamKeys.ImageToEdit.ToString(), dataModel.filepath}
                };
            await Navigation.NavigateAsync(ManagerPage.EditPage, param, animated: false);
        }
        #endregion
    }
}
