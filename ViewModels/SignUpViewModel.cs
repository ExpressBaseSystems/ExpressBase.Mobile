using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Services;
using ExpressBase.Mobile.ViewModels.BaseModels;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ExpressBase.Mobile.ViewModels
{
    public class SignUpViewModel : StaticBaseViewModel
    {
        private string email;
        private string password;
        private string mobile;

        public string Email
        {
            get => this.email;
            set { this.email = value; this.NotifyPropertyChanged(); }
        }

        public string PassWord
        {
            get => this.password;
            set { this.password = value; this.NotifyPropertyChanged(); }
        }

        public string Mobile
        {
            get => this.mobile;
            set { this.mobile = value; this.NotifyPropertyChanged(); }
        }

        public bool HasEmail => (signUpSettings != null && signUpSettings.Email);

        public bool HasMobile => (signUpSettings != null && signUpSettings.MobileNo);

        public bool HasPassword => (signUpSettings != null && signUpSettings.Password);

        public ImageSource LogoUrl { set; get; }

        private readonly MobileSignUpSettings signUpSettings;

        public Command GoToLoginCommand => new Command(async () => await GoToLogin());

        public SignUpViewModel()
        {
            this.LogoUrl = CommonServices.GetLogo(App.Settings.Sid);

            signUpSettings = App.Settings.CurrentSolution.GetSignUpSettings();
        }

        private async Task GoToLogin()
        {
            await NavigationService.LoginWithCS();
        }
    }
}
