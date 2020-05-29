using ExpressBase.Mobile.CustomControls;
using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
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

        public LinkedListRender(EbMobilePage page, EbMobileVisualization sourcevis, CustomFrame linkframe)
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
            try
            {
                bool flag = !string.IsNullOrEmpty(viewModel.Visualization.LinkRefId);
                int count = 1;

                if (viewModel.DataTable.Rows.Any())
                {
                    foreach (EbDataRow row in viewModel.DataTable.Rows)
                    {
                        CustomFrame CustFrame = new CustomFrame(row, viewModel.Visualization);
                        CustFrame.SetBackGroundColor(count);

                        if (viewModel.NetworkType == NetworkMode.Offline)
                            CustFrame.ShowSyncFlag(viewModel.DataTable.Columns);

                        if (flag) CustFrame.GestureRecognizers.Add(tapGesture);

                        ListContainer.Children.Add(CustFrame);

                        count++;
                    }
                }
                else
                {
                    ListContainer.Children.Add(new Label
                    {
                        Text = "Empty list",
                        VerticalOptions = LayoutOptions.CenterAndExpand,
                        HorizontalTextAlignment = TextAlignment.Center
                    });
                }
                this.UpdatePaginationBar();
            }
            catch (Exception ex)
            {
                Log.Write(ex.Message);
            }
        }

        private async void ListItem_Tapped(object sender, EventArgs e)
        {
            IToast toast = DependencyService.Get<IToast>();
            try
            {
                CustomFrame customFrame = (CustomFrame)sender;

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
                Console.WriteLine(ex.Message);
            }
        }

        private void ToggleLinks()
        {
            try
            {
                if (string.IsNullOrEmpty(viewModel.SourceVisualization.SourceFormRefId))
                {
                    SourceDataEdit.IsVisible = false;
                }

                if (string.IsNullOrEmpty(viewModel.Visualization.LinkRefId))
                {
                    AddLinkData.IsVisible = false;
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex.Message);
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
                Log.Write(ex.Message);
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
                Log.Write(ex.Message);
            }
        }

        private async void ListViewRefresh_Refreshing(object sender, EventArgs e)
        {
            IToast toast = DependencyService.Get<IToast>();
            try
            {
                ListViewRefresh.IsRefreshing = true;
                this.offset = 0;
                await this.RefreshListView();
                ListViewRefresh.IsRefreshing = false;
                toast.Show("Refreshed");
            }
            catch (Exception ex)
            {
                ListViewRefresh.IsRefreshing = false;
                Log.Write(ex.Message);
            }
        }
    }
}