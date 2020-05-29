using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.ViewModels;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using ZXing;

namespace ExpressBase.Mobile.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class NewSolution : ContentPage
    {
        private readonly NewSolutionViewModel ViewModel;

        private ValidateSidResponse response;

        private readonly bool isMasterPage;

        public NewSolution(bool hasBackButton = false)
        {
            InitializeComponent();
            isMasterPage = hasBackButton;
            if (hasBackButton)
            {
                NavigationPage.SetHasNavigationBar(this, true);
                NavigationPage.SetHasBackButton(this, true);
            }

            BindingContext = ViewModel = new NewSolutionViewModel();
        }

        public void QrScanner_OnScanResult(Result result)
        {
            IToast toast = DependencyService.Get<IToast>();
            QrScanner.IsAnalyzing = false;
            SolutionQrMeta meta = JsonConvert.DeserializeObject<SolutionQrMeta>(result.Text);
            if (meta != null)
            {
                if (!Utils.HasInternet)
                {
                    toast.Show("Not connected to internet!");
                    return;
                }
                if (string.IsNullOrEmpty(meta.sid) || ViewModel.IsSolutionExist(meta.sid))
                    return;

                AddSolutionQrScanned(meta);
            }
        }

        private void AddSolutionQrScanned(SolutionQrMeta meta)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    QrScannerView.IsVisible = false;
                    Loader.IsVisible = true;

                    response = await ViewModel.Validate(meta.sid);
                    if (response.IsValid)
                    {
                        await ViewModel.AddSolution(meta.sid, response);

                        if (isMasterPage)
                        {
                            App.RootMaster = null;
                            Application.Current.MainPage = new NavigationPage
                            {
                                BarBackgroundColor = Color.FromHex("0046bb"),
                                BarTextColor = Color.White
                            };
                        }
                        await Application.Current.MainPage.Navigation.PushAsync(new Login());
                    }
                    else
                        throw new Exception("invalid qr code meta");

                    Loader.IsVisible = true;
                }
                catch (Exception ex)
                {
                    Log.Write(ex.Message);
                    Loader.IsVisible = true;
                    QrScannerView.IsVisible = false;
                }
            });
        }

        private void QrButton_Tapped(object sender, EventArgs e)
        {
            QrScannerView.IsVisible = true;
            QrScanner.IsScanning = true;
        }

        protected override bool OnBackButtonPressed()
        {
            if (QrScannerView.IsVisible)
            {
                QrScannerView.IsVisible = false;
                QrScanner.IsScanning = false;
                return true;
            }
            else
            {
                return base.OnBackButtonPressed();
            }
        }

        private async void SaveSolution_Clicked(object sender, EventArgs e)
        {
            try
            {
                IToast toast = DependencyService.Get<IToast>();
                if (!Utils.HasInternet)
                {
                    toast.Show("Not connected to internet!");
                    return;
                }
                if (string.IsNullOrEmpty(SolutionName.Text) || ViewModel.IsSolutionExist(SolutionName.Text))
                    return;

                Loader.IsVisible = true;
                response = await ViewModel.Validate(SolutionName.Text.Trim());

                if (response.IsValid)
                {
                    Loader.IsVisible = false;
                    SolutionLogoPrompt.Source = ImageSource.FromStream(() => new MemoryStream(response.Logo));
                    SolutionLabel.Text = SolutionName.Text;
                    PopupContainer.IsVisible = true;
                }
                else
                {
                    Loader.IsVisible = false;
                    toast.Show("Invalid solution URL");
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex.Message);
            }
        }



        private void PopupCancel_Clicked(object sender, EventArgs e)
        {
            PopupContainer.IsVisible = false;
        }

        private async void ConfirmButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                await ViewModel.AddSolution(SolutionName.Text, response);

                Loader.IsVisible = true;
                PopupContainer.IsVisible = false;

                if (isMasterPage)
                {
                    App.RootMaster = null;
                    Application.Current.MainPage = new NavigationPage
                    {
                        BarBackgroundColor = Color.FromHex("0046bb"),
                        BarTextColor = Color.White
                    };
                }
                await Application.Current.MainPage.Navigation.PushAsync(new Login());
            }
            catch (Exception ex)
            {
                Log.Write(ex.Message);
            }
        }
    }
}