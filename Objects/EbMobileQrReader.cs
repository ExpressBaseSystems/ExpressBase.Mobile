using ExpressBase.Mobile.CustomControls;
using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Structures;
using ExpressBase.Mobile.Views.Shared;
using System;
using Xamarin.Forms;

namespace ExpressBase.Mobile
{
    public class EbMobileQrReader : EbMobileControl
    {
        public override EbDbTypes EbDbType { get { return EbDbTypes.String; } set { } }

        private TextBox dataHolder;

        public override void InitXControl(FormMode mode, NetworkMode network)
        {
            base.InitXControl(mode, network);

            dataHolder = new TextBox
            {
                BorderColor = Color.Transparent,
                IsReadOnly = true
            };
            Button link = new Button
            {
                Style = (Style)HelperFunctions.GetResourceValue("QRReaderButton")
            };
            link.Clicked += OpenQrScanner;

            this.XControl = new InputGroup(dataHolder, link) { BgColor = XBackground };
        }

        public override object GetValue()
        {
            return dataHolder.Text;
        }

        public override void SetValue(object value)
        {
            dataHolder.Text = value?.ToString();
        }

        public override void Reset()
        {
            dataHolder.ClearValue(TextBox.TextProperty);
        }

        public override bool Validate()
        {
            string value = GetValue() as string;

            if (Required && string.IsNullOrEmpty(value))
                return false;

            return true;
        }

        private async void OpenQrScanner(object sender, EventArgs e)
        {
            bool hasCameraAccess = await AppPermission.Camera();

            if (hasCameraAccess)
            {
                QrScanner scannerPage = new QrScanner();
                scannerPage.BindMethod(OnScannedResult);

                await App.RootMaster.Detail.Navigation.PushModalAsync(scannerPage);
            }
            else
                Utils.Toast("Allow permission to access camera");
        }

        public void OnScannedResult(string payload)
        {
            Device.BeginInvokeOnMainThread(() => dataHolder.Text = payload);
        }
    }
}
