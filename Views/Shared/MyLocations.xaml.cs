using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ExpressBase.Mobile.Views.Shared
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MyLocations : ContentPage
    {
        public ObservableCollection<EbLocation> Locations { get; private set; }

        public List<EbLocation> All { set; get; }

        public MyApplications MyApplicationsPage { set; get; }

        public EbLocation SelectedLocation { set; get; }

        public MyLocations()
        {
            InitializeComponent();

            this.SetLocations();
            BindingContext = this;
        }

        public MyLocations(MyApplications appPage)
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, true);
            BackButton.IsVisible = true;

            MyApplicationsPage = appPage;
            this.SetLocations();
            BindingContext = this;
        }

        public void SetLocations()
        {
            try
            {
                All = Utils.Locations;
                Locations = new ObservableCollection<EbLocation>(All);

                foreach (EbLocation loc in Locations)
                {
                    if (loc.LocId == App.Settings.CurrentLocId)
                    {
                        SelectedLocation = loc;
                        loc.Selected = true;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                EbLog.Write(ex.Message);
            }
        }

        private void ListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            EbLocation loc = (e.Item as EbLocation);

            if (loc != SelectedLocation)
            {
                this.Locations.Clear();
                foreach (EbLocation location in All)
                {
                    if (location.LocId == loc.LocId)
                    {
                        location.Selected = true;
                        SelectedLocation = loc;
                    }
                    else location.Selected = false;
                    this.Locations.Add(location);
                }
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

        private async void ApplyButton_Clicked(object sender, EventArgs e)
        {
            if (SelectedLocation != null)
            {
                await Store.SetJSONAsync(AppConst.CURRENT_LOCOBJ, SelectedLocation);
                App.Settings.CurrentLocation = SelectedLocation;

                if (MyApplicationsPage == null)
                    await (Application.Current.MainPage as MasterDetailPage).Detail.Navigation.PopAsync(true);
                else
                    await Application.Current.MainPage.Navigation.PopModalAsync();
            }
        }
    }
}