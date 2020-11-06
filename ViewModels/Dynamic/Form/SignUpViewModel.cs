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
        public ImageSource LogoUrl { set; get; }

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
            ApiAuthResponse authResp = await IdentityService.Instance.AuthenticateAsync("NIL", "NIL", true);

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

                Device.BeginInvokeOnMainThread(async () =>
                {
                    if (response.Status)
                    {

                    }
                    IsBusy = false;
                    EbLog.Info($"{this.PageName} save status '{response.Status}'");
                    EbLog.Info(response.Message);
                });
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
    }
}
