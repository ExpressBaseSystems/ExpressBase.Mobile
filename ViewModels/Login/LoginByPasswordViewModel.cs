﻿using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ExpressBase.Mobile.ViewModels.Login
{
    public class LoginByPasswordViewModel : LoginBaseViewModel
    {
        private string email;
        private string password;

        public string Email
        {
            get => this.email;
            set
            {
                this.email = value;
                this.NotifyPropertyChanged();
            }
        }

        public string PassWord
        {
            get => this.password;
            set
            {
                this.password = value;
                this.NotifyPropertyChanged();
            }
        }

        public Command LoginCommand => new Command(async () => await LoginAction());

        public LoginByPasswordViewModel() : base()
        {
            this.Email = App.Settings.CurrentSolution?.LastUser;
        }

        private async Task LoginAction()
        {
            if (!Utils.HasInternet)
            {
                Utils.Alert_NoInternet();
                return;
            }

            if (this.CanLogin())
            {
                string _username = this.Email.Trim();
                string _password = this.PassWord.Trim();
                IsBusy = true;
                try
                {
                    AuthResponse = await Service.AuthenticateAsync(_username, _password);
                }
                catch (Exception ex)
                {
                    AuthResponse = null;
                    EbLog.Error("Authentication failed :: " + ex.Message);
                }

                if (AuthResponse != null && AuthResponse.IsValid)
                {
                    if (AuthResponse.Is2FEnabled)
                        Toggle2FAW?.Invoke(AuthResponse);
                    else
                        await AfterLoginSuccess(AuthResponse, _username, LoginType.CREDENTIALS);
                }
                else
                    Utils.Toast("wrong username or password.");

                IsBusy = false;
            }
            else
                Utils.Toast("Email/Password cannot be empty");
        }

        protected override async Task SubmitOTP(object o)
        {
            string otp = o?.ToString();
            IsBusy = true;
            try
            {
                ApiAuthResponse resp = await Service.VerifyOTP(AuthResponse, otp);

                if (resp != null && resp.IsValid)
                    await AfterLoginSuccess(resp, this.Email.Trim(), LoginType.CREDENTIALS);
                else
                    Utils.Toast("The OTP is Invalid or Expired");
            }
            catch (Exception ex)
            {
                EbLog.Error("Otp verification failed :: " + ex.Message);
            }
            IsBusy = false;
        }

        private bool CanLogin()
        {
            if (string.IsNullOrEmpty(this.Email) || string.IsNullOrEmpty(this.PassWord))
                return false;
            return true;
        }
    }
}
