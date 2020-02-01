using ExpressBase.Mobile.CustomControls;
using ExpressBase.Mobile.ViewModels.Dynamic;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ExpressBase.Mobile.Views.Dynamic
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LinkedListViewRender : ContentPage
    {
        public LinkedListViewRender()
        {
            InitializeComponent();
            this.BindingContext = new LinkedListViewModel();
        }

        public LinkedListViewRender(EbMobilePage LinkPage, EbMobileVisualization SourceVis, CustomFrame CustFrame)
        {
            InitializeComponent();
            try
            {
                var Renderer = new LinkedListViewModel(LinkPage, SourceVis, CustFrame);

                HeaderContainer.Children.Add(Renderer.HeaderFrame);
                Grid.SetRow(Renderer.HeaderFrame, 0);

                ScrollContainer.Content = Renderer.View;

                BindingContext = Renderer;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (this.BindingContext is LinkedListViewModel vm)
            {
                if (vm.IsRedirect)
                {
                    vm.RefreshPage();
                    ScrollContainer.Content = vm.View;
                    vm.IsRedirect = false;
                }
            }
        }
    }
}