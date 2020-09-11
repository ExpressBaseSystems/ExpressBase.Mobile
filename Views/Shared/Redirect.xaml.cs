using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ExpressBase.Mobile.Views.Shared
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Redirect : ContentPage
    {
        public Redirect()
        {
            InitializeComponent();
        }

        public Redirect(string message)
        {
            InitializeComponent();
        }
    }
}