using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.CustomControls;
using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Extensions;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Services;
using ExpressBase.Mobile.Views;
using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ExpressBase.Mobile.ViewModels.Login
{
    public class LoginByPasswordViewModel : LoginBaseViewModel
    {
        private string email;
        private string password;
        private Loader msgLoader;

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

        public Command RefreshDataCommand => new Command(async () => await UpdateAsync());

        public LoginByPasswordViewModel(Loader loader) : base()
        {
            this.Email = App.Settings.CurrentSolution?.LastUser;
            this.msgLoader = loader;
        }

        private async Task LoginAction()
        {
            msgLoader.IsVisible = true;
            msgLoader.Message = "Logging in...";

            if (this.CanLogin())
            {
                string _username = this.Email.Trim();
                string _password = this.PassWord.Trim();
                string _md5PassCode = string.Concat(_password, _username, App.Settings.Sid).ToMD5();

                if (!Utils.HasInternet)
                {
                    if (App.Settings?.SyncInfo?.IsLoggedOut == true && App.Settings.SyncInfo.Md5PassCode != null)
                    {
                        if (App.Settings.SyncInfo.Md5PassCode == _md5PassCode)
                        {
                            string msg = EbPageHelper.GetFormRenderInvalidateMsg(NetworkMode.Offline);
                            if (msg == null)
                            {
                                App.Settings.SyncInfo.IsLoggedOut = false;
                                await Store.SetJSONAsync(AppConst.LAST_SYNC_INFO, App.Settings.SyncInfo);
                                if (App.Settings.AppId <= 0)
                                    await App.Navigation.NavigateAsync(new MyApplications());
                                else
                                {
                                    App.RootMaster = new RootMaster();
                                    Application.Current.MainPage = App.RootMaster;
                                }
                            }
                            else
                                Utils.Toast("Offline login is not available. " + msg);
                        }
                        else
                            Utils.Toast("Wrong username or password.");
                    }
                    else
                        Utils.Alert_NoInternet();
                    msgLoader.IsVisible = false;
                    return;
                }

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
                    {
                        App.Settings.SyncInfo.Md5PassCode = _md5PassCode;
                        await Store.SetJSONAsync(AppConst.LAST_SYNC_INFO, App.Settings.SyncInfo);
                        await AfterLoginSuccess(AuthResponse, _username, LoginType.CREDENTIALS, msgLoader);
                    }
                }
                else
                {
                    if (AuthResponse?.Message != null)
                        Utils.Toast(AuthResponse.Message);
                    else
                        Utils.Toast("Authentication failed");
                }

                IsBusy = false;
            }
            else
                Utils.Toast("Email/Password cannot be empty");
            msgLoader.IsVisible = false;
        }

        protected override async Task SubmitOTP(object o)
        {
            string otp = o?.ToString();

            msgLoader.IsVisible = true;
            msgLoader.Message = "Logging in...";

            try
            {
                ApiAuthResponse resp = await Service.VerifyOTP(AuthResponse, otp);

                if (resp != null && resp.IsValid)
                    await AfterLoginSuccess(resp, this.Email.Trim(), LoginType.CREDENTIALS, msgLoader);
                else
                    Utils.Toast("The OTP is Invalid or Expired");
            }
            catch (Exception ex)
            {
                EbLog.Error("Otp verification failed :: " + ex.Message);
            }
            msgLoader.IsVisible = false;
            IsBusy = false;
        }

        private bool CanLogin()
        {
            if (string.IsNullOrWhiteSpace(this.Email) || string.IsNullOrWhiteSpace(this.PassWord))
                return false;
            return true;
        }

        public override async Task UpdateAsync()
        {
            IsRefreshing = true;

            if (App.Settings?.Vendor?.BuildType == AppBuildType.Embedded)
            {
                try
                {
                    ISolutionService service = new SolutionService();

                    ValidateSidResponse response = await service.ValidateSid(App.Settings?.Vendor?.SolutionURL);
                    if (response != null && response.IsValid)
                    {
                        SolutionInfo sln = Store.GetJSON<SolutionInfo>(AppConst.SOLUTION_OBJ);
                        sln.SignUpPage = response.SignUpPage;
                        sln.SolutionObject = response.SolutionObj;
                        App.Settings.CurrentSolution = sln;
                        await Store.SetJSONAsync(AppConst.SOLUTION_OBJ, sln);
                    }
                }
                catch (Exception ex)
                {
                    Utils.Toast("Failed to refresh: " + ex.Message);
                }
            }
            IsRefreshing = false;
        }
    }
}
