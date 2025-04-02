using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.ViewModels.BaseModels;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ExpressBase.Mobile.ViewModels
{
    public class BluetoothDevicesViewModel : StaticBaseViewModel
    {
        private List<EbBTDevice> bt_devices;

        public List<EbBTDevice> BtDevices
        {
            get => bt_devices;
            set
            {
                bt_devices = value;
                NotifyPropertyChanged();
            }
        }

        public EbBTDevice SelectedBtDevice { set; get; }

        public Command SelectBtDeviceCommand => new Command(async () => await BtDeviceSelected());

        public override void Initialize()
        {

        }

        public async void InitBtDevicesList()
        {
            IEbBluetoothHelper BtHelper = DependencyService.Get<IEbBluetoothHelper>();
            if (!BtHelper.RequestBluetoothPermissions())
                Utils.Toast("Bluetooth permission required");
            if (!BtHelper.EnableAndCheckBluetoothAdapter())
                Utils.Toast("Can't read bluetooth devices");

            BtDevices = await BtHelper.GetBluetoothDeviceList();
            App.Settings.SelectedBtDevice = Store.GetJSON<EbBTDevice>(AppConst.CURRENT_BT_PRINTER);
            foreach (var device in BtDevices)
            {
                if (App.Settings.SelectedBtDevice != null && device.Address == App.Settings.SelectedBtDevice.Address)
                {
                    SelectedBtDevice = device;
                    SelectedBtDevice.Selected = true;
                    break;
                }
            }
        }

        private async Task BtDeviceSelected()
        {
            if (SelectedBtDevice != null)
            {
                await Store.SetJSONAsync(AppConst.CURRENT_BT_PRINTER, SelectedBtDevice);
                App.Settings.SelectedBtDevice = SelectedBtDevice;

                App.Navigation.UpdateViewStack();

                await App.Navigation.PopMasterAsync(true);
            }
        }

    }
}
