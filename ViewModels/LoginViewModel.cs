using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Services;
using ExpressBase.Mobile.Views;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace ExpressBase.Mobile.ViewModels
{
    public class LoginViewModel: BaseViewModel
    {

        private string email;
        public string Email
        {
            get
            {
                return this.email;
            }
            set
            {
                if (this.email == value)
                {
                    return;
                }
                this.email = value;
                this.NotifyPropertyChanged();
            }
        }

        private string password;
        public string PassWord
        {
            get
            {
                return this.password;
            }
            set
            {
                if (this.password == value)
                {
                    return;
                }
                this.password = value;
                this.NotifyPropertyChanged();
            }
        }

        public LoginViewModel()
        {
            this.LoginCommand = new Command(LoginAction);
        }

        public Command LoginCommand { set; get; }

        private void LoginAction(object obj)
        {
            string _username = this.Email.Trim();
            string _password = this.PassWord.Trim();

            if (CanLogin())
            {
                ApiAuthResponse response = Auth.TryAuthenticate(_username, _password);
                if (response.IsValid)
                {
                    Auth.UpdateStore(response, _username, password);
                    Application.Current.MainPage.Navigation.PushAsync(new AppSelect());
                }
            }
        }

        private bool CanLogin()
        {
            if (string.IsNullOrWhiteSpace(this.Email) || string.IsNullOrWhiteSpace(this.PassWord))
                return false;

            return true;
        }
    }
}
