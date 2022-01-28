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

        public Redirect(string message, MessageType type = MessageType.no_data)
        {
            InitializeComponent();
            Message = message;
            if (type != MessageType.no_data)
            {
                Image.Source = type + ".png";
                Image.HeightRequest = 200;
            }

            BindingContext = this;
        }

        private async void Button_Clicked(object sender, System.EventArgs e)
        {
            await App.Navigation.PopMasterAsync(true);
        }
    }

    public enum MessageType
    {
        no_data,
        disconnected
    }
}