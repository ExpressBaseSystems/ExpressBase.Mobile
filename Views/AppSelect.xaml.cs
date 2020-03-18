using ExpressBase.Mobile.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ExpressBase.Mobile.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AppSelect : ContentPage
    {
        public AppSelect()
        {
            InitializeComponent();
            BindingContext = new AppSelectViewModel();
        }
    }
}