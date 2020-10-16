using ExpressBase.Mobile.Models;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ExpressBase.Mobile.Views.Shared
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class GeoMapView : ContentPage
    {
        public GeoMapView()
        {
            InitializeComponent();
        }

        public GeoMapView(EbMapBinding binding)
        {
            InitializeComponent();

            BindingContext = binding;

            if (binding.Location != null)
            {
                GMapControl.SetLocation(binding.Location.Latitude, binding.Location.Longitude);
            }
        }
    }
}