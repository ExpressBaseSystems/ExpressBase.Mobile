using ExpressBase.Mobile.ViewModels.Dynamic.ListView;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ExpressBase.Mobile.Views.Dynamic
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class StaticListRender : ContentPage
    {
        private readonly StaticListViewModel viewModel;

        public StaticListRender(EbMobilePage page)
        {
            InitializeComponent();
            BindingContext = viewModel = new StaticListViewModel(page);
        }
    }
}