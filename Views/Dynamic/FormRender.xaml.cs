using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.ViewModels.Dynamic;
using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ExpressBase.Mobile.Views.Dynamic
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class FormRender : ContentPage
    {
        private bool isRendered;

        private readonly FormRenderViewModel viewModel;

        #region constructor @overloads

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
            BindingContext = viewModel = new FormRenderVME(page, rowId);

            SaveButton.Text = "Save Changes";
            SaveButton.IsVisible = false;
            EditButton.IsVisible = true;
        }

        //prefill/new mode 
        public FormRender(EbMobilePage page, List<EbMobileDataColToControlMap> linkMap, EbDataRow contextRow)
        {
            InitializeComponent();
            BindingContext = viewModel = new FormRenderVMPRE(page, linkMap, contextRow);
        }

        //reference mode
        public FormRender(EbMobilePage page, EbMobileVisualization context, EbDataRow contextRow)
        {
            InitializeComponent();
            BindingContext = viewModel = new FormRenderVMR(page, context, contextRow);
        }

        #endregion

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            EbLayout.ShowLoader();
            if (!isRendered)
            {
                await viewModel.InitializeAsync();
                isRendered = true;
            }
            EbLayout.HideLoader();
        }

        private void EditButton_Clicked(object sender, EventArgs e)
        {
            EditButton.IsVisible = false;
            SaveButton.IsVisible = true;

            viewModel.EnableControls();
        }

        public void ShowFullScreenImage(Image tapedImage)
        {
            if (tapedImage != null)
            {
                FullScreenImage.Source = tapedImage.Source;
                ImageFullScreen.IsVisible = true;
            }
        }

        private void FullScreenClose_Clicked(object sender, EventArgs e)
        {
            ImageFullScreen.IsVisible = false;
        }
    }
}