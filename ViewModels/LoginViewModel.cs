using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Services;
using ExpressBase.Mobile.Views;
using System;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace ExpressBase.Mobile.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {

        private string email;
        public string Email
        {
            get
            {
                return this.email;
            }
            set
            {
                if (this.email == value)
                {
                    return;
                }
                this.email = value;
                this.NotifyPropertyChanged();
            }
        }

        private string password;
        public string PassWord
        {
            get
            {
                return this.password;
            }
            set
            {
                if (this.password == value)
                {
                    return;
                }
                this.password = value;
                this.NotifyPropertyChanged();
            }
        }

        private ImageSource logourl;
        public ImageSource LogoUrl
        {
            get
            {
                return logourl;
            }
            set
            {
                logourl = value;
                this.NotifyPropertyChanged();
            }
        }

        public LoginViewModel()
        {
            this.LoginCommand = new Command(LoginAction);
            this.ResetConfig = new Command(ResetClicked);//bind reset button
            SetLogo();
        }

        public Command LoginCommand { set; get; }

        private void LoginAction(object obj)
        {
            if (Connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                DependencyService.Get<IToast>().Show("Not connected to internet!");
                return;
            }

            string _username = this.Email.Trim();
            string _password = this.PassWord.Trim();
            Task.Run(() =>
            {
                if (CanLogin())
                {
                    Device.BeginInvokeOnMainThread(() => IsBusy = true);

                    ApiAuthResponse response = Auth.TryAuthenticate(_username, _password);
                    if (response.IsValid)
                    {
                        Auth.UpdateStore(response, _username, password);
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            IsBusy = false;
                            Application.Current.MainPage.Navigation.PushAsync(new AppSelect());
                        });
                    }
                    else
                    {
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            IsBusy = false;
                            Application.Current.MainPage.DisplayAlert("Alert!", "User does not exist", "Ok");
                        });
                    }
                }
                else
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        IsBusy = false;
                        Application.Current.MainPage.DisplayAlert("Alert!", "Email/Password cannot be empty", "Ok");
                    });
                }
            });
        }

        private bool CanLogin()
        {
            if (string.IsNullOrWhiteSpace(this.Email) || string.IsNullOrWhiteSpace(this.PassWord))
                return false;

            return true;
        }

        private void SetLogo()
        {
            INativeHelper helper = DependencyService.Get<INativeHelper>();
            string sid = Settings.SolutionId;
            try
            {
                var bytes = helper.GetPhoto($"ExpressBase/{sid}/logo.png");

                if(bytes== null)
                {
                    LogoUrl = ImageSource.FromResource("eblogo.png");
                }
                else
                {
                    LogoUrl = ImageSource.FromStream(() => new MemoryStream(bytes));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
