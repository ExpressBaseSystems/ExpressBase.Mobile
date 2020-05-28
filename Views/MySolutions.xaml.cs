using ExpressBase.Mobile.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
namespace ExpressBase.Mobile.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MySolutions : ContentPage
    {
        private bool isRendered;

        private readonly MySolutionsViewModel ViewModel;

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

        private async void NewSolution_Tapped(object sender, System.EventArgs e)
        {
            await App.RootMaster.Detail.Navigation.PushAsync(new NewSolution(true));
        }
    }
}