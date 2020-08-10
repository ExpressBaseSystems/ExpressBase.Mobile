using ExpressBase.Mobile.CustomControls;
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

        public LinkedListRender(EbMobilePage page, EbMobileVisualization context, DynamicFrame sender)
        {
            InitializeComponent();
            BindingContext = viewModel = new LinkedListViewModel(page, context, sender);

            this.DrawContextHeader(sender, context);
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

        private void DrawContextHeader(DynamicFrame sender, EbMobileVisualization context)
        {
            var header = new DynamicFrame(sender.DataRow, context, true)
            {
                BackgroundColor = Color.Transparent,
                Padding = new Thickness(20, 10, 20, 0),
                Margin = 0
            };

            HeaderContainer.Children.Add(header);
        }

        private void ToggleLinks()
        {
            if (this.HasSourceLink) 
                SourceDataEdit.IsVisible = true;

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
            if (viewModel.DataCount <= 0)
            {
                PagingContainer.IsVisible = false;
                EmptyMessage.IsVisible = true;
            }
            else
            {
                PagingContainer.IsVisible = true;
                EmptyMessage.IsVisible = false;
            }
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
                EbLog.Write(ex.Message);
            }
        }

        private void FullScreenButton_Clicked(object sender, EventArgs e)
        {
            bool flag = HeaderView.IsVisible;
            if (flag)
                HeaderView.IsVisible = false;
            else
                HeaderView.IsVisible = true;
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
    }
}