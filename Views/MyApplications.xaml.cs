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
        private bool isRendered = false;

        private readonly MyApplicationsViewModel viewModel;

        private readonly bool isInternal;

        public MyApplications()
        {
            InitializeComponent();

            EbLayout.ShowLoader();
            BindingContext = viewModel = new MyApplicationsViewModel();
            EbLayout.HasBackButton = false;
        }

        public MyApplications(bool is_internal)
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
                    viewModel.Initialize();

                    bool flag = !viewModel.IsNullOrEmpty() && viewModel.Applications.Count == 1;

                    if (!isInternal && flag)
                    {
                        await viewModel.AppSelected(viewModel.Applications[0]);
                    }
                    isRendered = true;
                }
            }
            catch (Exception ex)
            {
                EbLog.Error(ex.Message);
            }
            EbLayout.HideLoader();
        }

        private void ResetButton_Clicked(object sender, EventArgs e)
        {
            ConfimReset.Show();
        }

        protected override bool OnBackButtonPressed()
        {
            return !isInternal;
        }
    }
}