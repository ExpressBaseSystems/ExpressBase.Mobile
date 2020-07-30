using ExpressBase.Mobile.ViewModels.Dynamic;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using ExpressBase.Mobile.CustomControls;
using ExpressBase.Mobile.Data;
using System.Linq;
using ExpressBase.Mobile.Helpers;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace ExpressBase.Mobile.Views.Dynamic
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ListRender : ContentPage
    {
        private int offset = 0;

        private int pageCount = 1;

        private bool isRendered;

        private readonly ListViewModel viewModel;

        private readonly TapGestureRecognizer tapGesture;

        private bool HasLink => !string.IsNullOrEmpty(viewModel.Visualization.LinkRefId);

        public ListRender(EbMobilePage Page)
        {
            InitializeComponent();

            BindingContext = viewModel = new ListViewModel(Page);
            viewModel.BindMethod(BindableMethod);

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
                this.AppendListItems();
            }
            isRendered = true;
            this.Loader.IsVisible = false;
        }

        private void ToggleLinks()
        {
            if (this.HasLink && viewModel.Visualization.ShowNewButton)
            {
                EbMobilePage page = HelperFunctions.GetPage(viewModel.Visualization.LinkRefId);

                if (page != null && page.Container is EbMobileForm)
                    AddLinkData.IsVisible = true;
            }
        }

        private void AppendListItems()
        {
            int counter = 1;
            this.ListContainer.Children.Clear();

            if (viewModel.DataTable.Rows.Any())
            {
                EmptyMessage.IsVisible = false;
                PagingContainer.IsVisible = true;

                try
                {
                    foreach (EbDataRow row in viewModel.DataTable.Rows)
                    {
                        DynamicFrame li = new DynamicFrame(row, viewModel.Visualization, false);

                        if (viewModel.Visualization.Style == RenderStyle.Flat)
                        {
                            li.SetBackGroundColor(counter);
                        }

                        if (viewModel.NetworkType == NetworkMode.Offline)
                            li.ShowSyncFlag(viewModel.DataTable.Columns);

                        if (this.HasLink) li.GestureRecognizers.Add(tapGesture);

                        this.ListContainer.Children.Add(li);
                        counter++;
                    }
                }
                catch(Exception ex)
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
                else
                {
                    ContentPage renderer = viewModel.GetPageByContainer(customFrame, page);

                    if (renderer != null)
                        await (Application.Current.MainPage as MasterDetailPage).Detail.Navigation.PushAsync(renderer);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void FilterButton_Clicked(object sender, EventArgs e)
        {
            this.FilterView.Show();
        }

        private async void PagingPrevButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                if (this.offset <= 0)
                    return;
                else
                {
                    this.offset -= viewModel.Visualization.PageLength;
                    this.pageCount--;
                    await this.RefreshListView(false);
                }
            }
            catch (Exception ex)
            {
                EbLog.Write("ListViewRender.PrevButton_Clicked---" + ex.Message);
            }
        }

        private async void PagingNextButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                if (this.offset + viewModel.Visualization.PageLength >= viewModel.DataCount)
                    return;
                else
                {
                    this.offset += viewModel.Visualization.PageLength;
                    this.pageCount++;
                    await this.RefreshListView(false);
                }
            }
            catch (Exception ex)
            {
                EbLog.Write("ListViewRender.NextButton_Clicked---" + ex.Message);
            }
        }

        private async Task RefreshListView(bool toInitial)
        {
            this.Loader.IsVisible = true;
            if (toInitial)
            {
                FilterView.ClearFilter();
                await viewModel.SetData(this.offset);
            }
            else
            {
                List<DbParameter> parameters = this.FilterView.GetFilterValues();
                await viewModel.Refresh(parameters, offset);
            }
            this.AppendListItems();
            this.Loader.IsVisible = false;
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

        private async void ListViewRefresh_Refreshing(object sender, EventArgs e)
        {
            this.ListViewRefresh.IsRefreshing = false;
            IToast toast = DependencyService.Get<IToast>();
            try
            {
                this.offset = 0;
                this.pageCount = 1;
                await this.RefreshListView(true);
                toast.Show("Refreshed");
            }
            catch (Exception ex)
            {
                EbLog.Write(ex.Message);
            }
        }

        public async void BindableMethod()
        {
            try
            {
                this.Loader.IsVisible = true;
                var parameters = this.FilterView.GetFilterValues();
                await viewModel.Refresh(parameters);
                this.AppendListItems();
                this.Loader.IsVisible = false;
            }
            catch (Exception ex)
            {
                EbLog.Write(ex.Message);
            }
        }
    }
}