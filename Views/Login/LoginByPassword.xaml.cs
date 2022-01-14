using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.ViewModels.Login;
using ExpressBase.Mobile.Views.Base;
using System;
using System.Collections.Generic;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ExpressBase.Mobile.Views.Login
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoginByPassword : ContentPage, IDynamicContent
    {
        private int backButtonCount;

        private readonly LoginByPasswordViewModel viewModel;

        public Dictionary<string, string> PageContent => App.Settings.Vendor.Content.Login;

        public LoginByPassword()
        {
            InitializeComponent();
            BindingContext = viewModel = new LoginByPasswordViewModel();
            viewModel.Bind2FAToggleEvent(ShowTwoFAWindow);
        }

        public void OnDynamicContentRendering()
        {
            LoginButtonLabel.Text = PageContent["NewSolutionButtonText"];
            SubmitButton.Text = PageContent["LoginButtonText"];
            INativeHelper helper = DependencyService.Get<INativeHelper>();
            DeviceIdButton.Text = helper.DeviceId;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            SolutionName.Text = App.Settings.SolutionName;
        }

        private void Email_Completed(object sender, EventArgs e)
        {
            PassWord.Focus();
        }

        private async void DeviceId_Clicked(object sender, EventArgs e)
        {
            INativeHelper helper = DependencyService.Get<INativeHelper>();
            await Clipboard.SetTextAsync(helper.DeviceId);
            Utils.Toast("Copied to clipboard");
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
            TwoFAWindow.Show();
        }

        private async void NewSolutionButton_Clicked(object sender, EventArgs e)
        {
            await App.Navigation.NavigateAsync(new NewSolution(true));
        }

        private async void SSOLoginButton_Clicked(object sender, EventArgs e)
        {
            await viewModel.ReplaceTopAsync(new LoginByOTP());
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
                    Utils.Toast("Press again to EXIT!");
                });
            }
            return true;
        }
    }
}