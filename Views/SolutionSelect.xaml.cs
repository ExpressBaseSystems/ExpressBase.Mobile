using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.CustomControls;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Services;
using ExpressBase.Mobile.ViewModels;
using RestSharp;
using System;
using System.Net;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
namespace ExpressBase.Mobile.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SolutionSelect : ContentPage
    {
        public SolutionSelect()
        {
            InitializeComponent();
        }

        protected override bool OnBackButtonPressed()
        {
            if(App.RootMaster != null){
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