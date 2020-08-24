using ExpressBase.Mobile.CustomControls;
using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.ViewModels.Dynamic;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ExpressBase.Mobile.Views.Dynamic
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LinkedListRender : ContentPage, IRefreshable
    {
        private int pageCount = 1;

        private bool isRendered;

        private readonly LinkedListViewModel viewModel;

        private bool HasSourceLink => viewModel.Visualization.HasSourceFormLink();

        private bool HasLink => viewModel.Visualization.HasLink();

        public LinkedListRender(EbMobilePage page, EbMobileVisualization context, EbDataRow row)
        {
            InitializeComponent();
            BindingContext = viewModel = new LinkedListViewModel(page, context, row);

            this.DrawContextHeader(row, context);
            this.Loader.IsVisible = true;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (!isRendered)
            {
                await viewModel.InitializeAsync();
                this.ToggleLinks();
                this.UpdatePaginationBar();
            }
            isRendered = true;
            this.Loader.IsVisible = false;
        }

        protected override bool OnBackButtonPressed()
        {
            if (FilterView.IsVisible)
            {
                FilterView.IsVisible = false;
                return true;
            }
            else
            {
                base.OnBackButtonPressed();
                return false;
            }
        }

        private void DrawContextHeader(EbDataRow row, EbMobileVisualization context)
        {
            var header = new DynamicFrame(row, context, true)
            {
                BackgroundColor = Color.Transparent,
                Padding = new Thickness(20, 10, 20, 0),
                Margin = 0
            };

            HeaderContainer.Children.Add(header);
        }

        private void ToggleLinks()
        {
            SourceDataEdit.IsVisible = this.HasSourceLink;

            if (this.HasLink && viewModel.Visualization.ShowNewButton)
            {
                EbMobilePage page = HelperFunctions.GetPage(viewModel.Visualization.LinkRefId);

                if (page != null && page.Container is EbMobileForm)
                    AddLinkData.IsVisible = true;
            }

            ToggleDataLength();
        }

        private void ToggleDataLength()
        {
            bool isEmpty = viewModel.DataCount <= 0;

            PagingContainer.IsVisible = !isEmpty;
            EmptyMessage.IsVisible = isEmpty;
        }

        private void FilterButton_Clicked(object sender, EventArgs e)
        {
            this.FilterView.Show();
        }

        public async void PagingPrevButton_Clicked(object sender, EventArgs e)
        {
            if (viewModel.Offset <= 0) return;

            viewModel.Offset -= viewModel.Visualization.PageLength;
            pageCount--;
            await viewModel.RefreshDataAsync();
        }

        public async void PagingNextButton_Clicked(object sender, EventArgs e)
        {
            if (viewModel.Offset + viewModel.Visualization.PageLength >= viewModel.DataCount) return;

            viewModel.Offset += viewModel.Visualization.PageLength;
            pageCount++;
            await viewModel.RefreshDataAsync();
        }

        private void UpdatePaginationBar()
        {
            try
            {
                int pageLength = viewModel.Visualization.PageLength;
                int totalEntries = viewModel.DataCount;
                int _offset = viewModel.Offset + 1;
                int length = pageLength + viewModel.Offset - 1;

                if (totalEntries < pageLength || pageLength + _offset > totalEntries)
                    length = totalEntries;

                this.PagingMeta.Text = $"{_offset} - {length}/{totalEntries}";
                this.PagingPageCount.Text = $"{pageCount}/{(int)Math.Ceiling((double)totalEntries / pageLength)}";
            }
            catch (Exception ex)
            {
                EbLog.Error(ex.Message);
            }
        }

        private void FullScreenButton_Clicked(object sender, EventArgs e)
        {
            HeaderView.IsVisible = !HeaderView.IsVisible;
        }

        public void Refreshed()
        {
            this.UpdatePaginationBar();
            this.ToggleDataLength();
        }

        public void UpdateRenderStatus()
        {
            isRendered = false;
        }

        public bool CanRefresh()
        {
            return true;
        }
    }
}