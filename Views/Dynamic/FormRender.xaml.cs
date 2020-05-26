using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.ViewModels.Dynamic;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ExpressBase.Mobile.Views.Dynamic
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class FormRender : ContentPage
    {
        private bool IsRendered;

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

        //edit
        public FormRender(EbMobilePage page, int rowId)
        {
            InitializeComponent();
            try
            {
                BindingContext = ViewModel = new FormRenderViewModel(page, rowId);
                FormScrollView.Content = ViewModel.XView;

                SaveButton.Text = "Save Changes";
                SaveButton.IsVisible = false;
                EditButton.IsVisible = true;
            }
            catch (Exception ex)
            {
                Log.Write("FormRender edit/prefill mode" + ex.Message);
            }
        }

        //prefill mode
        public FormRender(EbMobilePage page, EbDataRow currentRow)
        {
            InitializeComponent();
            try
            {
                BindingContext = ViewModel = new FormRenderViewModel(page, currentRow);
                FormScrollView.Content = ViewModel.XView;
            }
            catch (Exception ex)
            {
                Log.Write("FormRender edit/prefill mode" + ex.Message);
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

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            LoaderIconed.IsVisible = true;
            if (!IsRendered)
            {
                if(ViewModel.Mode == FormMode.EDIT)
                {
                    await ViewModel.SetDataOnEdit();
                    ViewModel.FillControlsValues();
                }
                IsRendered = true;
            }
            LoaderIconed.IsVisible = false;
        }

        private void EditButton_Clicked(object sender, EventArgs e)
        {
            EditButton.IsVisible = false;
            SaveButton.IsVisible = true;

            foreach (var pair in ViewModel.Form.ControlDictionary)
                pair.Value.SetAsReadOnly(false);
        }

        public void ShowFullScreenImage(object tapedImage)
        {
            if (tapedImage != null)
            {
                ImageFullScreen.Source = (tapedImage as Image).Source;
                ImageFullScreen.Show();
            }
        }
    }
}