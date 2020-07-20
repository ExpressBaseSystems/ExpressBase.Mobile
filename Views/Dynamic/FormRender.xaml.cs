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

        private readonly FormRenderViewModel viewModel;

        //new mode
        public FormRender(EbMobilePage page)
        {
            InitializeComponent();
            BindingContext = viewModel = new FormRenderViewModel(page);
        }

        //edit
        public FormRender(EbMobilePage page, int rowId)
        {
            InitializeComponent();
            BindingContext = viewModel = new FormRenderViewModel(page, rowId);

            SaveButton.Text = "Save Changes";
            SaveButton.IsVisible = false;
            EditButton.IsVisible = true;
        }

        //prefill mode
        public FormRender(EbMobilePage page, EbMobileVisualization context, EbDataRow contextRow)
        {
            InitializeComponent();
            BindingContext = viewModel = new FormRenderViewModel(page, context, contextRow);
        }

        //reference mode
        public FormRender(EbMobilePage page, EbMobileVisualization context, EbDataRow contextRow, int unused)
        {
            InitializeComponent();
            BindingContext = viewModel = new FormRenderViewModel(page, context, contextRow, unused);
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            LoaderIconed.IsVisible = true;
            if (!isRendered)
            {
                await viewModel.FillValues();
                isRendered = true;
            }
            LoaderIconed.IsVisible = false;
        }

        private void EditButton_Clicked(object sender, EventArgs e)
        {
            EditButton.IsVisible = false;
            SaveButton.IsVisible = true;

            foreach (var pair in viewModel.Form.ControlDictionary)
            {
                pair.Value.SetAsReadOnly(false);
            }
        }

        public void ShowFullScreenImage(Image tapedImage)
        {
            if (tapedImage != null)
            {
                ImageFullScreen.Source = tapedImage.Source;
                ImageFullScreen.Show();
            }
        }
    }
}