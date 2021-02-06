using ExpressBase.Mobile.Configuration;
using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Services;
using ExpressBase.Mobile.Views.Base;
using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ExpressBase.Mobile.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class WelcomeBoard : ContentPage, IDynamicContent
    {
        readonly AppVendor Vendor = App.Settings.Vendor;

        public Dictionary<string, string> PageContent => App.Settings.Vendor.Content.WelcomeBoard;

        public WelcomeBoard()
        {
            InitializeComponent();
            VendorLogo.Source = Vendor.Logo;
        }

        private async void StartApplicationClicked(object sender, EventArgs e)
        {
            if (!Utils.HasInternet)
            {
                Utils.Alert_NoInternet();
                return;
            }

            if (Vendor.BuildType == Enums.AppBuildType.Embedded)
            {
                EbLayout.ShowLoader();
                try
                {
                    ISolutionService service = new SolutionService();

                    ValidateSidResponse response = await service.ValidateSid(Vendor.SolutionURL);

                    if (response != null && response.IsValid)
                    {
                        await service.CreateEmbeddedSolution(response, Vendor.SolutionURL);
                        await Store.SetValueAsync(AppConst.FIRST_RUN, "true");
                        await App.Navigation.InitializeNavigation();
                    }
                }
                catch (Exception)
                {
                    Utils.Toast("Unable to finish the request");
                }
                EbLayout.HideLoader();
            }
            else
            {
                await Store.SetValueAsync(AppConst.FIRST_RUN, "true");
                await App.Navigation.InitializeNavigation();
            }
        }

        public void OnDynamicContentRendering()
        {
            LogoText.Text = PageContent["LogoText"];
            TitleText.Text = PageContent["Title"];
            Description.Text = PageContent["Description"];
            NextButon.Text = PageContent["ButtonText"];
        }
    }
}