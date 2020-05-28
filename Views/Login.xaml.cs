using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.ViewModels;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ExpressBase.Mobile.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Login : ContentPage
    {
        private bool isRendered;

        private readonly LoginViewModel ViewModel;

        public Login()
        {
            InitializeComponent();
            BindingContext = ViewModel = new LoginViewModel();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (!isRendered)
            {
                SolutionName.Text = App.Settings.Sid;

                await ViewModel.InitializeAsync();
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

        protected override bool OnBackButtonPressed()
        {
            return true;
        }
    }
}