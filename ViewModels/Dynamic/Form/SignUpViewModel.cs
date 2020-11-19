using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Services;
using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ExpressBase.Mobile.ViewModels.Dynamic
{
    public class SignUpViewModel : FormRenderViewModel
    {
        private bool _OTPWindowVisibility;

        public bool OTPWindowVisibility
        {
            get => _OTPWindowVisibility;
            set
            {
                _OTPWindowVisibility = value;
                NotifyPropertyChanged();
            }
        }

        public ImageSource LogoUrl { set; get; }

        public Command SubmitOTPCommand => new Command(async (o) => await SubmitOTP(o));

        public Command GoToLoginCommand => new Command(async () => await GoToLogin());

        public SignUpViewModel(EbMobilePage page) : base(page)
        {
            this.LogoUrl = CommonServices.GetLogo(App.Settings.Sid);
        }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            await AutheticateAnonymous();
        }

        private async Task AutheticateAnonymous()
        {
            ApiAuthResponse authResp = await IdentityService.Instance.AuthenticateAsync("NIL", "NIL", anonymous: true);

            if (authResp != null && authResp.IsValid)
            {
                App.Settings.RToken = authResp.RToken;
                App.Settings.BToken = authResp.BToken;
                App.Settings.CurrentUser = authResp.User;
            }
        }

        protected override async Task Submit()
        {
            try
            {
                Device.BeginInvokeOnMainThread(() => IsBusy = true);

                FormSaveResponse response = await this.Form.Save(this.RowId);

                if (response.Status)
                {
                    EbSignUpUserInfo signupUser = response.PushResponse.GetSignUpUserInfo();

                    if (signupUser == null)
                    {
                        EbLog.Info("[EbSignUpUserInfo] null, user not created due to some unknown error");
                        Utils.Toast("unable to signup");
                        return;
                    }

                    try
                    {
                        if (signupUser.VerificationRequired)
                        {
                            Device.BeginInvokeOnMainThread(() => OTPWindowVisibility = true);
                        }
                        else
                        {
                            var authResponse = await IdentityService.Instance.AuthenticateSSOAsync(signupUser.UserName, signupUser.AuthId, signupUser.Token);

                            if (authResponse != null && authResponse.IsValid)
                            {
                                await AfterLoginSuccess(authResponse, signupUser.UserName, LoginType.SSO);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        EbLog.Info(ex.Message);
                    }
                }

                EbLog.Info($"{this.PageName} save status '{response.Status}'");
                EbLog.Info(response.Message);
            }
            catch (Exception ex)
            {
                EbLog.Info($"Signup error");
                EbLog.Error(ex.Message);
            }
            Device.BeginInvokeOnMainThread(() => IsBusy = false);
        }

        private async Task GoToLogin()
        {
            await App.Navigation.NavigateToLogin();
        }

        private async Task AfterLoginSuccess(ApiAuthResponse resp, string username, LoginType loginType)
        {
            try
            {
                await IdentityService.Instance.UpdateAuthInfo(resp, username);
                await IdentityService.Instance.UpdateLastUser(username, loginType);

                EbMobileSolutionData data = await App.Settings.GetSolutionDataAsync(true, callback: status =>
                {
                    Utils.Alert_SlowNetwork();
                });

                if (App.Settings.Vendor.AllowNotifications)
                    await NotificationService.Instance.UpdateNHRegistration();

                if (data != null)
                    await IdentityService.Instance.Navigate(data);
            }
            catch (Exception ex)
            {
                EbLog.Error("Exception at after login :: " + ex.Message);
            }
        }

        protected async Task SubmitOTP(object o)
        {

        }
    }
}
