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
        public MySolutionsViewModel ViewModel { set; get; }

        public ValidateSidResponse Response { set; get; }

        public MySolutions()
        {
            InitializeComponent();
            BindingContext = ViewModel = new MySolutionsViewModel();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            if (ViewModel != null && ViewModel.MySolutions.Count <= 0)
                SolutionName.Focus();
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

        void AddSolution_Clicked(object sender, EventArgs e)
        {
            try
            {
                AddSolution.IsVisible = false;
                SaveSolution.IsVisible = true;
                InputArea.BackgroundColor = Color.FromHex("dddddd");
                SolutionName.IsVisible = true;
                SolutionName.Focus();

                scrollView.ScrollToAsync(0, this.Height, true);
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

                PopupContainer.IsVisible = true;
                Response = await RestServices.ValidateSid(SolutionName.Text);
                if (Response.IsValid)
                {
                    Loader.IsVisible = false;
                    SolutionLogoPrompt.Source = ImageSource.FromStream(() => new MemoryStream(Response.Logo));
                    SolutionLabel.Text = SolutionName.Text;
                    SolutionMetaGrid.IsVisible = true;
                }
                else
                {
                    PopupContainer.IsVisible = false;
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
                SolutionMetaGrid.IsVisible = false;
                PopupContainer.IsVisible = false;

                await Application.Current.MainPage.Navigation.PushAsync(new Login());
            }
            catch (Exception ex)
            {
                Log.Write(ex.Message);
            }
        }

        private void DeleteSolution_Clicked(object sender, EventArgs e)
        {
            try
            {
                string sname = (sender as Button).ClassId;
                SolutionInfo info = ViewModel.MySolutions.Single(item => item.SolutionName == sname);
                if (info != null)
                {
                    ViewModel.MySolutions.Remove(info);
                    Store.SetJSON(AppConst.MYSOLUTIONS, new List<SolutionInfo>(ViewModel.MySolutions));
                }

                if (sname == ViewModel.CurrentSolution)
                {
                    Store.ResetSolution();
                    Application.Current.MainPage = new NavigationPage(new MySolutions())
                    {
                        BarBackgroundColor = Color.FromHex("0046bb"),
                        BarTextColor = Color.White
                    };
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex.Message);
            }
        }
    }
}