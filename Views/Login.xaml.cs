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
    public partial class Login : ContentPage, IDynamicContent
    {
        private bool isRendered;

        private int backButtonCount;

        private readonly LoginViewModel viewModel;

        public Dictionary<string, string> PageContent => App.Settings.Vendor.Content.Login;

        public Login()
        {
            InitializeComponent();
            BindingContext = viewModel = new LoginViewModel();

            SetContentFromConfig();
            viewModel.Bind2FAToggleEvent(ShowTwoFAWindow);
        }

        public void SetContentFromConfig()
        {
            LoginButtonLabel.Text = PageContent["NewSolutionButtonText"];
            SubmitButton.Text = PageContent["LoginButtonText"];
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

        private void Email_Completed(object sender, EventArgs e)
        {
            PassWord.Focus();
        }

        private void ShowPassword_Clicked(object sender, EventArgs e)
        {
            PassWord.IsPassword = false;
            ShowPassword.IsVisible = false;
            HidePassword.IsVisible = true;
            if (PassWord.Text != null)
                PassWord.CursorPosition = PassWord.Text.Length;
        }

        private void HidePassword_Clicked(object sender, EventArgs e)
        {
            PassWord.IsPassword = true;
            ShowPassword.IsVisible = true;
            HidePassword.IsVisible = false;
            if (PassWord.Text != null)
                PassWord.CursorPosition = PassWord.Text.Length;
        }

        public void ShowTwoFAWindow(ApiAuthResponse auth)
        {
            TwoFAWindow.SetAddress(auth.TwoFAToAddress);

            TwoFAWindow.IsVisible = true;
            RestButton.IsVisible = false;
            BackButton.IsVisible = true;
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

        private async void SSOLoginButton_Clicked(object sender, EventArgs e)
        {
            await NavigationService.ReplaceTopAsync(new LoginByOTP());
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