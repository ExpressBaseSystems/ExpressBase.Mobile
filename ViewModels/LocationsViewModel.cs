using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Services;
using ExpressBase.Mobile.ViewModels.BaseModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ExpressBase.Mobile.ViewModels
{
    public class LocationsViewModel : StaticBaseViewModel
    {
        private List<EbLocation> locations;

        public List<EbLocation> Locations
        {
            get => locations;
            set
            {
                locations = value;
                NotifyPropertyChanged();
            }
        }

        public EbLocation SelectedLocation { set; get; }

        public Command ApplyLocationCommand => new Command(async () => await LocationSubmited());

        public override async Task InitializeAsync()
        {
            await Task.Delay(1);

            Locations = Utils.Locations;

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

        private async Task LocationSubmited()
        {
            if (SelectedLocation != null)
            {
                await Store.SetJSONAsync(AppConst.CURRENT_LOCOBJ, SelectedLocation);
                App.Settings.CurrentLocation = SelectedLocation;

                App.Navigation.UpdateViewStack();

                await App.Navigation.PopMasterAsync(true);
            }
        }

        public void UpdateToInitial()
        {
            Locations = Utils.Locations;
        }

        public async Task FilterBySearchValue(string search)
        {
            await Task.Delay(1);

            List<EbLocation> all = Utils.Locations;
            search = search.ToLower();

            try
            {
                List<EbLocation> filterd = all.Where(x => x.LongName.ToLower().Contains(search) || x.ShortName.ToLower().Contains(search)).ToList();
                Locations = filterd;
            }
            catch (Exception Ex)
            {
                EbLog.Error("Locations search got an error");
                EbLog.Error(Ex.Message);
            }
        }
    }
}
