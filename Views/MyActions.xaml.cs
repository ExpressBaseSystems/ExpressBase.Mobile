using ExpressBase.Mobile.ViewModels;
using ExpressBase.Mobile.Views.Base;
using Xamarin.Forms.Xaml;

namespace ExpressBase.Mobile.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MyActions : EbContentPage
    {
        private readonly MyActionsViewModel viewModel;

        public MyActions()
        {
            InitializeComponent();
            BindingContext = viewModel = new MyActionsViewModel();
            EbLayout.ShowLoader();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (!IsRendered)
            {
                await viewModel.InitializeAsync();
                IsRendered = true;
            }
            EbLayout.HideLoader();
        }

        public override void UpdateRenderStatus()
        {
            IsRendered = false;
        }

        public override bool CanRefresh()
        {
            return true;
        }
    }
}