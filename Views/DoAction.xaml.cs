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
            EbLayout.ShowLoader();
        }

        public DoAction(int actionid)
        {
            InitializeComponent();
            BindingContext = viewModel = new DoActionViewModel(actionid);
            EbLayout.ShowLoader();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (!isRendered)
            {
                await viewModel.InitializeAsync();
                isRendered = true;
            }
            EbLayout.HideLoader();
        }
    }
}