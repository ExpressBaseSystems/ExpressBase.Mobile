﻿using ExpressBase.Mobile.CustomControls;
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

        public LinkedListRender()
        {
            InitializeComponent();
            this.BindingContext = ViewModel = new LinkedListViewModel();
        }

        public LinkedListRender(EbMobilePage LinkPage, EbMobileVisualization SourceVis, CustomFrame CustFrame)
        {
            InitializeComponent();
            try
            {
                BindingContext = ViewModel = new LinkedListViewModel(LinkPage, SourceVis, CustFrame);

                HeaderContainer.Children.Add(ViewModel.HeaderFrame);
                Grid.SetRow(ViewModel.HeaderFrame, 0);

                ScrollContainer.Content = ViewModel.XView;
                this.UpdatePaginationBar();
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
                ViewModel.SetParameters(ViewModel.HeaderFrame.DataRow);
                ViewModel.SetData(Offset);
                ViewModel.CreateView();
                ScrollContainer.Content = ViewModel.XView;

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