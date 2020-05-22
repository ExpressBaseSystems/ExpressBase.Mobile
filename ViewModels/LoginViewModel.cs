using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Services;
using ExpressBase.Mobile.ViewModels.BaseModels;
using ExpressBase.Mobile.Views;
using System;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ExpressBase.Mobile.ViewModels
{
    public class LoginViewModel : StaticBaseViewModel
    {
        private string email;

        public string Email
        {
            get { return this.email; }
            set
            {
                this.email = value;
                this.NotifyPropertyChanged();
            }
        }

        private string password;

        public string PassWord
        {
            get { return this.password; }
            set
            {
                this.password = value;
                this.NotifyPropertyChanged();
            }
        }

        private ImageSource logourl;

        public ImageSource LogoUrl
        {
            get { return logourl; }
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
            identityService = new IdentityService();
        }

        public override async Task InitializeAsync()
        {
            try
            {
                this.Email = Settings.UserName;
                LogoUrl = await identityService.GetLogo(Settings.SolutionId);
            }
            catch (Exception ex)
            {
                Log.Write(ex.Message);
            }
        }

        private async Task LoginAction()
        {
            try
            {
                IToast toast = DependencyService.Get<IToast>();

                if (!Settings.HasInternet)
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
                        IsBusy = false;
                        await Application.Current.MainPage.Navigation.PushAsync(new MyApplications());
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
                Log.Write("login clicked : " + ex.Message);
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
