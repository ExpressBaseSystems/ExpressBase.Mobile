using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.ViewModels.Dynamic;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ExpressBase.Mobile.Views.Dynamic
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DashBoardRender : ContentPage
    {
        public DashBoardRender()
        {
            InitializeComponent();

            BindingContext = new DashBoardRenderViewModel();
        }

        public DashBoardRender(EbMobilePage Page)
        {
            InitializeComponent();

            DashBoardRenderViewModel model = new DashBoardRenderViewModel(Page);
            BindingContext = model;

            this.DashBoardContainer.Content = model.View;
        }

        public DashBoardRender(EbMobilePage Page,EbDataRow Row)
        {
            InitializeComponent();
            DashBoardRenderViewModel model = new DashBoardRenderViewModel(Page, Row);
            BindingContext = model;

            this.DashBoardContainer.Content = model.View;
        }
    }
}