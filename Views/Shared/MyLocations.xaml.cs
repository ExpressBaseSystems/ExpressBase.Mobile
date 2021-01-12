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

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (!isRendered)
            {
                viewModel.Initialize();
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

        private void LocSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string search = LocSearchBox.Text;

            if (search != null)
            {
                if (search.Length >= 3)
                {
                    EbLayout.ShowLoader();
                    viewModel.FilterBySearchValue(search);
                    EbLayout.HideLoader();
                    EmptyLabel.IsVisible = viewModel.Locations.Count <= 0;
                }
            }
        }

        private bool OnBackButtonPressed(object sender, EventArgs e)
        {
            if (LocSearchBox.IsVisible)
            {
                LocSearchBox.IsVisible = false;
                SearchButton.IsVisible = true;
                viewModel.UpdateToInitial();
                return false;
            }
            return true;
        }
    }
}