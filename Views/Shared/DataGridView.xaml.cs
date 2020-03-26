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
            InitializeComponent();

            Mode = GridMode.Edit;
            RowName = name;

            SaveAndContinue.IsVisible = false;
            SaveAndClose.Text = "Save Changes";
            Grid.SetColumn(SaveAndClose, 0);
            SaveAndClose.BackgroundColor = Color.FromHex("0046bb");
            SaveAndClose.TextColor = Color.White;

            this.DataGrid = dataGrid;
            this.CreateForm();
            this.FillValue(row);
        }

        private void CreateForm()
        {
            foreach (var ctrl in this.DataGrid.ChildControls)
            {
                ctrl.InitXControl(this.DataGrid.Mode);
                ctrl.Required = true;
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

        private void SaveAndContinue_Clicked(object sender, EventArgs e)
        {
            if (!Validate())
                return;

            if (Mode == GridMode.New)
                DataGrid.RowAddCallBack();
            else
                DataGrid.RowAddCallBack(this.RowName);

            DependencyService.Get<IToast>().Show("1 row added.");
            this.ResetControls();
        }

        private void SaveAndClose_Clicked(object sender, EventArgs e)
        {
            if (!Validate())
                return;

            (Application.Current.MainPage as MasterDetailPage).Detail.Navigation.PopModalAsync();
            if (Mode == GridMode.New)
                DataGrid.RowAddCallBack();
            else
                DataGrid.RowAddCallBack(this.RowName);
        }

        private void ResetControls()
        {
            foreach (var ctrl in DataGrid.ChildControls)
                ctrl.Reset();
        }

        private void BackButton_Clicked(object sender, EventArgs e)
        {
            (Application.Current.MainPage as MasterDetailPage).Detail.Navigation.PopModalAsync();
        }

        public bool Validate()
        {
            foreach (var ctrl in DataGrid.ChildControls)
            {
                if (ctrl.Required && ctrl.GetValue() == null)
                    return false;
            }
            return true;
        }
    }
}