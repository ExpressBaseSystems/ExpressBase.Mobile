using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.ViewModels;
using ExpressBase.Mobile.Views.Base;
using ExpressBase.Mobile.Views.Shared;
using Newtonsoft.Json;
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

            isMasterPage = hasBackButton;
            EbLayout.HasToolBar = isMasterPage;
            BindingContext = viewModel = new NewSolutionViewModel();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            this.SetContentFromConfig();

            await AppPermission.Camera();
        }

        public void SetContentFromConfig()
        {
            WallLabel.Text = PageContent["WallLabel"];
            SolutionName.Placeholder = PageContent["TextBoxPlaceHolder"];
        }

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
                        await App.Navigation.NavigateToLogin(isMasterPage);
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

        private async void QrButton_Tapped(object sender, EventArgs e)
        {
            bool hasCameraAccess = await AppPermission.Camera();

            if (hasCameraAccess)
            {
                QrScanner scannerPage = new QrScanner();
                scannerPage.BindMethod(QrScannerCallback);

                await App.Navigation.NavigateModalByRenderer(scannerPage);
            }
            else
                Utils.Toast("Allow permission to access camera");
        }

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

                EbLayout.ShowLoader();

                string domain = App.Settings.Vendor.GetDomain();

                if (surl.Split(CharConstants.DOT).Length == 1)
                {
                    surl += $".{domain}";
                    SolutionName.Text = surl;
                }

                response = await viewModel.Validate(surl);

                if (response.IsValid)
                {
                    EbLayout.HideLoader();
                    SolutionLogoPrompt.Source = ImageSource.FromStream(() => new MemoryStream(response.Logo));
                    SolutionLabel.Text = surl.Split(CharConstants.DOT)[0];
                    ShowSIDConfirmBox();
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

        private void ShowSIDConfirmBox()
        {
            PopupContainer.IsVisible = true;
            MainContent.FadeTo(1);
            MainContent.TranslateTo(MainContent.TranslationX, 0);
            ShadowView.IsVisible = true;
        }

        private void HideSIDConfirmBox()
        {
            ShadowView.IsVisible = false;
            var fade = new Animation(v => MainContent.Opacity = v, 1, 0);
            var translation = new Animation(v => MainContent.TranslationY = v, 0, MainContent.Height, null, () =>
            {
                PopupContainer.IsVisible = false;
            });
            var parent = new Animation { { 0.5, 1, fade }, { 0, 1, translation } };
            parent.Commit(this, "HidePopupContainer");
        }

        private void PopupCancel_Clicked(object sender, EventArgs e)
        {
            this.HideSIDConfirmBox();
        }

        private async void ConfirmButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                await viewModel.AddSolution(SolutionName.Text.Trim(), response);

                EbLayout.HideLoader();
                this.HideSIDConfirmBox();

                await App.Navigation.NavigateToLogin(isMasterPage);
            }
            catch (Exception)
            {
                ///
            }
        }
    }
}