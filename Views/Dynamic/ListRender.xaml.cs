using ExpressBase.Mobile.ViewModels.Dynamic;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Collections.Generic;
using ExpressBase.Mobile.CustomControls;
using ExpressBase.Mobile.Data;
using System.Linq;
using ExpressBase.Mobile.Models;

namespace ExpressBase.Mobile.Views.Dynamic
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ListRender : ContentPage
    {
        private int Offset = 0;

        private int PageCount = 1;

        public ListViewModel ViewModel { set; get; }

        public ListRender(EbMobilePage Page)
        {
            InitializeComponent();
            try
            {
                BindingContext = ViewModel = new ListViewModel(Page);
                if (ViewModel.DataTable.Rows.Any())
                {
                    listContainer.Content = ViewModel.XView;
                    if (ViewModel.FilterDialog != null)
                    {
                        FilterActionBar.IsVisible = true;
                        FilterContainer.Content = ViewModel.FilterDialog;
                    }
                }
                else
                    EmptyRecordLabel.IsVisible = true;

                this.UpdatePaginationBar();
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

                foreach (KeyValuePair<string, View> pair in ViewModel.FilterControls)
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
                    ViewModel.Refresh(parameters);
                    listContainer.Content = ViewModel.XView;
                    this.Offset = 0;
                    this.UpdatePaginationBar();
                }
            }
            catch (Exception ex)
            {
                Log.Write("ListViewRender.FDApply_Clicked---" + ex.Message);
            }
        }

        private object GetControlValue(View ctrl)
        {
            object value = null;

            if (ctrl is TextBox)
            {
                if (!string.IsNullOrEmpty((ctrl as TextBox).Text))
                    value = (ctrl as TextBox).Text;
            }
            else if (ctrl is NumericTextBox)
            {
                if (!string.IsNullOrEmpty((ctrl as NumericTextBox).Text))
                    value = (ctrl as NumericTextBox).Text;
            }
            else if (ctrl is CustomDatePicker)
            {
                value = (ctrl as CustomDatePicker).Date.ToString("yyyy-MM-dd");
            }
            else if (ctrl is CustomCheckBox)
            {
                value = (ctrl as CustomCheckBox).IsChecked;
            }
            return value;
        }

        private void PagingPrevButton_Clicked(object sender, EventArgs e)
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

        private void PagingNextButton_Clicked(object sender, EventArgs e)
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

        private void RefreshListView()
        {
            try
            {
                ViewModel.SetData(Offset);
                ViewModel.CreateView();
                listContainer.Content = ViewModel.XView;

                this.UpdatePaginationBar();
            }
            catch (Exception ex)
            {
                Log.Write(ex.Message);
            }
        }

        private void UpdatePaginationBar()
        {
            try
            {
                int totalEntries = ViewModel.DataCount;
                int offset = this.Offset + 1;
                int length = ViewModel.Visualization.PageLength + offset - 1;

                if (totalEntries < ViewModel.Visualization.PageLength)
                    length = totalEntries;

                if (ViewModel.Visualization.PageLength + offset > totalEntries)
                    length = totalEntries;

                PagingMeta.Text = $"Showing {offset} to {length} of {totalEntries} entries";
                PagingPageCount.Text = PageCount.ToString();
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