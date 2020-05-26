using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ExpressBase.Mobile.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class About : ContentPage
    {
        public About()
        {
            InitializeComponent();

            INativeHelper helper = DependencyService.Get<INativeHelper>();
            DeviceId.Text = $"DEVICE ID : {helper.DeviceId}";
            AppVersion.Text = $"Version {helper.AppVersion}";
        }
    }
}