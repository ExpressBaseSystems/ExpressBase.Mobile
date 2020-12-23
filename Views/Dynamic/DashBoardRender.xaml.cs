using ExpressBase.Mobile.ViewModels.Dynamic;
using ExpressBase.Mobile.Views.Base;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ExpressBase.Mobile.Views.Dynamic
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DashBoardRender : ContentPage, IDashBoardRenderer
    {
        private bool isRendered;

        private readonly DashBoardRenderViewModel viewModel;

        public DashBoardRender()
        {
            InitializeComponent();
            BindingContext = new DashBoardRenderViewModel();
        }

        public DashBoardRender(EbMobilePage Page)
        {
            InitializeComponent();
            BindingContext = viewModel = new DashBoardRenderViewModel(Page);
        }

        protected async override void OnAppearing()
        {
            EbLayout.ShowLoader();

            if (!isRendered)
            {
                await viewModel.InitializeAsync();
                isRendered = true;
            }
            EbLayout.HideLoader();
        }

        public void HideToolBar()
        {
            EbLayout.HasToolBar = false;
        }
    }
}