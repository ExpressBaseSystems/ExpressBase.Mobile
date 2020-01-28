using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.ViewModels;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ExpressBase.Mobile.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class FormRender : ContentPage
    {
        //new mode
        public FormRender(EbMobilePage page)
        {
            InitializeComponent();
            try
            {
                var Renderer = new FormRenderViewModel(page);
                this.Content = Renderer.View;
                BindingContext = Renderer;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        //edit mode
        public FormRender(EbMobilePage Page, int RowId)
        {
            InitializeComponent();
            try
            {
                var Renderer = new FormRenderViewModel(Page, RowId);
                this.Content = Renderer.View;
                BindingContext = Renderer;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        //reference mode
        public FormRender(EbMobilePage CurrentForm, EbMobilePage ParentForm, int ParentId)
        {
            InitializeComponent();
            try
            {
                var Renderer = new FormRenderViewModel(CurrentForm, ParentForm, ParentId);
                this.Content = Renderer.View;
                BindingContext = Renderer;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
        }

        protected override bool OnBackButtonPressed()
        {
            return false;
        }
    }
}