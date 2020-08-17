using ExpressBase.Mobile.CustomControls;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.ViewModels;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ExpressBase.Mobile.Views.Shared
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MyLocations : ContentPage
    {
        public bool isRendered;

        private readonly LocationsViewModel viewModel;

        public MyLocations()
        {
            InitializeComponent();
            BindingContext = viewModel = new LocationsViewModel();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (!isRendered)
            {
                await viewModel.InitializeAsync();
                isRendered = true;
            }
        }

        private void ListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (e.Item == null) return;

            if (sender is ListView lv) lv.SelectedItem = null;

            EbLocation loc = (EbLocation)e.Item;

            if (loc != viewModel.SelectedLocation)
            {
                viewModel.SelectedLocation?.UnSelect();
                viewModel.SelectedLocation = loc;
                loc.Select();
            }
        }

        private void SearchButton_Clicked(object sender, EventArgs e)
        {
            SearchButton.IsVisible = false;
            LocSearchBox.IsVisible = true;
            LocSearchBox.Focus();
        }

        private async void LocSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string search = LocSearchBox.Text;

            if (search != null)
            {
                SearchClear.IsVisible = search.Length > 0;

                if (search.Length >= 3)
                {
                    SearchLoader.IsVisible = true;
                    await viewModel.FilterBySearchValue(search);
                    SearchLoader.IsVisible = false;
                    EmptyLabel.IsVisible = viewModel.Locations.Count <= 0;
                }
                else
                {
                    if (!viewModel.IsInitialState)
                    {
                        viewModel.UpdateToInitial();
                    }
                }
            }
        }

        private void SearchClear_Clicked(object sender, EventArgs e)
        {
            LocSearchBox.ClearValue(TextBox.TextProperty);
            SearchClear.IsVisible = false;
            viewModel.UpdateToInitial();
            EmptyLabel.IsVisible = false;
        }
    }
}