using PORO.Untilities;
using Rg.Plugins.Popup.Extensions;
using Rg.Plugins.Popup.Pages;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace PORO.Controls
{
    public class PopupBasePage : PopupPage
    { 
       #region Constructors

        public PopupBasePage()
        { }

        #endregion

        #region Properties

        protected bool IsAutoClose;
        protected bool IsClosed;
        protected uint Duration;
        protected int _popupId;

        #endregion

        #region Events

        protected async void ClosedPopupEvent(object sender, EventArgs e)
        {
            await ClosedPopup();
        }

        protected void DoNothingEvent(object sender, EventArgs e)
        {
            // Do nothing
        }

        protected async void AutoClosedPopupAfter(uint duration)
        {
            int id = _popupId;
            await Task.Delay((int)duration);

            // If the popup still appear after duration time then close the popup
            if (_popupId == id)
                await ClosedPopup();
        }

        #endregion

        #region ClosedPopupCommand

        public static readonly BindableProperty ClosedPopupCommandProperty =
            BindableProperty.Create(nameof(ClosedPopupCommand),
                typeof(ICommand),
                typeof(PopupBasePage),
                null,
                BindingMode.TwoWay);

        public ICommand ClosedPopupCommand
        {
            get => (ICommand)GetValue(ClosedPopupCommandProperty);
            set => SetValue(ClosedPopupCommandProperty, value);
        }

        public static readonly BindableProperty ClosedPopupCommandParameterProperty =
            BindableProperty.Create(nameof(ClosedPopupCommandParameter),
                typeof(object),
                typeof(PopupBasePage),
                null,
                BindingMode.TwoWay);

        public object ClosedPopupCommandParameter
        {
            get => GetValue(ClosedPopupCommandParameterProperty);
            set => SetValue(ClosedPopupCommandParameterProperty, value);
        }

        private async Task ClosedPopup()
        {
            await DeviceExtension.BeginInvokeOnMainThreadAsync(async () =>
            {
                await Navigation.PopPopupAsync();
            });

            ClosedPopupCommand?.Execute(ClosedPopupCommandParameter);
            _popupId++;

            IsClosed = true;
        }

        #endregion

        #region OnDisappearing

        protected override void OnDisappearing()
        {
            IsClosed = true;
            base.OnDisappearing();
        }

        #endregion

        #region Animation

        // Invoked before an animation appearing
        protected override void OnAppearingAnimationBegin()
        {
            base.OnAppearingAnimationBegin();
        }

        // Invoked after an animation appearing
        protected override void OnAppearingAnimationEnd()
        {
            base.OnAppearingAnimationEnd();
        }

        // Invoked before an animation disappearing
        protected override void OnDisappearingAnimationBegin()
        {
            base.OnDisappearingAnimationBegin();
        }

        // Invoked after an animation disappearing
        protected override void OnDisappearingAnimationEnd()
        {
            base.OnDisappearingAnimationEnd();
        }

        #endregion
    }
}
