using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.CustomControls;
using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Extensions;
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
                    string _passCode = Store.GetJSON<string>(AppConst.MD5_PASSCODE);

                    if (_passCode != null)
                    {
                        if (_passCode == _md5PassCode)
                        {
                            string msg = EbPageHelper.GetFormRenderInvalidateMsg(NetworkMode.Offline);
                            if (msg == null)
                            {
                                EbMobileSolutionData data = App.Settings.GetOfflineSolutionDataAsync();
                                await Service.Navigate(data);
                                Utils.Toast("Offline login with limited access!");
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
                    Store.ResetCashedSolutionData();
                    if (AuthResponse.Is2FEnabled)
                        Toggle2FAW?.Invoke(AuthResponse);
                    else
                    {
                        await Store.SetJSONAsync(AppConst.MD5_PASSCODE, _md5PassCode);
                        await AfterLoginSuccess(AuthResponse, _username, LoginType.CREDENTIALS, msgLoader);
                    }
                }
                else
                    Utils.Toast("Wrong username or password.");

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
    }
}
