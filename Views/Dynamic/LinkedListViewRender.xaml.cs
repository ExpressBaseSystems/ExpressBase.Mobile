using ExpressBase.Mobile.CustomControls;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.ViewModels.Dynamic;
using System;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ExpressBase.Mobile.Views.Dynamic
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LinkedListViewRender : ContentPage
    {
        private int Offset = 0;

        private int PageCount = 1;

        public LinkedListViewModel Renderer { set; get; }

        public LinkedListViewRender()
        {
            InitializeComponent();
            Renderer = new LinkedListViewModel();
            this.BindingContext = Renderer;
        }

        public LinkedListViewRender(EbMobilePage LinkPage, EbMobileVisualization SourceVis, CustomFrame CustFrame)
        {
            InitializeComponent();
            try
            {
                Renderer = new LinkedListViewModel(LinkPage, SourceVis, CustFrame);
                HeaderContainer.Children.Add(Renderer.HeaderFrame);
                Grid.SetRow(Renderer.HeaderFrame, 0);

                if (Renderer.DataTable.Rows.Any())
                    ScrollContainer.Content = Renderer.XView;
                else
                    EmptyRecordLabel.IsVisible = true;

                int toVal = (Renderer.DataTable.Rows.Count < Renderer.DataCount) ? Renderer.Visualization.PageLength : Renderer.DataCount;
                PagingMeta.Text = $"Showing {Offset} to {toVal} of {Renderer.DataCount} entries";

                BindingContext = Renderer;
            }
            catch (Exception ex)
            {
                Log.Write("LinkedListViewRender.Constructor---" + ex.Message);
            }
        }

        public void PagingPrevButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                if (Renderer != null)
                {
                    if (Offset <= 0) return;

                    Offset -= Renderer.Visualization.PageLength;
                    PageCount--;
                    ResetPagedData();
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
                if (Renderer != null)
                {
                    if (Offset + Renderer.Visualization.PageLength >= Renderer.DataCount) return;

                    Offset += Renderer.Visualization.PageLength;
                    PageCount++;
                    ResetPagedData();
                }
            }
            catch (Exception ex)
            {
                Log.Write("ListViewRender.NextButton_Clicked---" + ex.Message);
            }
        }

        private void ResetPagedData()
        {
            Renderer.SetData(Offset);
            Renderer.CreateView();
            ScrollContainer.Content = Renderer.XView;
            PagingPageCount.Text = PageCount.ToString();
            PagingMeta.Text = $"Showing {Offset} to {Offset + Renderer.Visualization.PageLength} of {Renderer.DataCount} entries";
        }
    }
}