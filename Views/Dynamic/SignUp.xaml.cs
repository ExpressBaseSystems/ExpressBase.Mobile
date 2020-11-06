using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.ViewModels.Dynamic;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ExpressBase.Mobile.Views.Dynamic
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SignUp : ContentPage
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
    }
}