using ExpressBase.Mobile.Configuration;
using ExpressBase.Mobile.CustomControls;
using ExpressBase.Mobile.Helpers;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ExpressBase.Mobile.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class About : ContentPage, IDynamicContent
    {
        public About()
        {
            InitializeComponent();
            BindingContext = this;
            SetContentFromConfig();
        }

        public Dictionary<string, string> PageContent => App.Settings.Vendor.Content.About;

        public string VendorName => PageContent["VendorName"];

        public string VendorLogo => App.Settings.Vendor.Logo;

        public void SetContentFromConfig()
        {
            INativeHelper helper = DependencyService.Get<INativeHelper>();
            DeviceId.Text = $"DEVICE ID : {helper.DeviceId}";
            AppVersion.Text = $"Version {helper.AppVersion}";

            if(EbBuildConfig.VendorName == Expressbase.VendorName)
            {
                VendorDescription.Text = @"EXPRESSbase is a Platform on the cloud to build and run business applications 10x faster. Get the best of both worlds – stability of Ready-Made software, and flexibility of Custom software.";
            }
            else if(EbBuildConfig.VendorName == MoveOn.VendorName)
            {
                VendorDescription.Text = string.Empty;
            }
            else
            {
                VendorDescription.Text = string.Empty;
            }
        }
    }
}