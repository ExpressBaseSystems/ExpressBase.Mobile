using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Services;
using ExpressBase.Mobile.ViewModels.BaseModels;
using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ExpressBase.Mobile.ViewModels
{
    public class LoginByOTPViewModel : StaticBaseViewModel
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

        private ImageSource logourl;

        public ImageSource LogoUrl
        {
            get => logourl;
            set
            {
                logourl = value;
                this.NotifyPropertyChanged();
            }
        }

        private Action<ApiAuthResponse> toggle2FAW;

        private ApiAuthResponse authResponse;

        private readonly IIdentityService identityService;

        public Command SendOTPCommand => new Command(async (o) => await SendOTP());

        public Command SubmitOTPCommand => new Command(async (o) => await SubmitOTP(o));

        public Command ResendOTPCommand => new Command(async () => await ResendOTP());

        public LoginByOTPViewModel()
        {
            identityService = IdentityService.Instance;
        }

        public void Bind2FAToggleEvent(Action<ApiAuthResponse> action)
        {
            toggle2FAW = action;
        }

        public override async Task InitializeAsync()
        {
            LogoUrl = await identityService.GetLogo(App.Settings.Sid);
        }

        private async Task SendOTP()
        {
            string username = this.UserName?.Trim();

            if (string.IsNullOrEmpty(username))
                return;

            IToast toast = DependencyService.Get<IToast>();

            var otpT = OtpType(username);

            IsBusy = true;
            try
            {
                authResponse = await identityService.AuthenticateSSOAsync(username, otpT);

                if (authResponse != null && authResponse.IsValid)
                {
                    if (authResponse.Is2FEnabled)
                        toggle2FAW?.Invoke(authResponse);
                    else
                        toast.Show("OTP service unavailable");
                }
                else
                    toast.Show("wrong username or password.");
            }
            catch (Exception ex)
            {
                EbLog.Write("failed to send otp :: " + ex.Message);
            }

            IsBusy = true;
        }

        private async Task SubmitOTP(object o)
        {
            string otp = o.ToString();
            ApiAuthResponse resp = null;
            IsBusy = true;

            try
            {
                resp = await identityService.VerifyOTP(authResponse, otp);
            }
            catch (Exception ex)
            {
                EbLog.Write("Otp verification failed :: " + ex.Message);
            }

            if (resp != null && resp.IsValid)
            {
                await AfterLoginSuccess(resp, this.UserName);
            }
            else
            {
                DependencyService.Get<IToast>().Show("The OTP is Invalid or Expired");
            }

            IsBusy = false;
        }

        private async Task AfterLoginSuccess(ApiAuthResponse resp, string username)
        {
            try
            {
                await identityService.UpdateAuthInfo(resp, username, string.Empty);
                await identityService.UpdateLastUser(username);

                EbMobileSolutionData data = await App.Settings.GetSolutionDataAsync(true);

                if (App.Settings.Vendor.AllowNotifications)
                {
                    ///update notification hub regid  in background
                    await NotificationService.Instance.UpdateNHRegisratation();
                }

                if (data != null)
                {
                    await identityService.Navigate(data);
                }
            }
            catch (Exception ex)
            {
                EbLog.Write("Exception at after login :: " + ex.Message);
            }
        }

        private async Task ResendOTP()
        {
            ApiGenerateOTPResponse resp = null;

            try
            {
                resp = await identityService.GenerateOTP(authResponse);
            }
            catch (Exception ex)
            {
                EbLog.Write("failed to regenerate otp :: " + ex.Message);
            }

            if (resp != null && resp.IsValid)
            {
                DependencyService.Get<IToast>().Show("OTP sent");
            }
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
