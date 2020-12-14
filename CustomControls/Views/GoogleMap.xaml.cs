using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Services.GoogleMap;
using ExpressBase.Mobile.Views.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using Xamarin.Forms.Xaml;

namespace ExpressBase.Mobile.CustomControls.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class GoogleMap : ContentView
    {
        public static readonly BindableProperty SearchEnabledProperty =
            BindableProperty.Create(nameof(SearchEnabled), typeof(bool), typeof(GoogleMap));

        public static readonly BindableProperty EnableTrafficProperty =
            BindableProperty.Create(nameof(EnableTraffic), typeof(bool), typeof(GoogleMap));

        public static readonly BindableProperty ChangeLocationEnabledProperty =
            BindableProperty.Create(nameof(ChangeLocationEnabled), typeof(bool), typeof(GoogleMap));

        public static readonly BindableProperty LocationPickerEnabledProperty =
            BindableProperty.Create(nameof(LocationPickerEnabled), typeof(bool), typeof(GoogleMap));

        public static readonly BindableProperty GetResultCommandProperty =
            BindableProperty.Create(nameof(GetResultCommand), typeof(Command<EbGeoLocation>), typeof(GoogleMap));

        public static readonly BindableProperty ZoomEnabledProperty =
            BindableProperty.Create(nameof(ZoomEnabled), typeof(bool), typeof(GoogleMap));

        public static readonly BindableProperty ZoomLevelProperty =
            BindableProperty.Create(nameof(ZoomLevel), typeof(int), typeof(GoogleMap), defaultValue: 14);

        public event EbEventHandler ChangeLocationClicked;

        public bool SearchEnabled
        {
            get { return (bool)GetValue(SearchEnabledProperty); }
            set { SetValue(SearchEnabledProperty, value); }
        }

        public bool EnableTraffic
        {
            get { return (bool)GetValue(EnableTrafficProperty); }
            set { SetValue(EnableTrafficProperty, value); }
        }

        public bool ChangeLocationEnabled
        {
            get { return (bool)GetValue(ChangeLocationEnabledProperty); }
            set { SetValue(ChangeLocationEnabledProperty, value); }
        }

        public bool LocationPickerEnabled
        {
            get { return (bool)GetValue(LocationPickerEnabledProperty); }
            set { SetValue(LocationPickerEnabledProperty, value); }
        }

        public Command<EbGeoLocation> GetResultCommand
        {
            get { return (Command<EbGeoLocation>)GetValue(GetResultCommandProperty); }
            set { SetValue(GetResultCommandProperty, value); }
        }

        public bool ZoomEnabled
        {
            get { return (bool)GetValue(ZoomEnabledProperty); }
            set { SetValue(ZoomEnabledProperty, value); }
        }

        public int ZoomLevel
        {
            get { return (int)GetValue(ZoomLevelProperty); }
            set { SetValue(ZoomLevelProperty, value); }
        }

        private EbGeoLocation selectedLocation;

        readonly GoogleMapApiService service;

        public GoogleMap()
        {
            InitializeComponent();
            service = new GoogleMapApiService();

            OpenSettings();
        }

        private void OpenSettings()
        {
            try
            {
                ILocationHelper locService = DependencyService.Get<ILocationHelper>();
                locService.OpenSettings();
            }
            catch (Exception ex)
            {
                EbLog.Error("failed to open location settings in [GoogleMap] view, " + ex.Message);
            }
        }

        protected override void OnPropertyChanged(string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);

            if (propertyName == EnableTrafficProperty.PropertyName)
            {
                MapView.TrafficEnabled = EnableTraffic;
            }
            else if (propertyName == SearchEnabledProperty.PropertyName)
            {
                SearchBox.IsVisible = SearchEnabled;
            }
            else if (propertyName == ChangeLocationEnabledProperty.PropertyName)
            {
                LocationChangeButton.IsVisible = ChangeLocationEnabled;
            }
            else if (propertyName == LocationPickerEnabledProperty.PropertyName)
            {
                LocationPickerContainer.IsVisible = LocationPickerEnabled;
            }
            else if (propertyName == ZoomEnabledProperty.PropertyName)
            {
                MapView.HasZoomEnabled = ZoomEnabled;
            }
        }

        private void SetSingleLocation(double lat, double lng)
        {
            Position pos = new Position(lat, lng);

            double latlongDegrees = 360 / (Math.Pow(2, ZoomLevel));
            MapSpan span = new MapSpan(pos, latlongDegrees, latlongDegrees);
            MapView.MoveToRegion(span);

            Pin pin = new Pin
            {
                Label = "you are here",
                Type = PinType.Place,
                Position = pos
            };

            if (MapView.Pins.Count == 0)
                MapView.Pins.Add(pin);
            else
                MapView.Pins[0] = pin;
        }

        public void SetLocation(double lat, double lng)
        {
            this.SetSingleLocation(lat, lng);
            selectedLocation = new EbGeoLocation(lat, lng);

            this.SetAddress(lat, lng);
        }

        public async void SetAddress(double lat, double lng)
        {
            try
            {
                Geocoder geoCoder = new Geocoder();
                Position pos = new Position(lat, lng);

                IEnumerable<string> possibleAddresses = await geoCoder.GetAddressesForPositionAsync(pos);
                string address = possibleAddresses.FirstOrDefault();
                PickerTextLable.Text = address;
            }
            catch (Exception ex)
            {
                EbLog.Info("Error at [SetAddress] in googlemap view [GeoCoder]");
                EbLog.Info(ex.Message);
            }
        }

        private async void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string placeText = (sender as EbXTextBox).Text ?? string.Empty;

            if (placeText.Length < 3)
            {
                PlacesList.ItemsSource = null;
                return;
            }

            GooglePlaceAutoCompleteResults response = await service.GetPlaces(placeText);

            if (response != null && response.Predictions != null)
            {
                PlacesList.ItemsSource = response.Predictions;
            }
        }

        private void LocationChangeButton_Clicked(object sender, EventArgs e)
        {
            ChangeLocationClicked?.Invoke(sender, e);
        }

        private void SearchBox_Focused(object sender, FocusEventArgs e)
        {
            SearchFade.IsVisible = true;
            SearchFade.FadeTo(1);
            PlacesList.IsVisible = true;
            SearchBarIcon.IsVisible = false;
            SearchBarBackButton.IsVisible = true;
            SearchBox.BackgroundColor = Color.FromHex("#f2f2f2");
        }

        private void SearchBarBackButton_Clicked(object sender, EventArgs e)
        {
            this.HideSearch();
        }

        private void HideSearch()
        {
            PlacesList.IsVisible = false;
            SearchBox.BackgroundColor = Color.White;
            SearchFade.FadeTo(0);
            SearchFade.IsVisible = false;
            SearchBarBackButton.IsVisible = false;
            SearchBarIcon.IsVisible = true;
        }

        private async void PlacesList_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (e.Item == null) return;
            if (sender is ListView lv)
                lv.SelectedItem = null;

            GooglePlaceInfo place = (GooglePlaceInfo)e.Item;

            if (place != null)
            {
                try
                {
                    PickerTextLable.Text = place.Description;

                    GooglePlace placeDetails = await service.GetPlaceDetails(place.PlaceId);

                    EbGeoLocation cords = placeDetails?.GetCordinates();

                    if (cords != null)
                    {
                        cords.Address = place.Description;
                        selectedLocation = cords;
                        this.SetSingleLocation(cords.Latitude, cords.Longitude);
                    }
                }
                catch (Exception ex)
                {
                    EbLog.Info("Failed to fetch place details");
                    EbLog.Error(ex.Message);
                }
            }
            this.HideSearch();
        }

        private void SaveLocationButton_Clicked(object sender, EventArgs e)
        {
            GetResultCommand?.Execute(selectedLocation);
        }

        private void OnMapAreaTouched(object sender, MapClickedEventArgs e)
        {
            if (LocationPickerEnabled && e.Position != null)
            {
                SetLocation(e.Position.Latitude, e.Position.Longitude);
            }
        }
    }
}