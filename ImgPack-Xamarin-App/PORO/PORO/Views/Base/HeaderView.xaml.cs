using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace PORO.Views.Base
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class HeaderView : ContentView
    {
        public HeaderView()
        {
            InitializeComponent();
        }

        #region HeaderTitle

        public string PageHeaderTitle
        {
            get => (string)GetValue(PageHeaderTitleProperty);
            set => SetValue(PageHeaderTitleProperty, value);
        }

        public static readonly BindableProperty PageHeaderTitleProperty =
            BindableProperty.Create(nameof(PageHeaderTitle),
                typeof(string),
                typeof(HeaderView),
                string.Empty,
                BindingMode.TwoWay,
                propertyChanged: OnPageHeaderTitleChanged);

        private static void OnPageHeaderTitleChanged(BindableObject bindable, object oldValue, object newValue)
        {
            ((HeaderView)bindable).headerTitle.Text = newValue.ToString();
        }

        #endregion

        #region BackImageCommand

        public ICommand BackImageCommand
        {
            get => (ICommand)GetValue(BackImageCommandProperty);
            set => SetValue(BackImageCommandProperty, value);
        }

        public static readonly BindableProperty BackImageCommandProperty =
            BindableProperty.Create(nameof(BackImageCommand),
                typeof(ICommand),
                typeof(HeaderView),
                propertyChanged: OnBackImageCommandPropertyChanged);

        private static void OnBackImageCommandPropertyChanged(BindableObject bindable, object oldvalue, object newvalue)
        {
            ((HeaderView)bindable).backBtn.GestureRecognizers.Add
                (new TapGestureRecognizer()
                {
                    Command = (ICommand)newvalue,
                    NumberOfTapsRequired = 1
                });
        }

        #endregion

        #region SettingCommand

        public ICommand SettingCommand
        {
            get => (ICommand)GetValue(ShareImageCommandProperty);
            set => SetValue(ShareImageCommandProperty, value);
        }

        public static readonly BindableProperty ShareImageCommandProperty =
            BindableProperty.Create(nameof(SettingCommand),
                typeof(ICommand),
                typeof(HeaderView),
                propertyChanged: OnShareImageCommandPropertyChanged);

        private static void OnShareImageCommandPropertyChanged(BindableObject bindable, object oldvalue, object newvalue)
        {
            ((HeaderView)bindable).send.GestureRecognizers.Add
                (new TapGestureRecognizer()
                {
                    Command = (ICommand)newvalue,
                    NumberOfTapsRequired = 1
                });
        }

        #endregion

        #region IsBackImageVisible

        public bool IsBackImageVisible
        {
            get => (bool)GetValue(IsBackImageVisibleProperty);
            set => SetValue(IsBackImageVisibleProperty, value);
        }

        public static readonly BindableProperty IsBackImageVisibleProperty =
            BindableProperty.Create(nameof(IsBackImageVisible),
                typeof(bool),
                typeof(HeaderView),
                true,
                BindingMode.TwoWay,
                propertyChanged: OnIsBackImageVisiblePropertyChanged);

        private static void OnIsBackImageVisiblePropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            ((HeaderView)bindable).backBtn.IsVisible = (bool)newValue;
        }
        #endregion

        #region IsStatusVisible

        /// <summary>
        /// 
        /// </summary>
        public bool IsSendVisible
        {
            get => (bool)GetValue(IsStatusVisibleProperty);
            set => SetValue(IsStatusVisibleProperty, value);
        }

        public static readonly BindableProperty IsStatusVisibleProperty =
            BindableProperty.Create(nameof(IsSendVisible),
                typeof(bool),
                typeof(HeaderView),
                true,
                BindingMode.TwoWay,
                propertyChanged: OnIsStatusVisiblePropertyChanged);

        private static void OnIsStatusVisiblePropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            ((HeaderView)bindable).send.IsVisible = (bool)newValue;
        }
        #endregion
    }
}