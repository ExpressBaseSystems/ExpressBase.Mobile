using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Services;
using ExpressBase.Mobile.ViewModels.BaseModels;
using ExpressBase.Mobile.Views;
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

            string _username = this.Email.Trim();
            string _password = this.PassWord.Trim();

            if (this.CanLogin())
            {
                IsBusy = true;

                try
                {
                    authResponse = await identityService.AuthenticateAsync(_username, _password);
                }
                catch (Exception ex)
                {
                    authResponse = null;
                    EbLog.Write("Authentication failed :: " + ex.Message);
                }

                if (authResponse != null && authResponse.IsValid)
                {
                    if (authResponse.Is2FEnabled)
                        toggle2FAW?.Invoke(authResponse);
                    else
                        await AfterLoginSuccess(_username, _password);
                }
                else
                    toast.Show("wrong username or password.");

                IsBusy = false;
            }
            else
                toast.Show("Email/Password cannot be empty");
        }

        private async Task AfterLoginSuccess(string username, string password)
        {
            try
            {
                await identityService.UpdateAuthInfo(authResponse, username, password);
                await identityService.UpdateLastUser(username);

                EbMobileSolutionData data = await App.Settings.GetSolutionDataAsync(true);

                if (App.Settings.Vendor.AllowNotifications)
                {
                    ///update notification hub regid  in background
                    await NotificationService.Instance.UpdateNHRegisratation();
                }

                if (data != null)
                {
                    await Navigate(data);
                }
            }
            catch (Exception ex)
            {
                EbLog.Write("Exception at after login :: " + ex.Message);
            }
        }

        private async Task SubmitOtp(object o)
        {
            string otp = o.ToString();
            ApiTwoFactorResponse resp = null;
            IsBusy = true;

            try
            {
                resp = await identityService.VerifyOrGenerate2FA(authResponse, otp, false);
            }
            catch (Exception ex)
            {
                EbLog.Write("Otp verification failed :: " + ex.Message);
            }

            IsBusy = false;
            if (resp != null && resp.IsValid)
                await AfterLoginSuccess(this.Email.Trim(), this.PassWord.Trim());
            else
                DependencyService.Get<IToast>().Show("The OTP is Invalid or Expired");
        }

        private async Task ResendOtp()
        {
            ApiTwoFactorResponse resp = null;

            try
            {
                resp = await identityService.VerifyOrGenerate2FA(authResponse, string.Empty, true);
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

        private async Task Navigate(EbMobileSolutionData data)
        {
            if (data.Applications != null)
            {
                if (data.Applications.Count == 1)
                {
                    AppData appdata = data.Applications[0];

                    await Store.SetJSONAsync(AppConst.CURRENT_APP, appdata);
                    App.Settings.CurrentApplication = appdata;
                    App.Settings.MobilePages = appdata.MobilePages;

                    App.RootMaster = new RootMaster(typeof(Home));
                    Application.Current.MainPage = App.RootMaster;
                }
                else
                {
                    await Application.Current.MainPage.Navigation.PushAsync(new MyApplications());
                }
            }
        }

        bool CanLogin()
        {
            if (string.IsNullOrEmpty(this.Email) || string.IsNullOrEmpty(this.PassWord))
                return false;
            return true;
        }
    }
}
