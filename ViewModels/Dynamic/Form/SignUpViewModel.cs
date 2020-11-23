using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Services;
using ExpressBase.Mobile.Views;
using ExpressBase.Mobile.Views.Dynamic;
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

        public Command GoToLoginCommand => new Command(async () => await App.Navigation.NavigateToLogin());

        private EbSignUpUserInfo createdUser;

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
            Device.BeginInvokeOnMainThread(() => IsBusy = true);

            FormSaveResponse response = await this.Form.Save(this.RowId);

            if (response.Status)
            {
                createdUser = response.PushResponse.GetSignUpUserInfo();

                if (createdUser == null)
                {
                    EbLog.Info("[EbSignUpUserInfo] null, user not created due to some unknown error");
                    Utils.Toast("unable to signup");
                    return;
                }

                if (createdUser.VerificationRequired)
                {
                    Device.BeginInvokeOnMainThread(() => OTPWindowVisibility = true);
                }
                else
                {
                    try
                    {
                        ApiAuthResponse authResponse = await IdentityService.Instance.AuthenticateSSOAsync(createdUser.UserName, createdUser.AuthId, createdUser.Token);

                        if (authResponse != null && authResponse.IsValid)
                            await AfterAuthenticationSuccess(authResponse, createdUser);
                        else
                        {
                            EbLog.Error("sso authentication failed [auth-response] null");
                            throw new Exception("sso authentication failed [auth-response] null");
                        }
                    }
                    catch (Exception ex)
                    {
                        EbLog.Error("sso authenticate api error , EXCEPTION ::" + ex.Message);
                        Utils.Toast("unable to signup");
                    }
                }
            }
            else
            {
                EbLog.Info("signup form save error , [push-response] status false, " + response.Message);
                Utils.Toast("unable to signup");
            }
            Device.BeginInvokeOnMainThread(() => IsBusy = false);
        }

        private async Task AfterAuthenticationSuccess(ApiAuthResponse resp, EbSignUpUserInfo userInfo)
        {
            try
            {
                await IdentityService.Instance.UpdateAuthInfo(resp, userInfo.UserName);
                await IdentityService.Instance.UpdateLastUser(userInfo.UserName, LoginType.SSO);

                EbMobileSolutionData data = await App.Settings.GetSolutionDataAsync(true, callback: status =>
                {
                    Utils.Alert_SlowNetwork();
                });

                if (data != null)
                {
                    if (App.Settings.Vendor.AllowNotifications)
                        await NotificationService.Instance.UpdateNHRegistration();

                    if (data.ProfilePages != null && data.ProfilePages.Count > 0)
                        await ProfileSetUp(data, userInfo);
                    else
                        await IdentityService.Instance.Navigate(data);
                }
            }
            catch (Exception ex)
            {
                EbLog.Error("exception at authentication success, " + ex.Message);
            }
        }

        private async Task ProfileSetUp(EbMobileSolutionData data, EbSignUpUserInfo userInfo)
        {
            try
            {
                if (userInfo.UserType <= 0)
                {
                    throw new Exception("created user type id is [0], skipping proile pages");
                }

                EbProfileUserType userType = App.Settings.CurrentSolution?.GetUserTypeById(userInfo.UserType);

                if (userType != null && userType.HasUserTypeForm())
                {
                    EbLog.Info($"signup user type [{userType.Name}] with id [{userType.Id}]");

                    EbMobilePage page = data.ProfilePages.Find(x => x.RefId == userType.RefId)?.GetPage();

                    if (page != null && page.Container is EbMobileForm form)
                    {
                        form.RenderingAsExternal = true;
                        form.RAERedirectionType = typeof(MyApplications);

                        await App.Navigation.NavigateAsync(new FormRender(page));
                    }
                    else
                    {
                        throw new Exception("user type form does not exist in [solutiondata]");
                    }
                }
                else
                {
                    throw new Exception("profile user type [null] or [refid] empty");
                } 
            }
            catch (Exception ex)
            {
                EbLog.Error(ex.Message);
                await IdentityService.Instance.Navigate(data);
            }
        }

        protected async Task SubmitOTP(object o)
        {
            if (o == null) return;
            string otp = o.ToString();
            try
            {
                ApiAuthResponse authResponse = await IdentityService.Instance.VerifyUserByOTP(createdUser.Token, createdUser.AuthId, otp);

                if (authResponse != null && authResponse.IsValid)
                    await AfterAuthenticationSuccess(authResponse, createdUser);
                else
                    throw new Exception("userverification api response [null] or [invalid]");
            }
            catch (Exception ex)
            {
                EbLog.Error("user verification on signup error, " + ex.Message);
                Utils.Toast("unable to signup");
            }
        }
    }
}
