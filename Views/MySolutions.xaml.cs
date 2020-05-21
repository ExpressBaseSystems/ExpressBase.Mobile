using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Services;
using ExpressBase.Mobile.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
namespace ExpressBase.Mobile.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MySolutions : ContentPage
    {
        private bool isRendered;

        public MySolutionsViewModel ViewModel { set; get; }

        public ValidateSidResponse Response { set; get; }

        public MySolutions()
        {
            InitializeComponent();
            BindingContext = ViewModel = new MySolutionsViewModel();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (!isRendered)
            {
                await ViewModel.InitializeAsync();
                isRendered = true;
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
            PopupContainer.IsVisible = false;
        }

        async void AddSolution_Clicked(object sender, EventArgs e)
        {
            try
            {
                AddSolution.IsVisible = false;
                SaveSolution.IsVisible = true;
                InputArea.BackgroundColor = Color.FromHex("dddddd");
                SolutionName.IsVisible = true;
                SolutionName.Focus();

                await scrollView.ScrollToAsync(0, this.Height, true);
            }
            catch (Exception ex)
            {
                Log.Write(ex.Message);
            }
        }

        private async void SaveSolution_Clicked(object sender, EventArgs e)
        {
            try
            {
                IToast toast = DependencyService.Get<IToast>();
                if (!Settings.HasInternet)
                {
                    toast.Show("Not connected to internet!");
                    return;
                }
                if (string.IsNullOrEmpty(SolutionName.Text)) return;
                if (ViewModel.MySolutions.Any(item => item.SolutionName == SolutionName.Text.Trim().Split('.')[0])) return;

                Loader.IsVisible = true;
                Response = await RestServices.ValidateSid(SolutionName.Text);
                if (Response.IsValid)
                {
                    Loader.IsVisible = false;
                    SolutionLogoPrompt.Source = ImageSource.FromStream(() => new MemoryStream(Response.Logo));
                    SolutionLabel.Text = SolutionName.Text;
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
                Log.Write(ex.Message);
            }
        }

        private async void ConfirmButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                await ViewModel.AddSolution(SolutionName.Text, Response);

                Loader.IsVisible = true;
                PopupContainer.IsVisible = false;

                await Application.Current.MainPage.Navigation.PushAsync(new Login());
            }
            catch (Exception ex)
            {
                Log.Write(ex.Message);
            }
        }

        private async void DeleteSolution_Clicked(object sender, EventArgs e)
        {
            try
            {
                string sname = (sender as Button).ClassId;
                await ViewModel.RemoveSolution(sname);
            }
            catch (Exception ex)
            {
                Log.Write(ex.Message);
            }
        }
    }
}