using ExpressBase.Mobile.CustomControls;
using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.ViewModels.Dynamic;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ExpressBase.Mobile.Views.Dynamic
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LinkedListRender : ContentPage
    {
        private int offset = 0;

        private int pageCount = 1;

        private bool isRendered;

        private readonly LinkedListViewModel viewModel;

        private readonly TapGestureRecognizer tapGesture;

        private bool HasSourceLink => !string.IsNullOrEmpty(viewModel.SourceVisualization.SourceFormRefId);

        private bool HasLink => !string.IsNullOrEmpty(viewModel.Visualization.LinkRefId);

        public LinkedListRender(EbMobilePage page, EbMobileVisualization sourcevis, DynamicFrame linkframe)
        {
            InitializeComponent();
            BindingContext = viewModel = new LinkedListViewModel(page, sourcevis, linkframe);

            tapGesture = new TapGestureRecognizer { NumberOfTapsRequired = 1 };
            tapGesture.Tapped += ListItem_Tapped;
            this.ToggleLinks();
            this.Loader.IsVisible = true;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (!isRendered)
            {
                await viewModel.InitializeAsync();
                HeaderContainer.Children.Add(viewModel.HeaderFrame);
                this.AppendListItems();
            }
            isRendered = true;
            this.Loader.IsVisible = false;
        }

        private void AppendListItems()
        {
            int count = 1;
            this.ListContainer.Children.Clear();

            if (viewModel.DataTable.Rows.Any())
            {
                PagingContainer.IsVisible = true;
                EmptyMessage.IsVisible = false;

                try
                {
                    foreach (EbDataRow row in viewModel.DataTable.Rows)
                    {
                        DynamicFrame CustFrame = new DynamicFrame(row, viewModel.Visualization);

                        if (viewModel.Visualization.Style == RenderStyle.Flat)
                        {
                            CustFrame.SetBackGroundColor(count);
                        }

                        if (viewModel.NetworkType == NetworkMode.Offline)
                            CustFrame.ShowSyncFlag(viewModel.DataTable.Columns);

                        if (this.HasLink) CustFrame.GestureRecognizers.Add(tapGesture);

                        ListContainer.Children.Add(CustFrame);

                        count++;
                    }
                }
                catch (Exception ex)
                {
                    EbLog.Write("list item rendering :: " + ex.Message);
                }
            }
            else
            {
                PagingContainer.IsVisible = false;
                EmptyMessage.IsVisible = true;
            }
            this.UpdatePaginationBar();
        }

        private async void ListItem_Tapped(object sender, EventArgs e)
        {
            IToast toast = DependencyService.Get<IToast>();
            try
            {
                DynamicFrame customFrame = (DynamicFrame)sender;

                EbMobilePage page = HelperFunctions.GetPage(viewModel.Visualization.LinkRefId);

                if (viewModel.NetworkType != page.NetworkMode)
                {
                    toast.Show("Link page Mode is different.");
                    return;
                }
                ContentPage renderer = viewModel.GetPageByContainer(customFrame, page);

                if (renderer != null)
                    await (Application.Current.MainPage as MasterDetailPage).Detail.Navigation.PushAsync(renderer);
            }
            catch (Exception ex)
            {
                EbLog.Write(ex.Message);
            }
        }

        private void ToggleLinks()
        {
            if (this.HasSourceLink) SourceDataEdit.IsVisible = true;

            if (this.HasLink && viewModel.Visualization.ShowNewButton)
            {
                EbMobilePage page = HelperFunctions.GetPage(viewModel.Visualization.LinkRefId);

                if (page != null && page.Container is EbMobileForm)
                    AddLinkData.IsVisible = true;
            }
        }

        public async void PagingPrevButton_Clicked(object sender, EventArgs e)
        {
            if (offset <= 0) return;

            offset -= viewModel.Visualization.PageLength;
            pageCount--;
            await this.RefreshListView();
        }

        public async void PagingNextButton_Clicked(object sender, EventArgs e)
        {
            if (offset + viewModel.Visualization.PageLength >= viewModel.DataCount) return;

            offset += viewModel.Visualization.PageLength;
            pageCount++;
            await this.RefreshListView();
        }

        private void UpdatePaginationBar()
        {
            try
            {
                int pageLength = viewModel.Visualization.PageLength;
                int totalEntries = viewModel.DataCount;
                int _offset = offset + 1;
                int length = pageLength + offset - 1;

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

        private async Task RefreshListView()
        {
            try
            {
                this.Loader.IsVisible = true;
                await viewModel.SetData(this.offset);
                this.AppendListItems();
                this.Loader.IsVisible = false;
            }
            catch (Exception ex)
            {
                EbLog.Write(ex.Message);
            }
        }

        private async void ListViewRefresh_Refreshing(object sender, EventArgs e)
        {
            ListViewRefresh.IsRefreshing = false;
            IToast toast = DependencyService.Get<IToast>();
            try
            {
                this.offset = 0;
                this.pageCount = 1;
                await this.RefreshListView();
                toast.Show("Refreshed");
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
    }
}