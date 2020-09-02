using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ExpressBase.Mobile.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DoAction : ContentPage
    {
        public bool isRendered;

        private readonly DoActionViewModel viewModel;

        public DoAction(EbMyAction action)
        {
            InitializeComponent();
            BindingContext = viewModel = new DoActionViewModel(action);
            Loader.IsVisible = true;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (!isRendered)
            {
                await viewModel.InitializeAsync();
                isRendered = true;
            }
            Loader.IsVisible = false;
        }
    }
}