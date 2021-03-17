using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.ViewModels.Dynamic;
using ExpressBase.Mobile.Views.Base;
using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ExpressBase.Mobile.Views.Dynamic
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class FormRender : ContentPage, IFormRenderer
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
        }

        //edit
        public FormRender(EbMobilePage page, int rowId, WebformData data)
        {
            InitializeComponent();
            BindingContext = viewModel = new FormRenderVME(page, rowId, data);
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

                if (!viewModel.HasWebFormRef && viewModel.IsOnline())
                {
                    EbLog.Info($"Webform refid not configued for form '{viewModel.Page.Name}'");
                    SaveButton.IsEnabled = false;
                }
                isRendered = true;
            }
            EbLayout.HideLoader();
        }

        private void OnEditButtonClicked(object sender, EventArgs e)
        {
            viewModel.IsEditButtonVisible = false;
            viewModel.IsSaveButtonVisible = true;
            EbFormHelper.SwitchViewToEdit();
        }

        public void ShowFullScreenImage(ImageSource source)
        {
            if (source != null)
            {
                ImageFullScreen.SetSource(source).Show();
            }
        }
    }
}