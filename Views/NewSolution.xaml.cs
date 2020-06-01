using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.ViewModels;
using ExpressBase.Mobile.Views.Shared;
using Newtonsoft.Json;
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
        private readonly NewSolutionViewModel viewModel;

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

            BindingContext = viewModel = new NewSolutionViewModel();
        }

        private void QrScannerCallback(SolutionQrMeta meta)
        {
            if (string.IsNullOrEmpty(meta.Sid) || viewModel.IsSolutionExist(meta.Sid))
                return;

            Device.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    Loader.IsVisible = true;

                    response = await viewModel.Validate(meta.Sid);
                    if (response.IsValid)
                    {
                        await viewModel.AddSolution(meta.Sid, response);

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
                }
            });
        }

        private async void QrButton_Tapped(object sender, EventArgs e)
        {
            QrScanner scannerPage = new QrScanner(isMasterPage);
            scannerPage.BindMethod(QrScannerCallback);

            if (isMasterPage)
                await App.RootMaster.Detail.Navigation.PushModalAsync(scannerPage);
            else
                await Application.Current.MainPage.Navigation.PushModalAsync(scannerPage);
        }

        private async void SaveSolution_Clicked(object sender, EventArgs e)
        {
            try
            {
                string surl = SolutionName.Text.Trim();

                IToast toast = DependencyService.Get<IToast>();
                if (!Utils.HasInternet)
                {
                    toast.Show("Not connected to internet!");
                    return;
                }
                if (string.IsNullOrEmpty(surl) || viewModel.IsSolutionExist(surl))
                    return;

                Loader.IsVisible = true;

                if (surl.Split(CharConstants.DOT).Length == 1)
                {
                    surl += ".expressbase.com";
                    SolutionName.Text = surl;
                }

                response = await viewModel.Validate(surl);

                if (response.IsValid)
                {
                    Loader.IsVisible = false;
                    SolutionLogoPrompt.Source = ImageSource.FromStream(() => new MemoryStream(response.Logo));
                    SolutionLabel.Text = surl;
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
                await viewModel.AddSolution(SolutionName.Text.Trim(), response);

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