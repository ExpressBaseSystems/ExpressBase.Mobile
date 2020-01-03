using ExpressBase.Mobile.CustomControls;
using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Services;
using ExpressBase.Mobile.ViewModels;
using System;
using System.Collections.Generic;
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
        public FormRender(EbMobilePage Page, EbDataRow CurrentRow, ColumnColletion Columns)
        {
            InitializeComponent();
            try
            {
                var Renderer = new FormRenderViewModel(Page, CurrentRow, Columns);
                this.Content = Renderer.View;
                BindingContext = Renderer;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        //reference mode
        public FormRender(EbMobilePage CurrentForm, EbMobilePage ParentForm, EbDataRow CurrentRow)
        {
            InitializeComponent();
            try
            {
                var Renderer = new FormRenderViewModel(CurrentForm, ParentForm, CurrentRow);
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