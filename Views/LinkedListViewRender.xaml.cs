using ExpressBase.Mobile.CustomControls;
using ExpressBase.Mobile.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ExpressBase.Mobile.Views
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
    }
}