using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using ZXing;

namespace ExpressBase.Mobile.Views.Shared
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class QrScanner : ContentPage
    {
        private Action<SolutionQrMeta> viewAction;

        private readonly bool isMasterPage;

        public QrScanner(bool ismaster = false)
        {
            isMasterPage = ismaster;
            InitializeComponent();
        }

        private void ScannerView_OnScanResult(Result result)
        {
            try
            {
                SolutionQrMeta meta = JsonConvert.DeserializeObject<SolutionQrMeta>(result.Text);

                if (meta != null)
                {
                    viewAction?.Invoke(meta);
                    if (isMasterPage)
                        App.RootMaster.Detail.Navigation.PopModalAsync(true);
                    else
                        Application.Current.MainPage.Navigation.PopModalAsync(true);
                }
            }
            catch (Exception)
            {
                Log.Write("Invalid qr code");
            }
        }

        public void BindMethod(Action<SolutionQrMeta> action)
        {
            viewAction = action;
        }
    }
}