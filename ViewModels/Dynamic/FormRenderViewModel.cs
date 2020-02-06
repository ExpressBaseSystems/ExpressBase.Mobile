using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Models;
using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ExpressBase.Mobile.ViewModels.Dynamic
{
    public class FormRenderViewModel : BaseViewModel
    {
        public EbMobileForm Form { set; get; }

        private FormMode Mode { set; get; } = FormMode.NEW;

        public View View { set; get; }

        private int RowId { set; get; } = 0;

        public Command SaveCommand => new Command(OnSaveClicked);

        public EbMobileForm ParentForm { set; get; }

        public EbDataTable DataOnEdit { set; get; }

        //new mode
        public FormRenderViewModel(EbMobilePage Page)
        {
            PageTitle = Page.DisplayName;
            try
            {
                Form = (Page.Container as EbMobileForm);
                CreateView();
                this.Form.CreateTableSchema();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
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
                this.Form.CreateTableSchema();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
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
                Console.WriteLine(ex.Message);
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
                Console.WriteLine(e.Message);
                dt = new EbDataTable();
            }
            return dt;
        }

        private void CreateView()
        {
            StackLayout ScrollStack = new StackLayout { Spacing = 0 };

            foreach (var ctrl in this.Form.ChiledControls)
                this.EbCtrlToXamCtrl(ctrl, ScrollStack);

            this.View = ScrollStack;
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

                    if (this.Mode == FormMode.EDIT)
                    {
                        EbDataColumn _col = this.DataOnEdit.Columns[ctrl.Name];

                        if (_col != null)
                            ctrl.SetValue(this.DataOnEdit.Rows[0][_col.ColumnIndex]);
                        else if (ctrl is EbMobileFileUpload)
                            (ctrl as EbMobileFileUpload).RenderOnEdit(this.Form.TableName, this.RowId);
                        ctrl.SetAsReadOnly(true);
                    }
                    ContentStackTop.Children.Add(ctrl.XView);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void OnSaveClicked(object sender)
        {
            bool status;

            if (this.Mode == FormMode.REF)
                status = this.Form.SaveWithParentId(this.RowId, ParentForm.TableName);
            else
                status = this.Form.Save(this.RowId);

            IToast Toast = DependencyService.Get<IToast>();
            if (status && this.RowId == 0)
            {
                Toast.Show("Data pushed successfully :)");
                (Application.Current.MainPage as MasterDetailPage).Detail.Navigation.PopAsync(true);
            }
            else if (status && this.RowId > 0)
            {
                Toast.Show("Changes saved successfully :)");
                (Application.Current.MainPage as MasterDetailPage).Detail.Navigation.PopToRootAsync(true);
            }
            else
            {
                Toast.Show("Something went wrong!");
                (Application.Current.MainPage as MasterDetailPage).Detail.Navigation.PopToRootAsync(true);
            }
        }

        private void PushFromTableLayout(EbMobileTableLayout TL, StackLayout ContentStackTop)
        {
            foreach (EbMobileTableCell Tc in TL.CellCollection)
            {
                foreach (var ctrl in Tc.ControlCollection)
                {
                    this.EbCtrlToXamCtrl(ctrl, ContentStackTop);
                }
            }
        }
    }
}
