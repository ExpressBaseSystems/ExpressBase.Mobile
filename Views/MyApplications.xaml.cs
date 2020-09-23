using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.ViewModels;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ExpressBase.Mobile.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MyApplications : ContentPage
    {
        private bool isRendered;

        private readonly MyApplicationsViewModel viewModel;

        private readonly bool isInternal;

        public MyApplications(bool is_internal = false)
        {
            isInternal = is_internal;

            InitializeComponent();
            EbLayout.ShowLoader();
            BindingContext = viewModel = new MyApplicationsViewModel();

            if (isInternal)
                ResetButton.IsVisible = false;
            else
                EbLayout.HasBackButton = false;
        }

        protected async override void OnAppearing()
        {
            base.OnAppearing();

            try
            {
                CurrentLocation.Text = App.Settings.CurrentLocation?.LongName.ToLower();

                if (!isRendered)
                {
                    await viewModel.InitializeAsync();

                    if (!isInternal && viewModel.Applications.Count == 1)
                    {
                        await viewModel.AppSelected(viewModel.Applications[0]);
                    }
                    isRendered = true;
                }

                ToggleStatus();
                EbLayout.HideLoader();
            }
            catch (Exception ex)
            {
                EbLog.Error(ex.Message);
                EbLayout.HideLoader();
            }
        }

        private async void ApplicationsRefresh_Refreshing(object sender, EventArgs e)
        {
            if (!Utils.HasInternet)
            {
                Utils.Alert_NoInternet();
                return;
            }

            try
            {
                Store.RemoveJSON(AppConst.APP_COLLECTION);
                await viewModel.UpdateAsync();
            }
            catch (Exception ex)
            {
                EbLog.Error("Failed to refresh applications" + ex.Message);
            }

            ToggleStatus();
            ApplicationsRefresh.IsRefreshing = false;
        }

        private void ResetButton_Clicked(object sender, EventArgs e)
        {
            ConfimReset.Show();
        }

        private void ToggleStatus()
        {
            EmptyMessage.IsVisible = viewModel.IsEmpty();
        }
    }
}