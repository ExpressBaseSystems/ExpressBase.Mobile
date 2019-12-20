using ExpressBase.Mobile.CustomControls;
using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.DynamicRenders;
using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Services;
using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ExpressBase.Mobile.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class FormRender : ContentPage
    {
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

        public FormRender(EbMobileVisualization Visualization,EbDataRow CurrentRow,ColumnColletion Columns)
        {
            InitializeComponent();
            try
            {
                var Renderer = new FormRenderViewModel(Visualization, CurrentRow, Columns);
                this.Content = Renderer.View;
                BindingContext = Renderer;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        protected override bool OnBackButtonPressed()
        {
            return false;
        }
    }
}