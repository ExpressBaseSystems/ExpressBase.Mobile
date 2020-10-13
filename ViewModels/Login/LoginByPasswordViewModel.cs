using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Services;
using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ExpressBase.Mobile.ViewModels.Login
{
    public class LoginByPasswordViewModel : LoginBaseViewModel
    {
        private string email;

        public string Email
        {
            get => this.email;
            set
            {
                this.email = value;
                this.NotifyPropertyChanged();
            }
        }

        private string password;

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
            this.LogoUrl = CommonServices.GetLogo(App.Settings.Sid);
        }

        private async Task LoginAction()
        {
            IToast toast = DependencyService.Get<IToast>();

            if (!Utils.HasInternet)
            {
                toast.Show("Not connected to internet!");
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
                        await AfterLoginSuccess(_username);
                }
                else
                    toast.Show("wrong username or password.");

                IsBusy = false;
            }
            else
                toast.Show("Email/Password cannot be empty");
        }

        private async Task AfterLoginSuccess(string username)
        {
            try
            {
                await Service.UpdateAuthInfo(AuthResponse, username);
                await Service.UpdateLastUser(username, LoginType.CREDENTIALS);

                EbMobileSolutionData data = await App.Settings.GetSolutionDataAsync(true, callback: status =>
                {
                    Utils.Alert_SlowNetwork();
                });

                if (App.Settings.Vendor.AllowNotifications)
                    await NotificationService.Instance.UpdateNHRegisratation();

                if (data != null)
                    await Service.Navigate(data);
            }
            catch (Exception ex)
            {
                EbLog.Error("Exception at after login :: " + ex.Message);
            }
        }

        protected override async Task SubmitOTP(object o)
        {
            string otp = o.ToString();
            ApiAuthResponse resp = null;
            IsBusy = true;

            try
            {
                resp = await Service.VerifyOTP(AuthResponse, otp);
            }
            catch (Exception ex)
            {
                EbLog.Error("Otp verification failed :: " + ex.Message);
            }

            IsBusy = false;
            if (resp != null && resp.IsValid)
                await AfterLoginSuccess(this.Email.Trim());
            else
                DependencyService.Get<IToast>().Show("The OTP is Invalid or Expired");
        }

        protected override async Task ResendOTP()
        {
            ApiGenerateOTPResponse resp = null;
            try
            {
                resp = await Service.GenerateOTP(AuthResponse);
            }
            catch (Exception ex)
            {
                EbLog.Error("failed to regenerate otp :: " + ex.Message);
            }

            if (resp != null && resp.IsValid)
                DependencyService.Get<IToast>().Show("OTP sent");
        }

        bool CanLogin()
        {
            if (string.IsNullOrEmpty(this.Email) || string.IsNullOrEmpty(this.PassWord))
                return false;
            return true;
        }
    }
}
