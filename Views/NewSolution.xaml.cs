using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.ViewModels;
using System;
using System.IO;
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

        private bool isMasterPage;

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
            Device.BeginInvokeOnMainThread(async () =>
            {
                await DisplayAlert("Scanned result", result.Text, "OK");
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