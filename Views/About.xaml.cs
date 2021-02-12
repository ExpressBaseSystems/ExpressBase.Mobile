using ExpressBase.Mobile.Configuration;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Views.Base;
using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ExpressBase.Mobile.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class About : ContentPage, IDynamicContent
    {
        public bool ShowPoweredBy => (EbBuildConfig.VendorName != Expressbase.VendorName);

        public About()
        {
            InitializeComponent();
            BindingContext = this;
        }

        public Dictionary<string, string> PageContent => App.Settings.Vendor.Content.About;

        public string VendorName => PageContent["VendorName"];

        public string VendorLogo => App.Settings.Vendor.Logo;

        public void OnDynamicContentRendering()
        {
            try
            {
                INativeHelper helper = DependencyService.Get<INativeHelper>();

                DeviceId.Text = $"DEVICE ID : {helper.DeviceId}";

                AppVersion.Text = $"Version {helper.AppVersion}";

                VendorDescription.Text = PageContent["Description"];

                if (PageContent.TryGetValue("Url", out string url) && Utils.HasInternet)
                {
                    StaticContent.IsVisible = false;
                    ExternalWebLink.Source = url;
                    ExternalWebLink.IsVisible = true;
                }
            }
            catch (Exception ex)
            {
                EbLog.Error(ex.Message + ex.StackTrace);
            }
        }

        private void WebViewNavigating(object sender, WebNavigatingEventArgs e)
        {
            CpLayout.ShowLoader();
        }

        private void WebViewNavigated(object sender, WebNavigatedEventArgs e)
        {
            CpLayout.HideLoader();
        }
    }
}