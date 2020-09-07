using ExpressBase.Mobile.ViewModels.Dynamic;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.CustomControls;
using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Views.Base;

namespace ExpressBase.Mobile.Views.Dynamic
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ListRender : ListViewBasePage
    {
        public ListRender(EbMobilePage Page)
        {
            InitializeComponent();
            BindingContext = ViewModel = new ListViewModel(Page);
            this.Loader.IsVisible = true;
        }

        public ListRender(EbMobilePage Page, EbDataRow row)
        {
            InitializeComponent();
            BindingContext = ViewModel = new ListViewModel(Page, row);
            this.Loader.IsVisible = true;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (!IsRendered)
            {
                await ViewModel.InitializeAsync();
                this.ToggleLinks();
                this.UpdatePaginationBar();
            }
            IsRendered = true;
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

        private void ToggleLinks()
        {
            if (this.HasLink && ViewModel.Visualization.ShowNewButton)
            {
                EbMobilePage page = EbPageFinder.GetPage(ViewModel.Visualization.LinkRefId);

                if (page != null && page.Container is EbMobileForm)
                    AddLinkData.IsVisible = true;
            }

            this.ToggleDataLength();
        }

        protected override void ToggleDataLength()
        {
            int count = ViewModel.DataCount;
            int length = ViewModel.Visualization.PageLength;

            if (count <= 0 || (count <= length && this.PageCount <= 1))
                PagingContainer.IsVisible = false;
            else
                PagingContainer.IsVisible = true;

            EmptyMessage.IsVisible = count <= 0;
        }

        private void FilterButton_Clicked(object sender, EventArgs e)
        {
            this.FilterView.Show();
        }

        protected override void UpdatePaginationBar()
        {
            PagingMeta meta = base.GetPagingMeta();

            if(meta != null)
            {
                this.PagingMeta.Text = meta.Meta;
                this.PagingPageCount.Text = meta.PageCount;
            }
        }

        private void SearchButton_Clicked(object sender, EventArgs e)
        {
            SearchButton.IsVisible = false;
            SearchBox.IsVisible = true;
            SearchBox.Focus();
        }

        private async void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string search = SearchBox.Text;

            if (search == null || search.Length == 0)
            {
                SearchButton.IsVisible = true;
                SearchBox.IsVisible = false;
                await ViewModel.RefreshDataAsync();
            }
        }
    }
}