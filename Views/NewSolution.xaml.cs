using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.CustomControls;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Services;
using ExpressBase.Mobile.ViewModels;
using ExpressBase.Mobile.Views.Shared;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ExpressBase.Mobile.Views
{
    /// <summary>
    /// Page to add new solution
    /// Includes QR scanner
    /// </summary>
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class NewSolution : ContentPage, IDynamicContent
    {
        private readonly NewSolutionViewModel viewModel;

        private ValidateSidResponse response;

        private readonly bool isMasterPage;

        ///<value> return Dynamic content dictionary from vendor JSON </value>
        public Dictionary<string, string> PageContent => App.Settings.Vendor.Content.NewSolution;

        public NewSolution(bool hasBackButton = false)
        {
            InitializeComponent();

            isMasterPage = hasBackButton;
            EbLayout.HasToolBar = isMasterPage;
            BindingContext = viewModel = new NewSolutionViewModel();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            this.SetContentFromConfig();

            await viewModel.GetCameraAccess();
        }

        public void SetContentFromConfig()
        {
            WallLabel.Text = PageContent["WallLabel"];
            SolutionName.Placeholder = PageContent["TextBoxPlaceHolder"];
        }

        /// <summary>
        /// Callbak method on qr scan
        /// </summary>
        /// <param name="meta"></param>
        private void QrScannerCallback(string payload)
        {
            SolutionQrMeta meta = null;
            try
            {
                meta = JsonConvert.DeserializeObject<SolutionQrMeta>(payload);
            }
            catch (Exception ex)
            {
                EbLog.Info("failed to parse qr payload in new solution page");
                EbLog.Error(ex.Message);
                return;
            }

            if (meta == null)
                return;

            Device.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    if (string.IsNullOrEmpty(meta.Sid) || viewModel.IsSolutionExist(meta.Sid))
                    {
                        Utils.Toast($"{meta.Sid} exist");
                        return;
                    }
                    EbLayout.ShowLoader();
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
                        Utils.Toast("invalid qr code meta");

                    EbLayout.HideLoader();
                }
                catch (Exception)
                {
                    EbLayout.HideLoader();
                }
            });
        }

        /// <summary>
        /// qr show button taped event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void QrButton_Tapped(object sender, EventArgs e)
        {
            bool hasCameraAccess = await viewModel.GetCameraAccess();

            if (hasCameraAccess)
            {
                QrScanner scannerPage = new QrScanner();
                scannerPage.BindMethod(QrScannerCallback);

                if (App.RootMaster != null)
                    await App.RootMaster.Detail.Navigation.PushModalAsync(scannerPage);
                else
                    await Application.Current.MainPage.Navigation.PushModalAsync(scannerPage);
            }
            else
                Utils.Toast("Allow permission to access camera");
        }

        /// <summary>
        /// Validate solution when solution url is typing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void SaveSolution_Clicked(object sender, EventArgs e)
        {
            try
            {
                string surl = SolutionName.Text.Trim();

                if (!Utils.HasInternet)
                {
                    Utils.Alert_NoInternet();
                    return;
                }
                if (string.IsNullOrEmpty(surl) || viewModel.IsSolutionExist(surl))
                    return;

                EbLayout.ShowLoader(); ;

                if (surl.Split(CharConstants.DOT).Length == 1)
                {
                    surl += ".expressbase.com";
                    SolutionName.Text = surl;
                }
                //api call for validating solution
                response = await viewModel.Validate(surl);

                if (response.IsValid)
                {
                    EbLayout.HideLoader();
                    SolutionLogoPrompt.Source = ImageSource.FromStream(() => new MemoryStream(response.Logo));
                    SolutionLabel.Text = surl.Split(CharConstants.DOT)[0];
                    PopupContainer.IsVisible = true;
                }
                else
                {
                    EbLayout.HideLoader();
                    Utils.Toast("Invalid solution URL");
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

        /// <summary>
        /// After validation api response confirm solution when propmt
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ConfirmButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                await viewModel.AddSolution(SolutionName.Text.Trim(), response);

                EbLayout.HideLoader();
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
            catch (Exception)
            {
                ///
            }
        }
    }
}