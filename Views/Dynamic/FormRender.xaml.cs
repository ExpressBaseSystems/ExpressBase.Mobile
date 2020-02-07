using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.ViewModels.Dynamic;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ExpressBase.Mobile.Views.Dynamic
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class FormRender : ContentPage
    {
        public FormRenderViewModel Renderer { set; get; }

        //new mode
        public FormRender(EbMobilePage page)
        {
            InitializeComponent();
            try
            {
                Renderer = new FormRenderViewModel(page);
                FormScrollView.Content = Renderer.View;
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
                SaveButton.Text = "Save Changes";
                SaveButton.IsVisible = false;
                EditButton.IsVisible = true;

                Renderer = new FormRenderViewModel(Page, RowId);
                FormScrollView.Content = Renderer.View;
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
                Renderer = new FormRenderViewModel(CurrentForm, ParentForm, ParentId);
                FormScrollView.Content = Renderer.View;
                BindingContext = Renderer;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        //prefill new mode
        public FormRender(EbMobilePage Page, EbDataRow dataRow, ColumnColletion dataColumns)
        {
            InitializeComponent();
            try
            {
                Renderer = new FormRenderViewModel(Page, dataRow, dataColumns);
                FormScrollView.Content = Renderer.View;
                BindingContext = Renderer;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void EditButton_Clicked(object sender, EventArgs e)
        {
            if (Renderer != null)
            {
                EditButton.IsVisible = false;
                SaveButton.IsVisible = true;

                foreach (EbMobileControl Ctrl in Renderer.Form.FlatControls)
                {
                    Ctrl.SetAsReadOnly(false);
                }
            }
        }
    }
}