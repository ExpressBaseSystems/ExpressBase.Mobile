using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Services;
using System;
using System.Net.Mail;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ExpressBase.Mobile.ViewModels.Login
{
    public class LoginByOTPViewModel : LoginBaseViewModel
    {
        private string username;

        public string UserName
        {
            get => this.username;
            set
            {
                this.username = value;
                this.NotifyPropertyChanged();
            }
        }

        public Command SendOTPCommand => new Command(async (o) => await SendOTP());

        public LoginByOTPViewModel() : base()
        {
            this.UserName = App.Settings.CurrentSolution?.LastUser;
        }

        private async Task SendOTP()
        {
            string username = this.UserName?.Trim();

            if (string.IsNullOrEmpty(username))
                return;

            IToast toast = DependencyService.Get<IToast>();
            SignInOtpType otpT = OtpType(username);

            IsBusy = true;
            try
            {
                AuthResponse = await Service.AuthenticateSSOAsync(username, otpT);

                if (AuthResponse != null && AuthResponse.IsValid)
                {
                    Toggle2FAW?.Invoke(AuthResponse);
                }
                else
                    toast.Show("username or mobile invalid");
            }
            catch (Exception ex)
            {
                EbLog.Error("Failed to send otp :: " + ex.Message);
            }
            IsBusy = false;
        }

        protected override async Task SubmitOTP(object o)
        {
            string otp = o.ToString();

            if (!Service.IsValidOTP(otp)) return;

            IsBusy = true;
            try
            {
                ApiAuthResponse resp = await Service.VerifyOTP(AuthResponse, otp);

                if (resp != null && resp.IsValid)
                    await AfterLoginSuccess(resp, this.UserName);
                else
                    DependencyService.Get<IToast>().Show("The OTP is Invalid or Expired");
            }
            catch (Exception ex)
            {
                EbLog.Error("OTP verification failed :: " + ex.Message);
            }

            IsBusy = false;
        }

        private async Task AfterLoginSuccess(ApiAuthResponse resp, string username)
        {
            try
            {
                await Service.UpdateAuthInfo(resp, username);
                await Service.UpdateLastUser(username);

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

        protected override async Task ResendOTP()
        {
            ApiGenerateOTPResponse resp = null;
            try
            {
                resp = await Service.GenerateOTP(AuthResponse);
            }
            catch (Exception ex)
            {
                EbLog.Error("Failed to regenerate otp :: " + ex.Message);
            }

            if (resp != null && resp.IsValid)
                DependencyService.Get<IToast>().Show("OTP sent");
        }

        private SignInOtpType OtpType(string username)
        {
            if (IsEmail(username))
                return SignInOtpType.Email;
            else
                return SignInOtpType.Sms;
        }

        private bool IsEmail(string username)
        {
            try
            {
                MailAddress m = new MailAddress(username);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
