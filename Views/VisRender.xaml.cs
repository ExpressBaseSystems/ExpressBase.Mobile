using ExpressBase.Mobile.CustomControls;
using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.DynamicRenders;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ExpressBase.Mobile.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class VisRender : ContentPage
    {
        public VisRender(EbMobilePage page)
        {
            InitializeComponent();
            try
            {
                var Renderer = new VisRenderViewModel(page);
                this.Content = Renderer.View;
                BindingContext = Renderer;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}