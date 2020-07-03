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

        public Command LoginCommand => new Command(async () => await LoginAction());

        public LoginViewModel()
        {
            identityService = IdentityService.Instance;
        }

        public override async Task InitializeAsync()
        {
            this.Email = App.Settings.CurrentSolution?.LastUser;

            LogoUrl = await identityService.GetLogo(App.Settings.Sid);
        }

        private async Task LoginAction()
        {
            try
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
                    ApiAuthResponse response = await identityService.AuthenticateAsync(_username, _password);
                    if (response.IsValid)
                    {
                        await identityService.UpdateAuthInfo(response, _username, password);
                        await identityService.UpdateLastUser(_username);

                        EbMobileSolutionData data = await App.Settings.GetSolutionDataAsync(true);

                        ///update notification hub regid  in background
                        await NotificationService.Instance.UpdateNHRegisratation();

                        await Navigate(data);
                        IsBusy = false;
                    }
                    else
                    {
                        IsBusy = false;
                        toast.Show("wrong username or password.");
                    }
                }
                else
                    toast.Show("Email/Password cannot be empty");
            }
            catch (Exception ex)
            {
                EbLog.Write("login clicked : " + ex.Message);
            }
        }

        private async Task Navigate(EbMobileSolutionData data)
        {
            if (data != null && data.Applications != null)
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
            if ((string.IsNullOrEmpty(this.Email) || string.IsNullOrEmpty(this.PassWord)))
                return false;
            return true;
        }
    }
}
