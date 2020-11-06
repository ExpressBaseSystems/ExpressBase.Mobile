using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

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
            await App.Navigation.NavigateToLogin(true);
        }
    }
}