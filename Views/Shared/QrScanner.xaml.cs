using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using Newtonsoft.Json;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using ZXing;

namespace ExpressBase.Mobile.Views.Shared
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class QrScanner : ContentPage
    {
        private Action<SolutionQrMeta> viewAction;

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
                SolutionQrMeta meta = JsonConvert.DeserializeObject<SolutionQrMeta>(result.Text);

                if (meta != null)
                {
                    ScannerView.IsAnalyzing = false;
                    ScannerView.IsScanning = false;

                    viewAction?.Invoke(meta);          

                    if (App.RootMaster != null)
                        App.RootMaster.Detail.Navigation.PopModalAsync(true);
                    else
                        Application.Current.MainPage.Navigation.PopModalAsync(true);
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

        public void BindMethod(Action<SolutionQrMeta> action)
        {
            viewAction = action;
        }

        private void BackButton_Clicked(object sender, EventArgs e)
        {
            if (App.RootMaster != null)
                App.RootMaster.Detail.Navigation.PopModalAsync(true);
            else
                Application.Current.MainPage.Navigation.PopModalAsync(true);
        }
    }
}