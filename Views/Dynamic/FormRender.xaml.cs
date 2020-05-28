using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.ViewModels.Dynamic;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ExpressBase.Mobile.Views.Dynamic
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class FormRender : ContentPage
    {
        private bool isRendered;

        public FormRenderViewModel ViewModel { set; get; }

        //new mode
        public FormRender(EbMobilePage page)
        {
            InitializeComponent();
            BindingContext = ViewModel = new FormRenderViewModel(page);
        }

        //edit
        public FormRender(EbMobilePage page, int rowId)
        {
            InitializeComponent();
            BindingContext = ViewModel = new FormRenderViewModel(page, rowId);

            SaveButton.Text = "Save Changes";
            SaveButton.IsVisible = false;
            EditButton.IsVisible = true;
        }

        //prefill mode
        public FormRender(EbMobilePage page, EbDataRow currentRow)
        {
            InitializeComponent();
            BindingContext = ViewModel = new FormRenderViewModel(page, currentRow);
        }

        //reference mode
        public FormRender(EbMobilePage currentForm, EbMobilePage parentForm, int parentId)
        {
            InitializeComponent();
            BindingContext = ViewModel = new FormRenderViewModel(currentForm, parentForm, parentId);
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            LoaderIconed.IsVisible = true;
            if (!isRendered)
            {
                if (ViewModel.Mode == FormMode.EDIT)
                {
                    await ViewModel.SetDataOnEdit();
                    ViewModel.FillControlsValues();
                }
                else if(ViewModel.Mode == FormMode.PREFILL)
                {
                    ViewModel.FillControlsFlat();
                }
                isRendered = true;
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