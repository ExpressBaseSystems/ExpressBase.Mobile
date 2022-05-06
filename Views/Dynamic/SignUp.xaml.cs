using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.ViewModels.Dynamic;
using ExpressBase.Mobile.Views.Base;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ExpressBase.Mobile.Views.Dynamic
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SignUp : ContentPage, IFormRenderer
    {
        private bool isRendered;

        private readonly SignUpViewModel viewModel;

        public SignUp()
        {
            InitializeComponent();

            EbMobilePage page = App.Settings.CurrentSolution.GetSignUpPage();
            BindingContext = viewModel = new SignUpViewModel(page);
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            EbLayout.ShowLoader();
            viewModel.MsgLoader = EbLayout.GetMessageLoader();
            if (!isRendered)
            {
                await viewModel.InitializeAsync();

                if (!viewModel.HasWebFormRef && viewModel.IsOnline())
                {
                    EbLog.Info($"Webform refid not configued for form '{viewModel.Page.Name}'");
                    SaveButton.IsEnabled = false;
                }
                isRendered = true;
            }
            EbLayout.HideLoader();
        }

        protected override bool OnBackButtonPressed()
        {
            return true;
        }

        public void ShowFullScreenImage(ImageSource source)
        {
            if (source != null)
            {
                ImageFullScreen.SetSource(source).Show();
            }
        }
    }
}