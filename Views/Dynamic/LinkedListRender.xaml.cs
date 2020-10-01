using ExpressBase.Mobile.CustomControls;
using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.ViewModels.Dynamic;
using ExpressBase.Mobile.Views.Base;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ExpressBase.Mobile.Views.Dynamic
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LinkedListRender : ListViewBasePage
    {
        private bool HasSourceLink => ViewModel.Visualization.HasSourceFormLink();

        public LinkedListRender(EbMobilePage page, EbMobileVisualization context, EbDataRow row)
        {
            InitializeComponent();
            BindingContext = ViewModel = new LinkedListViewModel(page, context, row);

            this.DrawContextHeader(row, context);
            EbLayout.ShowLoader();
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
            EbLayout.HideLoader();
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

            if (this.HasLink && ViewModel.Visualization.ShowNewButton)
            {
                EbMobilePage page = EbPageFinder.GetPage(ViewModel.Visualization.LinkRefId);

                if (page != null && page.Container is EbMobileForm)
                {
                    if (this.HasLinkText)
                    {
                        FormLinkTextLabel.Text = ViewModel.Visualization.NewButtonText;
                        FormLinkWText.IsVisible = true;
                    }
                    else
                    {
                        FormLinkWOText.IsVisible = true;
                    }
                }

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
            this.FilterButton.IsVisible = false;
        }

        private void FilterView_OnDisAppearing()
        {
            this.FilterButton.IsVisible = true;
        }

        protected override void UpdatePaginationBar()
        {
            PagingMeta meta = base.GetPagingMeta();

            if (meta != null)
            {
                this.PagingMeta.Text = meta.Meta;
                this.PagingPageCount.Text = meta.PageCount;
            }
        }

        private void FullScreenButton_Clicked(object sender, EventArgs e)
        {
            HeaderView.IsVisible = !HeaderView.IsVisible;
        }

        private void SearchButton_Clicked(object sender, EventArgs e)
        {
            SearchButton.IsVisible = false;
            SearchBox.IsVisible = true;
            SearchBox.Focus();
        }

        protected override bool BeforeBackButtonPressed()
        {
            if (FilterView.IsVisible)
            {
                FilterView.Hide();
                return false;
            }

            if (SearchBox.IsVisible)
            {
                SearchBox.Unfocus();
                SearchBox.IsVisible = false;
                SearchButton.IsVisible = true;

                return false;
            }
            return true;
        }

        protected override bool OnBackButtonPressed()
        {
            if (BeforeBackButtonPressed())
            {
                base.OnBackButtonPressed();
                return false;
            }
            return true;
        }

        private bool ToolBarBackButtonPressed(object sender, EventArgs e)
        {
            return this.BeforeBackButtonPressed();
        }
    }
}