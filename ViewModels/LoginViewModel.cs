using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Services;
using ExpressBase.Mobile.ViewModels.BaseModels;
using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ExpressBase.Mobile.ViewModels
{
    public class LoginViewModel : StaticBaseViewModel
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

        public bool ShowNewSolutionLink => (App.Settings.Vendor.BuildType != AppBuildType.Embedded);

        public bool IsResetVisible => App.Settings.Vendor.HasSolutionSwitcher;

        private readonly IIdentityService identityService;

        private Action<ApiAuthResponse> toggle2FAW;

        private ApiAuthResponse authResponse;

        public Command LoginCommand => new Command(async () => await LoginAction());

        public Command SubmitOtpCommand => new Command(async (o) => await SubmitOtp(o));

        public Command ResendOtpCommand => new Command(async () => await ResendOtp());

        public LoginViewModel()
        {
            identityService = IdentityService.Instance;
        }

        public void Bind2FAToggleEvent(Action<ApiAuthResponse> action)
        {
            toggle2FAW = action;
        }

        public override async Task InitializeAsync()
        {
            this.Email = App.Settings.CurrentSolution?.LastUser;

            LogoUrl = await identityService.GetLogo(App.Settings.Sid);
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
                    authResponse = await identityService.AuthenticateAsync(_username, _password);
                }
                catch (Exception ex)
                {
                    authResponse = null;
                    EbLog.Error("Authentication failed :: " + ex.Message);
                }

                if (authResponse != null && authResponse.IsValid)
                {
                    if (authResponse.Is2FEnabled)
                        toggle2FAW?.Invoke(authResponse);
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
                await identityService.UpdateAuthInfo(authResponse, username);
                await identityService.UpdateLastUser(username, LoginType.CREDENTIALS);

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

        private async Task SubmitOtp(object o)
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
                EbLog.Error("Otp verification failed :: " + ex.Message);
            }

            IsBusy = false;
            if (resp != null && resp.IsValid)
                await AfterLoginSuccess(this.Email.Trim());
            else
                DependencyService.Get<IToast>().Show("The OTP is Invalid or Expired");
        }

        private async Task ResendOtp()
        {
            ApiGenerateOTPResponse resp = null;
            try
            {
                resp = await identityService.GenerateOTP(authResponse);
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
