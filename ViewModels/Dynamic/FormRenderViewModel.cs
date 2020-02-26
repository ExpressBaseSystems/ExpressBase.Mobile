using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.ViewModels.BaseModels;
using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ExpressBase.Mobile.ViewModels.Dynamic
{
    public class FormRenderViewModel : DynamicBaseViewModel
    {
        public EbMobileForm Form { set; get; }

        private FormMode Mode { set; get; } = FormMode.NEW;

        private int RowId { set; get; } = 0;

        public Command SaveCommand => new Command(OnSaveClicked);

        public EbMobileForm ParentForm { set; get; }

        public EbDataTable DataOnEdit { set; get; }

        //new mode
        public FormRenderViewModel(EbMobilePage page) : base(page)
        {
            try
            {
                Form = (this.Page.Container as EbMobileForm);
                CreateView();
                this.Form.CreateTableSchema();
            }
            catch (Exception ex)
            {
                Log.Write("Form render new mode---" + ex.Message);
            }
        }

        //edit mode
        public FormRenderViewModel(EbMobilePage Page, int RowIdLocal)
        {
            this.Mode = FormMode.EDIT;
            try
            {
                this.RowId = RowIdLocal;
                this.Form = (Page.Container as EbMobileForm);
                PageTitle = Page.DisplayName;

                this.DataOnEdit = GetDataOnEdit();

                this.CreateView();
                this.FillControls(this.DataOnEdit.Rows[0], this.DataOnEdit.Columns);
                this.Form.CreateTableSchema();
            }
            catch (Exception ex)
            {
                Log.Write("Form render edit mode---" + ex.Message);
            }
        }

        //referenced mode
        public FormRenderViewModel(EbMobilePage Page, EbMobilePage ParentPage, int ParentId)
        {
            this.Mode = FormMode.REF;
            ParentForm = (ParentPage.Container as EbMobileForm);
            try
            {
                this.RowId = ParentId;
                this.Form = (Page.Container as EbMobileForm);
                PageTitle = Page.DisplayName;

                this.CreateView();
                this.Form.CreateTableSchema();
            }
            catch (Exception ex)
            {
                Log.Write("Form render reference mode" + ex.Message);
            }
        }

        //prefill new mode
        public FormRenderViewModel(EbMobilePage Page, EbDataRow dataRow, ColumnColletion dataColumns)
        {
            this.Mode = FormMode.NEW;
            try
            {
                this.Form = (Page.Container as EbMobileForm);
                PageTitle = Page.DisplayName;

                this.CreateView();
                this.FillControls(dataRow, dataColumns);
                this.Form.CreateTableSchema();
            }
            catch (Exception ex)
            {
                Log.Write("Form render prefill mode" + ex.Message);
            }
        }

        private EbDataTable GetDataOnEdit()
        {
            EbDataTable dt;
            try
            {
                string sql = $"SELECT * FROM {this.Form.TableName} WHERE id = {this.RowId}";

                dt = App.DataDB.DoQuery(sql);
            }
            catch (Exception e)
            {
                Log.Write("form_GetDataOnEdit---" + e.Message);
                dt = new EbDataTable();
            }
            return dt;
        }

        private void CreateView()
        {
            StackLayout ScrollStack = new StackLayout { Spacing = 0 };

            foreach (var ctrl in this.Form.ChildControls)
                this.EbCtrlToXamCtrl(ctrl, ScrollStack);

            this.XView = ScrollStack;
        }

        private void EbCtrlToXamCtrl(EbMobileControl ctrl, StackLayout ContentStackTop)
        {
            try
            {
                if (ctrl is EbMobileTableLayout)
                    this.PushFromTableLayout((ctrl as EbMobileTableLayout), ContentStackTop);
                else
                {
                    ctrl.InitXControl(this.Mode);
                    ContentStackTop.Children.Add(ctrl.XView);
                    this.Form.ControlDictionary.Add(ctrl.Name, ctrl);
                }
            }
            catch (Exception ex)
            {
                Log.Write("Form_EbCtrlToXamCtrl---" + ex.Message);
            }
        }

        public void OnSaveClicked(object sender)
        {
            this.Form.NetworkType = this.NetworkType;
            FormSaveResponse saveResponse;

            if (this.Mode == FormMode.REF)
                saveResponse = this.Form.SaveFormWParent(this.RowId, ParentForm.TableName);
            else
                saveResponse = this.Form.SaveForm(this.RowId);

            DependencyService.Get<IToast>().Show(saveResponse.Message);
            (Application.Current.MainPage as MasterDetailPage).Detail.Navigation.PopAsync(true);
        }

        private void PushFromTableLayout(EbMobileTableLayout TL, StackLayout ContentStackTop)
        {
            foreach (EbMobileTableCell Tc in TL.CellCollection)
            {
                foreach (var ctrl in Tc.ControlCollection)
                    this.EbCtrlToXamCtrl(ctrl, ContentStackTop);
            }
        }

        public void FillControls(EbDataRow row, ColumnColletion columns)
        {
            foreach (var pair in this.Form.ControlDictionary)
            {
                EbDataColumn _col = columns[pair.Value.Name];

                if (_col != null)
                    pair.Value.SetValue(row[_col.ColumnIndex]);

                if (this.Mode == FormMode.EDIT)
                {
                    pair.Value.SetAsReadOnly(true);

                    if (pair.Value is EbMobileFileUpload)
                        (pair.Value as EbMobileFileUpload).RenderOnEdit(this.Form.TableName, this.RowId);
                }
            }
        }
    }
}
