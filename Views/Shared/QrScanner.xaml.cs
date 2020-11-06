using ExpressBase.Mobile.Helpers;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using ZXing;

namespace ExpressBase.Mobile.Views.Shared
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class QrScanner : ContentPage
    {
        private Action<string> viewAction;

        public QrScanner()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            ScannerView.IsAnalyzing = true;
            ScannerView.IsScanning = true;
        }

        public void ScannerView_OnScanResult(Result result)
        {
            if (result == null)
                return;
            try
            {
                bool hasPayLoad = !string.IsNullOrEmpty(result.Text);

                if (hasPayLoad)
                {
                    ScannerView.IsAnalyzing = false;
                    ScannerView.IsScanning = false;

                    viewAction?.Invoke(result.Text);

                    App.Navigation.PopModalByRenderer(true);
                }
                else
                {
                    ScannerView.IsAnalyzing = true;
                    ScannerView.IsScanning = true;
                }
            }
            catch (Exception ex)
            {
                EbLog.Error("Invalid qr code");
                EbLog.Error(ex.Message);

                ScannerView.IsAnalyzing = true;
                ScannerView.IsScanning = true;
            }
        }

        public void BindMethod(Action<string> action)
        {
            viewAction = action;
        }

        private async void BackButton_Clicked(object sender, EventArgs e)
        {
           await App.Navigation.PopModalByRenderer(true);
        }
    }
}