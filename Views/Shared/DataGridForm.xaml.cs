using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Views.Base;
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
    public partial class DataGridForm : ContentPage
    {
        private readonly GridMode mode;

        public string rowName;

        public event DataGridInsertHandler OnInserted;

        private readonly EbMobileDataGrid dataGrid;

        public DataGridForm(EbMobileDataGrid dataGrid)
        {
            mode = GridMode.New;
            this.dataGrid = dataGrid;

            InitializeComponent();
            CreateForm();
            SetValues();
        }

        public DataGridForm(EbMobileDataGrid dataGrid, MobileTableRow row, string name)
        {
            InitializeComponent();

            mode = GridMode.Edit;
            rowName = name;
            this.dataGrid = dataGrid;

            SaveAndContinue.IsVisible = false;
            SaveAndClose.Text = "Save Changes";
            Grid.SetColumn(SaveAndClose, 0);
            SaveAndClose.BackgroundColor = Color.FromHex("0046bb");
            SaveAndClose.TextColor = Color.White;

            CreateForm();
            FillValue(row);
        }

        protected virtual void SetValues()
        {
            InitDefaultValueExpressions();
            InitOnLoadExpressions();
        }
        private void CreateForm()
        {
            foreach (EbMobileControl ctrl in dataGrid.ChildControls)
            {
                ctrl.Parent = dataGrid.Name;
                if (ctrl is EbMobileTableLayout table)
                {
                    ControlContainer.Children.Add(table.GetGridObject(dataGrid.Name, dataGrid.FormRenderMode, dataGrid.NetworkType, null));
                }
                else
                {
                    View view = ctrl.XControl == null ? ctrl.Draw(dataGrid.FormRenderMode, dataGrid.NetworkType) : ctrl.XView;
                    ControlContainer.Children.Add(view);
                }
            }
        }

        private void FillValue(MobileTableRow row)
        {
            foreach (EbMobileControl ctrl in dataGrid.ChildControls)
            {
                MobileTableColumn col = row[ctrl.Name];

                if (col != null)
                {
                    ctrl.SetValue(col.Value);
                }
            }
        }

        private void OnSaveAndContinueClicked(object sender, EventArgs e)
        {
            if (this.dataGrid.IsTaped())
                return;
            OnInserted?.Invoke(mode == GridMode.New ? null : rowName);
            Utils.Toast("1 row added.");
            ResetControls();
            EbFormHelper.ExecDGOuterDependency(this.dataGrid.Name);
        }

        private async void OnSaveAndCloseClicked(object sender, EventArgs e)
        {
            if (this.dataGrid.IsTaped())
                return;
            OnInserted?.Invoke(mode == GridMode.New ? null : rowName);
            ResetControls();
            EbFormHelper.ExecDGOuterDependency(this.dataGrid.Name);
            await App.Navigation.PopMasterModalAsync(true);
        }

        private void ResetControls()
        {
            foreach (EbMobileControl ctrl in dataGrid.ChildControls) ctrl.Reset();
        }

        private async void OnBackButtonClicked(object sender, EventArgs e)
        {
            ResetControls();
            await App.Navigation.PopMasterModalAsync(true);
        }

        protected void InitDefaultValueExpressions()
        {
            foreach (EbMobileControl ctrl in dataGrid.ChildControls)
            {
                if (!ctrl.DefaultExprEvaluated)
                {
                    EbScript defExp = ctrl.DefaultValueExpression;

                    if (defExp != null && !defExp.IsEmpty())
                    {
                        EbFormHelper.SetDefaultValue(ctrl.Name);
                    }
                }
            }
        }

        protected void InitOnLoadExpressions()
        {
            foreach (EbMobileControl ctrl in dataGrid.ChildControls)
            {
                EbFormHelper.EvaluateExprOnLoad(ctrl, dataGrid.FormRenderMode);
            }
        }
    }
}