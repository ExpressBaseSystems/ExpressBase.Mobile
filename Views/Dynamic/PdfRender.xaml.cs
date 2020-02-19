using ExpressBase.Mobile.ViewModels.Dynamic;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ExpressBase.Mobile.Views.Dynamic
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PdfRender : ContentPage
    {
        public PdfRenderViewModel Renderer { set; get; }

        public PdfRender()
        { 
            InitializeComponent();
        }

        public PdfRender(EbMobilePage page)
        {
            InitializeComponent();
            Renderer = new PdfRenderViewModel(page);
            this.BindingContext = Renderer;
        }
    }
}