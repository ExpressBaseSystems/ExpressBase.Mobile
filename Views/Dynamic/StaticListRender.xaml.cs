using ExpressBase.Mobile.ViewModels.Dynamic.ListView;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ExpressBase.Mobile.Views.Dynamic
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class StaticListRender : ContentPage
    {
        private readonly StaticListViewModel viewModel;

        public StaticListRender(EbMobilePage page)
        {
            InitializeComponent();
            BindingContext = viewModel = new StaticListViewModel(page);
        }

        private void SearchButton_Clicked(object sender, EventArgs e)
        {
            SearchButton.IsVisible = false;
            SearchBox.IsVisible = true;
            SearchBox.Focus();
        }

        protected bool BeforeBackButtonPressed()
        {
            if (SearchBox.IsVisible)
            {
                SearchBox.Unfocus();
                SearchBox.IsVisible = false;
                SearchButton.IsVisible = true;

                viewModel.SetInitialState();

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