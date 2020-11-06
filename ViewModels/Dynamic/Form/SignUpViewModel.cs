using ExpressBase.Mobile.Services;
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

        private async Task GoToLogin()
        {
            await App.Navigation.NavigateToLogin();
        }
    }
}
