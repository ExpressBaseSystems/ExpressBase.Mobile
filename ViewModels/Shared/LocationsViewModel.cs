using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.ViewModels.BaseModels;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace ExpressBase.Mobile.ViewModels.Shared
{
    public class LocationsViewModel : StaticBaseViewModel
    {
        public IList<EbLocation> Locations { get; private set; }

        public Command LocationSwitchCommand { set; get; }

        private EbLocation _selectedItem;
        public EbLocation SelectedItem
        {
            get
            {
                return _selectedItem;
            }
            set
            {
                _selectedItem = value;

                if (_selectedItem == null)
                    return;

                LocationSwitchCommand.Execute(_selectedItem);

                SelectedItem = null;
            }
        }

        public LocationsViewModel()
        {
            PageTitle = "Locations";

            LocationSwitchCommand = new Command(LocationSwitchClicked);

            Locations = Settings.Locations;

            int _current = Convert.ToInt32(Store.GetValue(AppConst.CURRENT_LOCATION));

            foreach(EbLocation _loc in Locations)
            {
                if(_loc.LocId == _current)
                {
                    _loc.Selected = true;
                    break;
                }
            }
        }

        private void LocationSwitchClicked(object sender)
        {
            EbLocation _loc = (sender as EbLocation);
            if(_loc.LocId != Settings.LocationId)
            {
                _loc.Selected = true;
                Store.SetValue(AppConst.CURRENT_LOCATION,_loc.LocId.ToString());
                (Application.Current.MainPage as MasterDetailPage).Detail.Navigation.PopAsync(true);
            }
        }
    }
}
