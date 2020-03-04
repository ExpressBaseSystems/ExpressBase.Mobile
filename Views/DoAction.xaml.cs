using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ExpressBase.Mobile.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DoAction : ContentPage
    {
        public DoAction()
        {
            InitializeComponent();
        }
        public DoAction(EbMyAction action)
        {
            InitializeComponent();
            BindingContext = new DoActionViewModel(action);
        }
    }
}