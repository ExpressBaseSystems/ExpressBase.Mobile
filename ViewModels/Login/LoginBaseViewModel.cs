using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Services;
using ExpressBase.Mobile.ViewModels.BaseModels;
using ExpressBase.Mobile.Views;
using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ExpressBase.Mobile.ViewModels.Login
{
    public class LoginBaseViewModel : StaticBaseViewModel
    {
        public ImageSource LogoUrl { set; get; }

        public bool ShowNewSolutionLink => (App.Settings.Vendor.BuildType != AppBuildType.Embedded);

        public bool IsResetVisible => App.Settings.Vendor.HasSolutionSwitcher;

        public bool IsSignUpVisible { set; get; }

        protected readonly IIdentityService Service;

        protected Action<ApiAuthResponse> Toggle2FAW;

        protected ApiAuthResponse AuthResponse;

        public Command SubmitOTPCommand => new Command(async (o) => await SubmitOTP(o));

        public Command ResendOTPCommand => new Command(async () => await ResendOTP());

        public Command SignUpCommand => new Command(async () => await GoToSignUp());

        public LoginBaseViewModel()
        {
            Service = IdentityService.Instance;
            IsSignUpVisible = App.Settings.CurrentSolution.SignupEnabled();
            LogoUrl = CommonServices.GetLogo(App.Settings.Sid);
        }

        protected virtual Task SubmitOTP(object o)
        {
            return Task.FromResult(false);
        }

        protected virtual Task ResendOTP()
        {
            return Task.FromResult(false);
        }

        private async Task GoToSignUp()
        {
            await Application.Current.MainPage.Navigation.PushAsync(new SignUp());
        }

        public void Bind2FAToggleEvent(Action<ApiAuthResponse> action)
        {
            Toggle2FAW = action;
        }
    }
}
