using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
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

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            EbLayout.ShowLoader();
            viewModel.MsgLoader = EbLayout.GetMessageLoader();
            viewModel.MsgLoader.Message = "Saving record...";
            if (!isRendered)
            {
                await viewModel.InitializeAsync();
                AdjustButtonContainer();

                if (!viewModel.HasWebFormRef && viewModel.IsOnline())
                {
                    EbLog.Info($"Webform refid not configued for form '{viewModel.Page.Name}'");
                    SaveButton.IsEnabled = false;
                }
                isRendered = true;
            }
            EbLayout.HideLoader();
        }

        private void AdjustButtonContainer()
        {
            if (viewModel.Form.PrintDocs?.Count > 0)
            {
                if (viewModel.Form.RenderAsFilterDialog)
                {
                    ButtonGrid.ColumnDefinitions[1].Width = new GridLength(1, GridUnitType.Star);
                    (ButtonGrid.Children[0] as Button).IsVisible = false;
                    (ButtonGrid.Children[1] as Button).IsVisible = true;
                    viewModel.MsgLoader.Message = "Loading...";
                }
                else
                {
                    ButtonGrid.ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Star);
                    ButtonGrid.ColumnDefinitions[1].Width = new GridLength(1, GridUnitType.Star);
                    (ButtonGrid.Children[1] as Button).IsVisible = true;
                }
            }

            if (viewModel.NetworkType == NetworkMode.Offline)
            {
                LastSyncInfo syncInfo = Store.GetJSON<LastSyncInfo>(AppConst.LAST_SYNC_INFO);
                if (syncInfo == null || !syncInfo.PullSuccess)
                {
                    (ButtonGrid.Children[0] as Button).IsVisible = false;
                    (ButtonGrid.Children[1] as Button).IsVisible = false;
                    SaveButton.IsEnabled = false;
                    App.Navigation.PopByRenderer(true);
                    Utils.Toast("Sync data required");
                }
            }
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