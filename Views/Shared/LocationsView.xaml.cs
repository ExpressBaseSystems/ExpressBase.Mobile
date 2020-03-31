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
    public partial class LocationsView : ContentPage
    {
        public ObservableCollection<EbLocation> Locations { get; private set; }

        public LocationsView()
        {
            InitializeComponent();

            Locations = new ObservableCollection<EbLocation>(Settings.Locations);

            int _current = Convert.ToInt32(Store.GetValue(AppConst.CURRENT_LOCATION));

            foreach (EbLocation _loc in Locations)
            {
                if (_loc.LocId == _current)
                {
                    _loc.Selected = true;
                    break;
                }
            }
            BindingContext = this;
        }

        private async void ListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            EbLocation _loc = (e.Item as EbLocation);
            if (_loc.LocId != Settings.LocationId)
            {
                _loc.Selected = true;
                Store.SetValue(AppConst.CURRENT_LOCATION, _loc.LocId.ToString());
                await (Application.Current.MainPage as MasterDetailPage).Detail.Navigation.PopAsync(true);
            }
        }
    }
}