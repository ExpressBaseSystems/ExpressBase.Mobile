using ExpressBase.Mobile.CustomControls;
using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.ViewModels.Dynamic;
using System;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ExpressBase.Mobile.Views.Dynamic
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LinkedListRender : ContentPage
    {
        private int Offset = 0;

        private int PageCount = 1;

        public LinkedListViewModel ViewModel { set; get; }

        public LinkedListRender(EbMobilePage LinkPage, EbMobileVisualization SourceVis, CustomFrame CustFrame)
        {
            InitializeComponent();
            try
            {
                BindingContext = ViewModel = new LinkedListViewModel(LinkPage, SourceVis, CustFrame);

                HeaderContainer.Children.Add(ViewModel.HeaderFrame);
                Grid.SetRow(ViewModel.HeaderFrame, 0);
                Loader.IsVisible = true;
                ToggleLinks();
            }
            catch (Exception ex)
            {
                Log.Write("LinkedListViewRender.Constructor---" + ex.Message);
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (ViewModel.DataTable == null)
            {
                await ViewModel.SetData();
                this.AppendListItems();
            }
        }

        private void AppendListItems()
        {
            try
            {
                TapGestureRecognizer tap = new TapGestureRecognizer { NumberOfTapsRequired = 1 };
                tap.Tapped += Tap_Tapped;
                bool IsClickable = !string.IsNullOrEmpty(ViewModel.Visualization.LinkRefId);
                int rowColCount = 1;

                if (ViewModel.DataTable.Rows.Any())
                {
                    foreach (EbDataRow row in ViewModel.DataTable.Rows)
                    {
                        CustomFrame CustFrame = new CustomFrame(row, ViewModel.Visualization);
                        CustFrame.SetBackGroundColor(rowColCount);

                        if (ViewModel.NetworkType == NetworkMode.Offline)
                            CustFrame.ShowSyncFlag(ViewModel.DataTable.Columns);

                        if (IsClickable) CustFrame.GestureRecognizers.Add(tap);
                        ListContainer.Children.Add(CustFrame);

                        rowColCount++;
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
                Loader.IsVisible = false;
            }
            catch (Exception ex)
            {
                Log.Write(ex.Message);
            }
        }

        private async void Tap_Tapped(object sender, EventArgs e)
        {
            IToast toast = DependencyService.Get<IToast>();
            try
            {
                CustomFrame customFrame = (CustomFrame)sender;
                EbMobilePage page = HelperFunctions.GetPage(ViewModel.Visualization.LinkRefId);

                if (ViewModel.NetworkType != page.NetworkMode)
                {
                    toast.Show("Link page Mode is different.");
                    return;
                }
                ContentPage renderer = await ViewModel.GetPageByContainer(customFrame, page);
                if (renderer != null)
                    await (Application.Current.MainPage as MasterDetailPage).Detail.Navigation.PushAsync(renderer);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        void ToggleLinks()
        {
            try
            {
                if (string.IsNullOrEmpty(ViewModel.SourceVisualization.SourceFormRefId))
                {
                    SourceDataEdit.IsVisible = false;
                }

                if (string.IsNullOrEmpty(ViewModel.Visualization.LinkRefId))
                {
                    AddLinkData.IsVisible = false;
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex.Message);
            }
        }

        public void PagingPrevButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                if (ViewModel != null)
                {
                    if (Offset <= 0) return;

                    Offset -= ViewModel.Visualization.PageLength;
                    PageCount--;
                    this.RefreshListView();
                }
            }
            catch (Exception ex)
            {
                Log.Write("ListViewRender.PrevButton_Clicked---" + ex.Message);
            }
        }

        public void PagingNextButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                if (ViewModel != null)
                {
                    if (Offset + ViewModel.Visualization.PageLength >= ViewModel.DataCount) return;

                    Offset += ViewModel.Visualization.PageLength;
                    PageCount++;
                    this.RefreshListView();
                }
            }
            catch (Exception ex)
            {
                Log.Write("ListViewRender.NextButton_Clicked---" + ex.Message);
            }
        }

        private void UpdatePaginationBar()
        {
            try
            {
                int pageLength = ViewModel.Visualization.PageLength;
                int totalEntries = ViewModel.DataCount;
                int offset = this.Offset + 1;
                int length = pageLength + offset - 1;

                if (totalEntries < pageLength)
                    length = totalEntries;

                if (pageLength + offset > totalEntries)
                    length = totalEntries;

                PagingMeta.Text = $"{offset}-{length}/{totalEntries}";
                int totalpages = (int)Math.Ceiling((double)totalEntries / pageLength);
                PagingPageCount.Text = $"{PageCount}/{totalpages}";
            }
            catch (Exception ex)
            {
                Log.Write(ex.Message);
            }
        }

        private void RefreshListView()
        {
            try
            {
                this.UpdatePaginationBar();
            }
            catch (Exception ex)
            {
                Log.Write(ex.Message);
            }
        }

        private void ListViewRefresh_Refreshing(object sender, EventArgs e)
        {
            IToast toast = DependencyService.Get<IToast>();
            try
            {
                ListViewRefresh.IsRefreshing = true;
                this.Offset = 0;
                this.RefreshListView();
                ListViewRefresh.IsRefreshing = false;
                toast.Show("Refreshed");
            }
            catch (Exception ex)
            {
                ListViewRefresh.IsRefreshing = false;
                toast.Show("Something went wrong. Please try again");
                Log.Write(ex.Message);
            }
        }
    }
}