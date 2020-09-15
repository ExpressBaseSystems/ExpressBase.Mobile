using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Services;
using ExpressBase.Mobile.ViewModels.BaseModels;
using System;
using System.Net.Mail;
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

        public ImageSource LogoUrl { set; get; }

        private Action<ApiAuthResponse> toggle2FAW;

        private ApiAuthResponse authResponse;

        private readonly IIdentityService identityService;

        public bool ShowNewSolutionLink => (App.Settings.Vendor.BuildType != AppBuildType.Embedded);

        public bool IsResetVisible => App.Settings.Vendor.HasSolutionSwitcher;

        public Command SendOTPCommand => new Command(async (o) => await SendOTP());

        public Command SubmitOTPCommand => new Command(async (o) => await SubmitOTP(o));

        public Command ResendOTPCommand => new Command(async () => await ResendOTP());

        public LoginByOTPViewModel()
        {
            identityService = IdentityService.Instance;

            this.UserName = App.Settings.CurrentSolution?.LastUser;
            this.LogoUrl = CommonServices.GetLogo(App.Settings.Sid);
        }

        public void Bind2FAToggleEvent(Action<ApiAuthResponse> action)
        {
            toggle2FAW = action;
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
                authResponse = await identityService.AuthenticateSSOAsync(username, otpT);

                if (authResponse != null && authResponse.IsValid)
                    toggle2FAW?.Invoke(authResponse);
                else
                    toast.Show("username or mobile invalid");
            }
            catch (Exception ex)
            {
                EbLog.Error("Failed to send otp :: " + ex.Message);
            }

            IsBusy = false;
        }

        private async Task SubmitOTP(object o)
        {
            string otp = o.ToString();

            if (!identityService.IsValidOTP(otp)) return;

            IsBusy = true;
            try
            {
                ApiAuthResponse resp = await identityService.VerifyOTP(authResponse, otp);

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
                await identityService.UpdateAuthInfo(resp, username);
                await identityService.UpdateLastUser(username);

                EbMobileSolutionData data = await App.Settings.GetSolutionDataAsync(true, callback: status =>
                {
                    Utils.Alert_SlowNetwork();
                });

                if (App.Settings.Vendor.AllowNotifications)
                    await NotificationService.Instance.UpdateNHRegisratation();

                if (data != null)
                    await identityService.Navigate(data);
            }
            catch (Exception ex)
            {
                EbLog.Error("Exception at after login :: " + ex.Message);
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
