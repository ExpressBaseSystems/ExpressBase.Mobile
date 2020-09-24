using ExpressBase.Mobile.CustomControls;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Services;
using ExpressBase.Mobile.ViewModels;
using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ExpressBase.Mobile.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoginByOTP : ContentPage, IDynamicContent
    {
        private int backButtonCount;

        private readonly LoginByOTPViewModel viewModel;

        public Dictionary<string, string> PageContent => App.Settings.Vendor.Content.Login;

        public LoginByOTP()
        {
            InitializeComponent();
            BindingContext = viewModel = new LoginByOTPViewModel();

            SetContentFromConfig();
            viewModel.Bind2FAToggleEvent(ShowTwoFAWindow);
        }

        private void StartOTPListner()
        {
            MessagingCenter.Subscribe<string>(this, "ReceivedOTP", (message) =>
            {

            });
        }

        public void SetContentFromConfig()
        {
            LoginButtonLabel.Text = PageContent["NewSolutionButtonText"];
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            SolutionName.Text = App.Settings.Sid.ToUpper();
        }

        public void ShowTwoFAWindow(ApiAuthResponse auth)
        {
            TwoFAWindow.SetAddress(auth.TwoFAToAddress);

            TwoFAWindow.Show();
            RestButton.IsVisible = false;
        }

        private async void CredLoginButton_Clicked(object sender, EventArgs e)
        {
            await NavigationService.ReplaceTopAsync(new Login());
        }

        private async void NewSolutionButton_Clicked(object sender, EventArgs e)
        {
            await Application.Current.MainPage.Navigation.PushAsync(new NewSolution(true));
        }

        protected override bool OnBackButtonPressed()
        {
            if (TwoFAWindow.IsVisible)
            {
                TwoFAWindow.Hide();
                return true;
            }

            backButtonCount++;

            if (backButtonCount == 2)
            {
                backButtonCount = 0;
                DependencyService.Get<INativeHelper>().Close();
            }
            else
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    DependencyService.Get<IToast>().Show("Press again to EXIT!");
                });
            }

            return true;
        }
    }
}