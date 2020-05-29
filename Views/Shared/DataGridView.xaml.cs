using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using System;
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
        private readonly GridMode mode;

        public string rowName;

        private readonly EbMobileDataGrid dataGrid;

        public DataGridView()
        {
            InitializeComponent();
        }

        public DataGridView(EbMobileDataGrid dg)
        {
            mode = GridMode.New;
            dataGrid = dg;
            InitializeComponent();
            this.CreateForm();
        }

        //edit
        public DataGridView(EbMobileDataGrid dataGrid, MobileTableRow row, string name)
        {
            InitializeComponent();

            mode = GridMode.Edit;
            rowName = name;

            SaveAndContinue.IsVisible = false;
            SaveAndClose.Text = "Save Changes";
            Grid.SetColumn(SaveAndClose, 0);
            SaveAndClose.BackgroundColor = Color.FromHex("0046bb");
            SaveAndClose.TextColor = Color.White;

            this.dataGrid = dataGrid;
            this.CreateForm();
            this.FillValue(row);
        }

        private void CreateForm()
        {
            foreach (var ctrl in this.dataGrid.ChildControls)
            {
                ctrl.InitXControl(this.dataGrid.FormRenderMode, this.dataGrid.NetworkType);
                ctrl.Required = true;
                ControlContainer.Children.Add(ctrl.XView);
            }
        }

        private void FillValue(MobileTableRow row)
        {
            foreach (var ctrl in this.dataGrid.ChildControls)
            {
                var col = row[ctrl.Name];
                if (col != null)
                    ctrl.SetValue(col.Value);
            }
        }

        private void SaveAndContinue_Clicked(object sender, EventArgs e)
        {
            if (!Validate())
                return;

            if (mode == GridMode.New)
                dataGrid.RowAddCallBack();
            else
                dataGrid.RowAddCallBack(this.rowName);

            DependencyService.Get<IToast>().Show("1 row added.");
            this.ResetControls();
        }

        private void SaveAndClose_Clicked(object sender, EventArgs e)
        {
            if (!Validate())
                return;

            (Application.Current.MainPage as MasterDetailPage).Detail.Navigation.PopModalAsync();
            if (mode == GridMode.New)
                dataGrid.RowAddCallBack();
            else
                dataGrid.RowAddCallBack(this.rowName);
        }

        private void ResetControls()
        {
            foreach (var ctrl in dataGrid.ChildControls)
                ctrl.Reset();
        }

        private void BackButton_Clicked(object sender, EventArgs e)
        {
            (Application.Current.MainPage as MasterDetailPage).Detail.Navigation.PopModalAsync();
        }

        public bool Validate()
        {
            foreach (var ctrl in dataGrid.ChildControls)
            {
                if (ctrl.Required && ctrl.GetValue() == null)
                    return false;
            }
            return true;
        }
    }
}