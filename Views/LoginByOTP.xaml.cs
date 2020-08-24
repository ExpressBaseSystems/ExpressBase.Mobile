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
        private bool isRendered;

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

        public void SetContentFromConfig()
        {
            LoginButtonLabel.Text = PageContent["NewSolutionButtonText"];
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            SolutionName.Text = App.Settings.Sid.ToUpper();

            if (!isRendered)
            {
                await viewModel.InitializeAsync();
                isRendered = true;
            }
        }

        public void ShowTwoFAWindow(ApiAuthResponse auth)
        {
            TwoFAWindow.SetAddress(auth.TwoFAToAddress);

            TwoFAWindow.IsVisible = true;
            RestButton.IsVisible = false;
            BackButton.IsVisible = true;
        }

        private async void CredLoginButton_Clicked(object sender, EventArgs e)
        {
            await NavigationService.ReplaceTopAsync(new Login());
        }

        private void BackButton_Clicked(object sender, EventArgs e)
        {
            TwoFAWindow.IsVisible = false;
            BackButton.IsVisible = false;
            RestButton.IsVisible = true;
        }

        private async void NewSolutionButton_Clicked(object sender, EventArgs e)
        {
            await Application.Current.MainPage.Navigation.PushAsync(new NewSolution(true));
        }

        protected override bool OnBackButtonPressed()
        {
            if (TwoFAWindow.IsVisible)
            {
                TwoFAWindow.IsVisible = false;
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