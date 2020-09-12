using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ExpressBase.Mobile.Views.Shared
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Redirect : ContentPage
    {
        public string Message { set; get; }

        public Redirect()
        {
            InitializeComponent();
        }

        public Redirect(string message)
        {
            InitializeComponent();
            Message = message;

            BindingContext = this;
        }

        private async void Button_Clicked(object sender, System.EventArgs e)
        {
            await App.RootMaster.Detail.Navigation.PopAsync(true);
        }
    }
}