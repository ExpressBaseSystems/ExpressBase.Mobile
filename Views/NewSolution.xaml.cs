using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.CustomControls;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Services;
using ExpressBase.Mobile.ViewModels;
using ExpressBase.Mobile.Views.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ExpressBase.Mobile.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class NewSolution : ContentPage, IDynamicContent
    {
        private readonly NewSolutionViewModel viewModel;

        private ValidateSidResponse response;

        private readonly bool isMasterPage;

        public Dictionary<string, string> PageContent => App.Settings.Vendor.Content.NewSolution;

        public NewSolution(bool hasBackButton = false)
        {
            InitializeComponent();

            try
            {
                isMasterPage = hasBackButton;
                if (hasBackButton)
                {
                    NavigationPage.SetHasNavigationBar(this, true);
                    NavigationPage.SetHasBackButton(this, true);
                }

                BindingContext = viewModel = new NewSolutionViewModel();
            }
            catch(Exception ex)
            {
                DependencyService.Get<IToast>().Show(ex.Message);
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            SetContentFromConfig();
        }

        public void SetContentFromConfig()
        {
            WallLabel.Text = PageContent["WallLabel"];
            SolutionName.Placeholder = PageContent["TextBoxPlaceHolder"];
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
                                BarBackgroundColor = App.Settings.Vendor.GetPrimaryColor(),
                                BarTextColor = Color.White
                            };
                        }
                        await NavigationService.LoginWithCS();
                    }
                    else
                        throw new Exception("invalid qr code meta");

                    Loader.IsVisible = true;
                }
                catch (Exception ex)
                {
                    EbLog.Error(ex.Message);
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
                    Utils.Alert_NoInternet(toast);
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
                    SolutionLabel.Text = surl.Split(CharConstants.DOT)[0];
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
                EbLog.Error(ex.Message);
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
                        BarBackgroundColor = App.Settings.Vendor.GetPrimaryColor(),
                        BarTextColor = Color.White
                    };
                }
                await NavigationService.LoginWithCS();
            }
            catch (Exception ex)
            {
                EbLog.Error(ex.Message);
            }
        }
    }
}