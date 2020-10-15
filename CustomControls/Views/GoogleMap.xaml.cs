using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Services.GoogleMap;
using ExpressBase.Mobile.Views.Base;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using Xamarin.Forms.Xaml;

namespace ExpressBase.Mobile.CustomControls.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class GoogleMap : ContentView
    {
        public static readonly BindableProperty SearchEnabledProperty =
            BindableProperty.Create(nameof(SearchEnabled), typeof(bool), typeof(GoogleMap), defaultValue: true);

        public static readonly BindableProperty EnableTrafficProperty =
            BindableProperty.Create(nameof(EnableTraffic), typeof(bool), typeof(GoogleMap), defaultValue: false);

        public static readonly BindableProperty ChangeLocationEnabledProperty =
            BindableProperty.Create(nameof(ChangeLocationEnabled), typeof(bool), typeof(GoogleMap), defaultValue: false);

        public static readonly BindableProperty LocationPickerEnabledProperty =
            BindableProperty.Create(nameof(LocationPickerEnabled), typeof(bool), typeof(GoogleMap), defaultValue: false);

        public static readonly BindableProperty GetResultCommandProperty =
            BindableProperty.Create(nameof(GetResultCommand), typeof(Command<GoogleLocation>), typeof(GoogleMap));

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

        public Command<GoogleLocation> GetResultCommand
        {
            get { return (Command<GoogleLocation>)GetValue(GetResultCommandProperty); }
            set { SetValue(GetResultCommandProperty, value); }
        }

        readonly double zoomLevel = 15;

        private GoogleLocation selectedLocation;

        readonly GoogleMapApiService service;

        public GoogleMap()
        {
            InitializeComponent();
            BindingContext = this;

            service = new GoogleMapApiService();
        }

        protected override void OnPropertyChanged(string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);
        }

        private void SetLocationInternal(double lat, double lng)
        {
            Position pos = new Position(lat, lng);
            MapSpan span = MapSpan.FromCenterAndRadius(pos, Distance.FromMiles(10));
            MapView.MoveToRegion(span);
            MapView.Pins.Add(new Pin
            {
                Label = "you are here",
                Type = PinType.Place,
                Position = pos
            });

            SetzoomLevel(zoomLevel);
        }

        public void SetLocation(double lat, double lng)
        {
            SetLocationInternal(lat, lng);
            selectedLocation = new GoogleLocation(lat, lng);
        }

        private void SetzoomLevel(double zoom)
        {
            double latlongDegrees = 360 / (Math.Pow(2, zoom));
            if (MapView.VisibleRegion != null)
            {
                MapView.MoveToRegion(new MapSpan(MapView.VisibleRegion.Center, latlongDegrees, latlongDegrees));
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
        }

        private void SearchBarBackButton_Clicked(object sender, EventArgs e)
        {
            this.HideSearch();
        }

        void HideSearch()
        {
            PlacesList.IsVisible = false;
            SearchFade.FadeTo(0);
            SearchFade.IsVisible = false;
            SearchBarBackButton.IsVisible = false;
            SearchBarIcon.IsVisible = true;
        }

        private async void PlacesList_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (e.Item == null) return;

            if (sender is ListView lv) lv.SelectedItem = null;

            GooglePlaceInfo place = (GooglePlaceInfo)e.Item;

            if (place != null)
            {
                try
                {
                    PickerTextLable.Text = place.Description;

                    GooglePlace placeDetails = await service.GetPlaceDetails(place.PlaceId);

                    GoogleLocation cords = placeDetails?.GetCordinates();

                    if (cords != null)
                    {
                        cords.Address = place.Description;
                        selectedLocation = cords;
                        SetLocationInternal(cords.Latitude, cords.Longitude);
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
    }
}