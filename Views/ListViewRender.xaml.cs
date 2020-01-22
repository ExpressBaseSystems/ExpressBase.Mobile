using ExpressBase.Mobile.CustomControls;
using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.ViewModels;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ExpressBase.Mobile.Views
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