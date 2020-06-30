using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ExpressBase.Mobile.Views.Shared
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MyLocations : ContentPage
    {
        public ObservableCollection<EbLocation> Locations { get; private set; }

        private EbLocation selectedLocation;

        public MyLocations()
        {
            InitializeComponent();

            this.SetLocations();
            BindingContext = this;
        }

        public void SetLocations()
        {
            try
            {
                Locations = new ObservableCollection<EbLocation>(Utils.Locations);

                foreach (EbLocation loc in Locations)
                {
                    if (loc.LocId == App.Settings.CurrentLocId)
                    {
                        selectedLocation = loc;
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
            if (e.Item == null) return;

            if (sender is ListView lv) lv.SelectedItem = null;

            EbLocation loc = (EbLocation)e.Item;

            if (loc != selectedLocation)
            {
                selectedLocation?.UnSelect();
                selectedLocation = loc;
                loc.Select();
            }
        }

        private async void BackButton_Clicked(object sender, EventArgs e)
        {
            await Application.Current.MainPage.Navigation.PopModalAsync(true);
        }

        private async void ApplyButton_Clicked(object sender, EventArgs e)
        {
            if (selectedLocation != null)
            {
                await Store.SetJSONAsync(AppConst.CURRENT_LOCOBJ, selectedLocation);
                App.Settings.CurrentLocation = selectedLocation;

                await (Application.Current.MainPage as MasterDetailPage).Detail.Navigation.PopAsync(true);

                try
                {
                    IReadOnlyList<Page> stack = (Application.Current.MainPage as MasterDetailPage).Detail.Navigation.NavigationStack;

                    if (stack.Any() && stack[0] is Home)
                    {
                        (stack[0] as Home).Refresh();
                    }
                }
                catch (Exception)
                {
                    EbLog.Write($"Location changed to {selectedLocation.LongName}, failed to update menu");
                }
            }
        }
    }
}