using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.ViewModels;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ExpressBase.Mobile.Views.Shared
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class BluetoothDevices : ContentPage
    {
        public bool isRendered;

        private readonly BluetoothDevicesViewModel viewModel;

        public BluetoothDevices()
        {
            InitializeComponent();
            BindingContext = viewModel = new BluetoothDevicesViewModel();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (!isRendered)
            {
                viewModel.Initialize();
                isRendered = true;
            }
            viewModel.InitBtDevicesList();

            EmptyLabel.IsVisible = viewModel.BtDevices?.Count <= 0;
        }

        private void ListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (e.Item == null) return;

            if (sender is ListView lv) lv.SelectedItem = null;

            EbBTDevice dev = (EbBTDevice)e.Item;

            if (dev != viewModel.SelectedBtDevice)
            {
                viewModel.SelectedBtDevice?.UnSelect();
                viewModel.SelectedBtDevice = dev;
                dev.Select();
            }
        }

        private bool OnBackButtonPressed(object sender, EventArgs e)
        {
            //viewModel.UpdateToInitial();
            //return false;

            return true;
        }
    }
}