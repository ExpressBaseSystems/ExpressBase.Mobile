using ExpressBase.Mobile.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
namespace ExpressBase.Mobile.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MySolutions : ContentPage
    {
        private bool isRendered;

        private readonly bool isMasterPage;

        private readonly MySolutionsViewModel viewModel;

        public MySolutions(bool ismaster = false)
        {
            isMasterPage = ismaster;
            InitializeComponent();
            BindingContext = viewModel = new MySolutionsViewModel();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (!isRendered)
            {
                await viewModel.InitializeAsync();
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

        private async void NewSolution_Tapped(object sender, System.EventArgs e)
        {
            if (isMasterPage)
                await App.RootMaster.Detail.Navigation.PushAsync(new NewSolution(true));
            else
                await Application.Current.MainPage.Navigation.PushAsync(new NewSolution());
        }
    }
}