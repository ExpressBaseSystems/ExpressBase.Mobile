using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ExpressBase.Mobile.Views.Shared
{
    public enum GridMode
    {
        New = 0,
        Edit = 1
    }

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DataGridView : ContentPage
    {
        public GridMode Mode { set; get; }

        public string RowName { set; get; }

        public EbMobileDataGrid DataGrid { set; get; }

        public DataGridView()
        {
            InitializeComponent();
        }

        public DataGridView(EbMobileDataGrid dataGrid)
        {
            Mode = GridMode.New;
            this.DataGrid = dataGrid;
            InitializeComponent();
            this.CreateForm();
        }

        //edit
        public DataGridView(EbMobileDataGrid dataGrid, MobileTableRow row, string name)
        {
            Mode = GridMode.Edit;
            RowName = name;

            this.DataGrid = dataGrid;
            InitializeComponent();
            this.CreateForm();
            this.FillValue(row);
        }

        private void CreateForm()
        {
            foreach (var ctrl in this.DataGrid.ChildControls)
            {
                ctrl.InitXControl(this.DataGrid.Mode);
                ControlContainer.Children.Add(ctrl.XView);
            }
        }

        private void FillValue(MobileTableRow row)
        {
            foreach (var ctrl in this.DataGrid.ChildControls)
            {
                var col = row[ctrl.Name];
                if (col != null)
                    ctrl.SetValue(col.Value);
            }
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            (Application.Current.MainPage as MasterDetailPage).Detail.Navigation.PopModalAsync();
            if (Mode == GridMode.New)
                DataGrid.RowAddCallBack();
            else
                DataGrid.RowAddCallBack(this.RowName);
        }
    }
}