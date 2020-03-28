using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.Helpers;
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
                {
                    toast.Show("Not connected to internet!");
                    return;
                }

                var info = ViewModel.MySolutions.Find(item => item.SolutionName == SolutionName.Text.Trim().Split('.')[0]);
                if (info != null)
                    return;

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
            catch (Exception ex)
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
                SolutionInfo info = ViewModel.MySolutions.Find(item => item.SolutionName == sname);
                if (info != null)
                {
                    ViewModel.MySolutions.Remove(info);
                    Store.SetJSON(AppConst.MYSOLUTIONS, ViewModel.MySolutions);
                }

                string current = Store.GetValue(AppConst.SID);
                if(sname== current)
                {
                    Store.ResetSolution();
                    Application.Current.MainPage = new NavigationPage(new SolutionSelect())
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