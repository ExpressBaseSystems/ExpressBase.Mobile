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
            {
                return base.OnBackButtonPressed();
            }
        }

        private void SolutionUrl_TextChanged(object sender, TextChangedEventArgs e)
        {
            string text = (sender as TextBox).Text;

            if (string.IsNullOrEmpty(text))
            {
                (BindingContext as SolutionSelectViewModel).IsSaveEnabled = false;
            }
            else
            {

                string url = Settings.RootUrl ?? string.Empty;

                if (text == url.Replace("https://", string.Empty))
                    (BindingContext as SolutionSelectViewModel).IsSaveEnabled = false;
                else
                    (BindingContext as SolutionSelectViewModel).IsSaveEnabled = true;
            }
        }
    }
}