using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.ViewModels.Dynamic;
using System;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ExpressBase.Mobile.Views.Dynamic
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class FormRender : ContentPage
    {
        public FormRenderViewModel ViewModel { set; get; }

        //new mode
        public FormRender(EbMobilePage page)
        {
            InitializeComponent();
            try
            {
                BindingContext = ViewModel = new FormRenderViewModel(page);
                FormScrollView.Content = ViewModel.XView;
            }
            catch (Exception ex)
            {
                Log.Write("FormRender new mode" + ex.Message);
            }
        }

        //edit/prefill mode
        public FormRender(EbMobilePage page, int rowId, FormMode mode)
        {
            InitializeComponent();
            try
            {
                BindingContext = ViewModel = new FormRenderViewModel(page, rowId, mode);
                FormScrollView.Content = ViewModel.XView;

                SaveButton.Text = "Save Changes";
                SaveButton.IsVisible = false;
                EditButton.IsVisible = true;
            }
            catch (Exception ex)
            {
                Log.Write("FormRender edit mode" + ex.Message);
            }
        }

        //reference mode
        public FormRender(EbMobilePage currentForm, EbMobilePage parentForm, int parentId)
        {
            InitializeComponent();
            try
            {
                BindingContext = ViewModel = new FormRenderViewModel(currentForm, parentForm, parentId);
                FormScrollView.Content = ViewModel.XView;
            }
            catch (Exception ex)
            {
                Log.Write("FormRender reference mode" + ex.Message);
            }
        }

        private void EditButton_Clicked(object sender, EventArgs e)
        {
            if (ViewModel != null)
            {
                EditButton.IsVisible = false;
                SaveButton.IsVisible = true;

                foreach (var pair in ViewModel.Form.ControlDictionary)
                    pair.Value.SetAsReadOnly(false);
            }
        }

        public void ShowFullScreenImage(object tapedImage)
        {
            if(tapedImage != null)
            {
                ImageFullScreen.Source = (tapedImage as Image).Source;
                ImageFullScreen.Show();
            }
        }
    }
}