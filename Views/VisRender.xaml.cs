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
        public VisRender(EbMobilePage Page)
        {
            InitializeComponent();
            try
            {
                var Renderer = new VisRenderViewModel(Page);
                this.Content = Renderer.View;
                BindingContext = Renderer;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public VisRender(EbMobilePage Page,bool Not)
        {
            InitializeComponent();
            try
            {
                var Renderer = new VisRenderViewModel(Page);
                this.Content = Renderer.View;
                BindingContext = Renderer;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}