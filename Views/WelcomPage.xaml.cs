using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.CustomControls;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Services;
using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ExpressBase.Mobile.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class WelcomPage : ContentPage, IDynamicContent
    {
        public ImageSource SolutionLogo { set; get; }

        public string UserName { set; get; }

        public Dictionary<string, string> PageContent => App.Settings.Vendor.Content.WelcomePage;

        private readonly EbMobileSolutionData solData;

        public WelcomPage(EbMobileSolutionData data)
        {
            InitializeComponent();

            solData = data;

            UserName = App.Settings.UserDisplayName;
            SolutionLogo = CommonServices.GetLogo(App.Settings.Sid);
            BindingContext = this;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            SetContentFromConfig();
            SolutionImage.ScaleTo(1, 300);
        }

        public void SetContentFromConfig()
        {
            Description.Text = PageContent["Description"];
        }

        private async void Button_Clicked(object sender, EventArgs e)
        {
            this.Opacity = 0.3;

            await Store.SetValueAsync(AppConst.IS_FRESH_START, "false");

            if (solData.Applications.Count == 1)
            {
                AppData appdata = solData.Applications[0];

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
}