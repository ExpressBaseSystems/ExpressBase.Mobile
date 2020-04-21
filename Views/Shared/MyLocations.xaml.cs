using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using System;
using System.Collections.ObjectModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ExpressBase.Mobile.Views.Shared
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MyLocations : ContentPage
    {
        public ObservableCollection<EbLocation> Locations { get; private set; }

        public MyApplications MyApplicationsPage { set; get; }

        public MyLocations()
        {
            InitializeComponent();

            SetLocations();
            BindingContext = this;
        }

        public MyLocations(MyApplications appPage)
        {
            InitializeComponent();

            NavigationPage.SetHasNavigationBar(this, true);
            MyApplicationsPage = appPage;
            SetLocations();
            BackButton.IsVisible = true;
            BindingContext = this;
        }

        public void SetLocations()
        {
            try
            {
                Locations = new ObservableCollection<EbLocation>(Settings.Locations);

                int current = Convert.ToInt32(Store.GetValue(AppConst.CURRENT_LOCATION));

                foreach (EbLocation loc in Locations)
                {
                    if (loc.LocId == current)
                    {
                        loc.Selected = true;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex.Message);
            }
        }

        private async void ListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            EbLocation loc = (e.Item as EbLocation);
            if (loc.LocId != Settings.LocationId)
            {
                loc.Selected = true;
                Store.SetValue(AppConst.CURRENT_LOCATION, loc.LocId.ToString());

                if (MyApplicationsPage == null)
                    await (Application.Current.MainPage as MasterDetailPage).Detail.Navigation.PopAsync(true);
                else
                    await Application.Current.MainPage.Navigation.PopModalAsync();
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            if (MyApplicationsPage != null) MyApplicationsPage.LocationPagePoped();
        }

        private void BackButton_Clicked(object sender, EventArgs e)
        {
            Application.Current.MainPage.Navigation.PopModalAsync(true);
        }
    }
}