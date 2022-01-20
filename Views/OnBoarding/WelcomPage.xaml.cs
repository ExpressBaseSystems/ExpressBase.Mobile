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
            SolutionImage.ScaleTo(1, 300);
        }

        public void OnDynamicContentRendering()
        {
            string desc = PageContent["Description"];
            Description.Text = desc.Replace("@solutionname@", App.Settings.Sid);
        }

        private async void Button_Clicked(object sender, EventArgs e)
        {
            await Store.SetValueAsync(AppConst.IS_FRESH_START, "false");

            if (solData.Applications.Count == 1)
            {
                AppData appdata = solData.Applications[0];

                await Store.SetJSONAsync(AppConst.CURRENT_APP, appdata);
                App.Settings.CurrentApplication = appdata;
                App.Settings.MobilePages = appdata.MobilePages;
                App.Settings.WebObjects = appdata.WebObjects;

                App.RootMaster = new RootMaster();
                Application.Current.MainPage = App.RootMaster;
            }
            else
            {
                await App.Navigation.NavigateAsync(new MyApplications());
            }
        }
    }
}