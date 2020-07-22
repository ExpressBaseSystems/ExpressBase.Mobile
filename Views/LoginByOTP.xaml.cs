using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.ViewModels;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ExpressBase.Mobile.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoginByOTP : ContentPage
    {
        private bool isRendered;

        private readonly LoginByOTPViewModel viewModel;

        public LoginByOTP()
        {
            InitializeComponent();
            BindingContext = viewModel = new LoginByOTPViewModel();

            viewModel.Bind2FAToggleEvent(ShowTwoFAWindow);
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
            await Application.Current.MainPage.Navigation.PushAsync(new Login());
        }

        private void BackButton_Clicked(object sender, EventArgs e)
        {
            TwoFAWindow.IsVisible = false;
            BackButton.IsVisible = false;
            RestButton.IsVisible = true;
        }
    }
}