using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using ExpressBase.Mobile.Services;

namespace ExpressBase.Mobile.Views.Shared
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoginAction : ContentPage
    {
        public LoginAction()
        {
            InitializeComponent();
        }

        private async void GoToLogin_Clicked(object sender, EventArgs e)
        {
            await NavigationService.LoginWithNS();
        }
    }
}