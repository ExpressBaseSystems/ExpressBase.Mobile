using ExpressBase.Mobile.ViewModels.Dynamic;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Collections.Generic;
using ExpressBase.Mobile.CustomControls;
using ExpressBase.Mobile.Data;
using System.Linq;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Enums;
using System.Threading.Tasks;

namespace ExpressBase.Mobile.Views.Dynamic
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ListRender : ContentPage
    {
        private int Offset = 0;

        private int PageCount = 1;

        public ListViewModel ViewModel { set; get; }

        private TapGestureRecognizer TapGesture { set; get; }

        public ListRender(EbMobilePage Page)
        {
            InitializeComponent();

            BindingContext = ViewModel = new ListViewModel(Page);
            this.ViewModel.BindMethod(BindableMethod);

            this.TapGesture = new TapGestureRecognizer { NumberOfTapsRequired = 1 };
            this.TapGesture.Tapped += Tap_Tapped;
            this.Loader.IsVisible = true;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (this.ViewModel.DataTable == null)
            {
                await this.ViewModel.SetData();
                this.AppendListItems();
            }
            this.Loader.IsVisible = false;
        }

        private void AppendListItems()
        {
            try
            {
                bool flag = !string.IsNullOrEmpty(this.ViewModel.Visualization.LinkRefId);
                int counter = 1;

                this.ListContainer.Children.Clear();

                if (this.ViewModel.DataTable.Rows.Any())
                {
                    foreach (EbDataRow row in this.ViewModel.DataTable.Rows)
                    {
                        CustomFrame customFrame = new CustomFrame(row, this.ViewModel.Visualization, false);
                        customFrame.SetBackGroundColor(counter);

                        if (this.ViewModel.NetworkType == NetworkMode.Offline)
                            customFrame.ShowSyncFlag(this.ViewModel.DataTable.Columns);

                        if (flag) customFrame.GestureRecognizers.Add(this.TapGesture);

                        this.ListContainer.Children.Add(customFrame);
                        counter++;
                    }
                }
                else
                {
                    this.ListContainer.Children.Add(new Label
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

        private async void Tap_Tapped(object sender, EventArgs e)
        {
            IToast toast = DependencyService.Get<IToast>();
            try
            {
                CustomFrame customFrame = (CustomFrame)sender;
                EbMobilePage page = HelperFunctions.GetPage(this.ViewModel.Visualization.LinkRefId);
                if (this.ViewModel.NetworkType != page.NetworkMode)
                {
                    toast.Show("Link page Mode is different.");
                    return;
                }
                else
                {
                    ContentPage renderer = await this.ViewModel.GetPageByContainer(customFrame, page);
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
                if (this.Offset <= 0)
                    return;
                else
                {
                    this.Offset -= this.ViewModel.Visualization.PageLength;
                    this.PageCount--;
                    await this.RefreshListView();
                }
            }
            catch (Exception ex)
            {
                Log.Write("ListViewRender.PrevButton_Clicked---" + ex.Message);
            }
        }

        private async void PagingNextButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                if (this.Offset + this.ViewModel.Visualization.PageLength >= this.ViewModel.DataCount)
                    return;
                else
                {
                    this.Offset += this.ViewModel.Visualization.PageLength;
                    this.PageCount++;
                    await this.RefreshListView();
                }
            }
            catch (Exception ex)
            {
                Log.Write("ListViewRender.NextButton_Clicked---" + ex.Message);
            }
        }

        private async Task RefreshListView()
        {
            await this.ViewModel.SetData(0);
            this.AppendListItems();
        }

        private void UpdatePaginationBar()
        {
            try
            {
                int pageLength = this.ViewModel.Visualization.PageLength;
                int totalEntries = this.ViewModel.DataCount;
                int offset = this.Offset + 1;
                int length = pageLength + offset - 1;

                if (totalEntries < pageLength || pageLength + offset > totalEntries)
                    length = totalEntries;

                this.PagingMeta.Text = $"{offset} - {length}/{totalEntries}";
                this.PagingPageCount.Text = $"{PageCount}/{(int)Math.Ceiling((double)totalEntries / pageLength)}";
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
                this.ListViewRefresh.IsRefreshing = true;
                this.Offset = 0;
                await this.RefreshListView();
                this.ListViewRefresh.IsRefreshing = false;
                toast.Show("Refreshed");
            }
            catch (Exception ex)
            {
                this.ListViewRefresh.IsRefreshing = false;
                Log.Write(ex.Message);
            }
        }

        public async void BindableMethod()
        {
            try
            {
                this.Loader.IsVisible = true;
                var parameters = this.FilterView.GetFilterValues();
                await this.ViewModel.Refresh(parameters);
                this.AppendListItems();
                this.Loader.IsVisible = false;
            }
            catch (Exception ex)
            {
                Log.Write(ex.Message);
            }
        }
    }
}