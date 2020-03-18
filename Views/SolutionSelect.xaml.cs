using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.ViewModels;
using System;
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
                ViewModel.HideMessageBox("Back to online", Color.FromHex("41d041"));

            Connectivity.ConnectivityChanged += Connectivity_ConnectivityChanged; ;
        }

        private void Connectivity_ConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
        {
            if (ViewModel != null)
            {
                if (e.NetworkAccess == NetworkAccess.Internet)
                    ViewModel.HideMessageBox("Back to online", Color.FromHex("41d041"));
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

        private void EditSolutionButton_Clicked(object sender, EventArgs e)
        {
            SolutionUrl.IsEnabled = true;
        }

        private void PopupCancel_Clicked(object sender, EventArgs e)
        {
            SolutionMetaGrid.IsVisible = false;
            PopupContainer.IsVisible = false;
        }
    }
}