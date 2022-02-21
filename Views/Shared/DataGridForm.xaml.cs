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

        private bool isTapped;

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
            Grid.SetColumn(SaveAndClose, 0);
            Grid.SetColumnSpan(SaveAndClose, 3);
            Grid.SetColumn(CancelAndClose, 3);
            Grid.SetColumnSpan(CancelAndClose, 3);

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
            EbFormHelper.AddAllControlViews(ControlContainer, dataGrid.ChildControls, dataGrid.FormRenderMode, dataGrid.NetworkType, null, dataGrid.Name, true);
        }

        private void FillValue(MobileTableRow row)
        {
            foreach (EbMobileControl ctrl in dataGrid.ChildControls)
            {
                MobileTableColumn col = row[ctrl.Name];

                if (col != null)
                {
                    ctrl.DoNotPropagateChange = true;
                    ctrl.SetValue(col.Value);
                    ctrl.DoNotPropagateChange = false;
                }
            }
        }

        private void OnSaveAndContinueClicked(object sender, EventArgs e)
        {
            if (isTapped || this.dataGrid.IsTaped())
                return;
            isTapped = true;
            string invalidMsg = EbFormHelper.ValidateDataGrid(this.dataGrid);
            if (invalidMsg == null)
            {
                OnInserted?.Invoke(mode == GridMode.New ? null : rowName);
                Utils.Toast("1 row added.");
                ResetControls();
                EbFormHelper.ExecDGOuterDependency(this.dataGrid.Name);
            }
            else
                Utils.Toast(invalidMsg);
            isTapped = false;
        }

        private async void OnSaveAndCloseClicked(object sender, EventArgs e)
        {
            if (isTapped || this.dataGrid.IsTaped())
                return;
            isTapped = true;
            string invalidMsg = EbFormHelper.ValidateDataGrid(this.dataGrid);
            if (invalidMsg == null)
            {
                OnInserted?.Invoke(mode == GridMode.New ? null : rowName);
                ResetControls();
                EbFormHelper.ExecDGOuterDependency(this.dataGrid.Name);
                await App.Navigation.PopMasterModalAsync(true);
                this.dataGrid.IsDgViewOpen = false;
            }
            else
                Utils.Toast(invalidMsg);
            isTapped = false;
        }

        private void ResetControls()
        {
            foreach (EbMobileControl ctrl in dataGrid.ChildControls)
            {
                ctrl.DoNotPropagateChange = true;
                ctrl.Reset();
                ctrl.SetValidation(true, string.Empty);
                ctrl.DoNotPropagateChange = false;
            }
        }

        private async void OnBackButtonClicked(object sender, EventArgs e)
        {
            if (isTapped || this.dataGrid.IsTaped())
                return;
            isTapped = true;
            ResetControls();
            await App.Navigation.PopMasterModalAsync(true);
            this.dataGrid.IsDgViewOpen = false;
            isTapped = false;
        }

        protected override bool OnBackButtonPressed()
        {
            OnBackButtonClicked(null, null);
            return true;
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