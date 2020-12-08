using ExpressBase.Mobile.ViewModels.Dynamic;
using System;
using Xamarin.Forms.Xaml;
using ExpressBase.Mobile.CustomControls;
using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Views.Base;
using ExpressBase.Mobile.CustomControls.XControls;
using ExpressBase.Mobile.CustomControls.Views;
using Xamarin.Forms;

namespace ExpressBase.Mobile.Views.Dynamic
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ListRender : ListViewBasePage
    {
        public ListRender(EbMobilePage Page)
        {
            InitializeComponent();
            BindingContext = ViewModel = new ListViewModel(Page);
            EbLayout.ShowLoader();
        }

        public ListRender(EbMobilePage Page, EbDataRow row)
        {
            InitializeComponent();
            BindingContext = ViewModel = new ListViewModel(Page, row);
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

        private void ToggleLinks()
        {
            if (this.HasFabLink)
            {
                if (this.HasFabText)
                {
                    FormLinkTextLabel.Text = ViewModel.Visualization.NewButtonText;
                    FormLinkWText.IsVisible = true;
                }
                else
                    FormLinkWOText.IsVisible = true;
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

        private void SearchButton_Clicked(object sender, EventArgs e)
        {
            FilterView.Hide();

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

        public override void ShowAudioFiles(EbPlayButton playButton)
        {
            if (playButton.AudioFiles == null)
                return;

            AudioPopup.Children.Clear();

            foreach (var file in playButton.AudioFiles)
            {
                EbAudioTemplate template = new EbAudioTemplate(file.Bytea)
                {
                    AllowDelete = false
                };
                Frame view = (Frame)template.CreateView();
                view.HasShadow = false;
                AudioPopup.Children.Add(view);
            }
            AudioPopupView.Show();
        }
    }
}