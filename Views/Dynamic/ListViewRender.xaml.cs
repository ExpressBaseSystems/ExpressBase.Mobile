using ExpressBase.Mobile.ViewModels.Dynamic;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Collections.Generic;
using ExpressBase.Mobile.CustomControls;
using ExpressBase.Mobile.Data;
using System.Linq;
using ExpressBase.Mobile.Models;
using Xamarin.Essentials;

namespace ExpressBase.Mobile.Views.Dynamic
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ListViewRender : ContentPage
    {
        private int Offset = 0;

        private int PageCount = 1;

        public ListViewRenderViewModel Renderer { set; get; }

        public ListViewRender(EbMobilePage Page)
        {
            InitializeComponent();
            try
            {
                Renderer = new ListViewRenderViewModel(Page);
                BindingContext = Renderer;

                if (Renderer.DataTable.Rows.Any())
                {
                    listContainer.Content = Renderer.XView;

                    if (Renderer.FilterDialog != null)
                    {
                        FilterActionBar.IsVisible = true;
                        FilterContainer.Content = Renderer.FilterDialog;
                    }
                }
                else
                    EmptyRecordLabel.IsVisible = true;

                int toVal = (Renderer.DataTable.Rows.Count < Renderer.DataCount) ? Renderer.Visualization.PageLength : Renderer.DataCount;
                PagingMeta.Text = $"Showing {Offset} to {toVal} of {Renderer.DataCount} entries";

                if (Page.NetworkMode == NetworkMode.Online && !Settings.HasInternet)
                    ShowMessage("You are not connected to internet!", Color.FromHex("fd6b6b"));

                Connectivity.ConnectivityChanged += Connectivity_ConnectivityChanged;
            }
            catch (Exception ex)
            {
                Log.Write("ListViewRender.Constructor invoke---" + ex.Message);
            }
        }

        private void FilterButton_Clicked(object sender, EventArgs e)
        {
            FilterDialogView.IsVisible = true;
            PagingContainer.IsVisible = false;
        }

        private void FDCancel_Clicked(object sender, EventArgs e)
        {
            FilterDialogView.IsVisible = false;
            PagingContainer.IsVisible = true;
        }

        private void FDApply_Clicked(object sender, EventArgs e)
        {
            try
            {
                var paramDict = new Dictionary<string, object>();

                foreach (KeyValuePair<string, View> pair in Renderer.FilterControls)
                {
                    if (paramDict.ContainsKey(pair.Key))
                        continue;
                    var ctrlValue = GetControlValue(pair.Value);
                    if (ctrlValue != null)
                        paramDict.Add(pair.Key, ctrlValue);
                }

                FilterDialogView.IsVisible = false;
                PagingContainer.IsVisible = true;

                if (paramDict.Any())
                {
                    List<DbParameter> parameters = paramDict.Select(item => new DbParameter { ParameterName = item.Key, Value = item.Value }).ToList();
                    Renderer.Refresh(parameters);
                    listContainer.Content = Renderer.XView;
                }
            }
            catch (Exception ex)
            {
                Log.Write("ListViewRender.FDApply_Clicked---" + ex.Message);
            }
        }

        private object GetControlValue(View ctrl)
        {
            if (ctrl is TextBox)
                return (ctrl as TextBox).Text;
            else if (ctrl is CustomDatePicker)
                return (ctrl as CustomDatePicker).Date;
            else if (ctrl is CustomCheckBox)
                return (ctrl as CustomCheckBox).IsChecked;
            else
                return null;
        }

        private void PagingPrevButton_Clicked(object sender, EventArgs e)
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
            catch(Exception ex)
            {
                Log.Write("ListViewRender.PrevButton_Clicked---" + ex.Message);
            }
        }

        private void PagingNextButton_Clicked(object sender, EventArgs e)
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
            listContainer.Content = Renderer.XView;
            PagingPageCount.Text = PageCount.ToString();
            PagingMeta.Text = $"Showing {Offset} to {Offset + Renderer.Visualization.PageLength} of {Renderer.DataCount} entries";
        }

        private void ShowMessage(string message, Color background)
        {
            MessageBoxFrame.BackgroundColor = background;
            MessageBoxLabel.Text = message;
            MessageBox.IsVisible = true;
        }

        private void HideMessage(string message_beforehide, Color background)
        {
            MessageBoxFrame.BackgroundColor = background;
            MessageBoxLabel.Text = message_beforehide;
            MessageBox.IsVisible = false;
        }

        private void Connectivity_ConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
        {
            if (e.NetworkAccess == NetworkAccess.Internet)
                HideMessage("Back to online", Color.FromHex("41d041"));
            else
                ShowMessage("You are not connected to internet!", Color.FromHex("fd6b6b"));
        }
    }
}