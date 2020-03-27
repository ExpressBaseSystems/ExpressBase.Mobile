using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Services;
using ExpressBase.Mobile.ViewModels;
using System;
using System.IO;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
namespace ExpressBase.Mobile.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SolutionSelect : ContentPage
    {
        public SolutionSelectViewModel ViewModel { set; get; }

        public SolutionSelect()
        {
            InitializeComponent();
            ViewModel = new SolutionSelectViewModel();
            BindingContext = ViewModel;

            if (!Settings.HasInternet)
                ViewModel.ShowMessageBox("You are not connected to internet!", Color.FromHex("fd6b6b"));
            else
                ViewModel.HideMessageBox();

            Connectivity.ConnectivityChanged += Connectivity_ConnectivityChanged;
        }

        private void Connectivity_ConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
        {
            if (ViewModel != null)
            {
                if (e.NetworkAccess == NetworkAccess.Internet)
                    ViewModel.HideMessageBox();
                else
                    ViewModel.ShowMessageBox("You are not connected to internet!", Color.FromHex("fd6b6b"));
            }
        }

        protected override bool OnBackButtonPressed()
        {
            if (App.RootMaster != null)
            {
                Application.Current.MainPage = App.RootMaster;
                return true;
            }
            else
                return base.OnBackButtonPressed();
        }

        private void PopupCancel_Clicked(object sender, EventArgs e)
        {
            SolutionMetaGrid.IsVisible = false;
            PopupContainer.IsVisible = false;
        }

        private async void AddSolution_Clicked(object sender, EventArgs e)
        {
            try
            {
                IToast toast = DependencyService.Get<IToast>();
                if (!Settings.HasInternet)
                    toast.Show("Not connected to internet!");

                PopupContainer.IsVisible = true;
                ValidateSidResponse resp = await RestServices.ValidateSid(SolutionName.Text);
                if (resp.IsValid)
                {
                    Loader.IsVisible = false;
                    SolutionLogoPrompt.Source = ImageSource.FromStream(() => new MemoryStream(resp.Logo));
                    SolutionLabel.Text = SolutionName.Text;
                    SolutionMetaGrid.IsVisible = true;
                }
                else
                {
                    PopupContainer.IsVisible = false;
                    toast.Show("Invalid solution URL");
                }
            }
            catch(Exception ex)
            {
                Log.Write(ex.Message);
            }
        }

        private async void ConfirmButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                await ViewModel.AddSolution();

                Loader.IsVisible = true;
                SolutionMetaGrid.IsVisible = false;
                PopupContainer.IsVisible = false;

                await Application.Current.MainPage.Navigation.PushAsync(new Login());
            }
            catch(Exception ex)
            {
                Log.Write(ex.Message);
            }
        }
    }
}