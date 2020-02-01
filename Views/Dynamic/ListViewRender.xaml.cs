using ExpressBase.Mobile.ViewModels.Dynamic;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ExpressBase.Mobile.Views.Dynamic
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ListViewRender : ContentPage
    {
        public ListViewRender(EbMobilePage Page)
        {
            InitializeComponent();
            try
            {
                var Renderer = new ListViewRenderViewModel(Page);
                listContainer.Content = Renderer.View;
                BindingContext = Renderer;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}