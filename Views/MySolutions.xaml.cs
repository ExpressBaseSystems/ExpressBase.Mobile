using ExpressBase.Mobile.ViewModels;
using ExpressBase.Mobile.Views.Base;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
namespace ExpressBase.Mobile.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MySolutions : ContentPage, IDynamicContent
    {
        private bool isRendered;

        private readonly bool isMasterPage;

        private readonly MySolutionsViewModel viewModel;

        public Dictionary<string, string> PageContent => App.Settings.Vendor.Content.MySolutions;

        public MySolutions(bool ismaster = false)
        {
            isMasterPage = ismaster;
            InitializeComponent();

            BindingContext = viewModel = new MySolutionsViewModel();
            EbLayout.HasBackButton = isMasterPage;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (!isRendered)
            {
                SetContentFromConfig();
                await viewModel.InitializeAsync();
                isRendered = true;
            }
        }

        public void SetContentFromConfig()
        {
            EbLayout.Title = PageContent["Title"];
        }

        private async void NewSolution_Clicked(object sender, System.EventArgs e)
        {
            if (isMasterPage)
                await App.RootMaster.Detail.Navigation.PushAsync(new NewSolution(true));
            else
                await Application.Current.MainPage.Navigation.PushAsync(new NewSolution());
        }
    }
}