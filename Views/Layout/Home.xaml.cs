using ExpressBase.Mobile.CustomControls;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.ViewModels;
using ExpressBase.Mobile.Views.Base;
using ExpressBase.Mobile.Views.Shared;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ExpressBase.Mobile.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Home : EbContentPage, IMasterPage
    {
        private int backButtonCount;

        private readonly HomeViewModel viewModel;

        private bool _syncData;

        public Home(bool syncData = false)
        {
            InitializeComponent();
            BindingContext = viewModel = new HomeViewModel();
            EbLayout.ShowLoader();
            _syncData = syncData;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            try
            {
                CurrentLocation.Text = App.Settings.CurrentLocation?.LongName.ToLower();
                CurrentSolution.Text = App.Settings.SolutionName;

                if (!IsRendered)
                {
                    await viewModel.InitializeAsync();
                    IsRendered = true;
                }
            }
            catch (Exception ex)
            {
                EbLog.Error(ex.Message);
            }
            EbLayout.HideLoader();
            if (_syncData)
                viewModel.SyncData(Loader);
        }

        protected override bool OnBackButtonPressed()
        {
            backButtonCount++;

            if (backButtonCount == 2)
            {
                backButtonCount = 0;
                return false;
            }
            else
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    Utils.Toast("Press again to EXIT!");
                });
                return true;
            }
        }

        public override bool CanRefresh() => viewModel.RefreshOnAppearing;

        public override void UpdateRenderStatus() => IsRendered = false;

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);
            App.ScreenX = width;
        }

        private async void LocationSwitch_Clicked(object sender, EventArgs e)
        {
            await App.Navigation.NavigateMasterAsync(new MyLocations());
        }

        public void UpdateMasterLayout()
        {
            EbLayout.IsMasterPage = true;
            EbLayout.HasBackButton = false;
        }

        public EbCPLayout GetCurrentLayout() => EbLayout;
    }
}