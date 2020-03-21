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

        public Command LoginCommand { set; get; }

        public LoginViewModel()
        {
            this.Email = Settings.UserName; // fill email on redirect
            this.LoginCommand = new Command(async () => await LoginAction());
            SetLogo();
        }

        private async Task LoginAction()
        {
            IToast toast = DependencyService.Get<IToast>();

            if (!Settings.HasInternet)
            {
                toast.Show("Not connected to internet!");
                return;
            }

            string _username = this.Email.Trim();
            string _password = this.PassWord.Trim();
            if (CanLogin())
            {
                IsBusy = true;
                ApiAuthResponse response = await Auth.TryAuthenticateAsync(_username, _password);
                if (response.IsValid)
                {
                    Auth.UpdateStore(response, _username, password);
                    IsBusy = false;
                    await Application.Current.MainPage.Navigation.PushAsync(new AppSelect());
                }
                else
                {
                    IsBusy = false;
                    toast.Show("User does not exist");
                }
            }
            else
                toast.Show("Email/Password cannot be empty");
        }

        bool CanLogin()
        {
            if ((string.IsNullOrEmpty(this.Email) || string.IsNullOrEmpty(this.PassWord)))
                return false;
            return true;
        }

        void SetLogo()
        {
            INativeHelper helper = DependencyService.Get<INativeHelper>();
            string sid = Settings.SolutionId;
            try
            {
                var bytes = helper.GetPhoto($"ExpressBase/{sid}/logo.png");

                if (bytes == null)
                    LogoUrl = ImageSource.FromFile("logo.png");
                else
                    LogoUrl = ImageSource.FromStream(() => new MemoryStream(bytes));
            }
            catch (Exception ex)
            {
                Log.Write("Login_SetLogo" + ex.Message);
            }
        }
    }
}
