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
                this.Form = this.Page.Container as EbMobileForm;
                this.CreateView();
                this.Form.CreateTableSchema();
            }
            catch (Exception ex)
            {
                Log.Write("Form render new mode---" + ex.Message);
            }
        }

        //edit mode
        public FormRenderViewModel(EbMobilePage page, int RowIdLocal) : base(page)
        {
            this.Mode = FormMode.EDIT;
            try
            {
                this.RowId = RowIdLocal;
                this.Form = page.Container as EbMobileForm;
                this.DataOnEdit = GetDataOnEdit();
                this.CreateView();
                this.FillControls(this.DataOnEdit.Rows[RowIdLocal]);
                this.Form.CreateTableSchema();
            }
            catch (Exception ex)
            {
                Log.Write("Form render edit mode---" + ex.Message);
            }
        }

        //prefill new mode
        public FormRenderViewModel(EbMobilePage page, EbDataRow dataRow) : base(page)
        {
            this.Mode = FormMode.NEW;
            try
            {
                this.Form = page.Container as EbMobileForm;

                this.CreateView();
                this.FillControls(dataRow);
                this.Form.CreateTableSchema();
            }
            catch (Exception ex)
            {
                Log.Write("Form render prefill mode" + ex.Message);
            }
        }

        //referenced mode
        public FormRenderViewModel(EbMobilePage page, EbMobilePage ParentPage, int ParentId) : base(page)
        {
            this.Mode = FormMode.REF;
            ParentForm = (ParentPage.Container as EbMobileForm);
            try
            {
                this.RowId = ParentId;
                this.Form = page.Container as EbMobileForm;
                this.CreateView();
                this.Form.CreateTableSchema();
            }
            catch (Exception ex)
            {
                Log.Write("Form render reference mode" + ex.Message);
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
            try
            {
                StackLayout ScrollStack = new StackLayout { Spacing = 0 };

                foreach (var ctrl in this.Form.ChildControls)
                {
                    if (ctrl is EbMobileTableLayout)
                    {
                        foreach (EbMobileTableCell Tc in (ctrl as EbMobileTableLayout).CellCollection)
                        {
                            foreach (var tbctrl in Tc.ControlCollection)
                            {
                                tbctrl.InitXControl(this.Mode);
                                ScrollStack.Children.Add(tbctrl.XView);
                                this.Form.ControlDictionary.Add(tbctrl.Name, tbctrl);
                            }
                        }
                    }
                    else
                    {
                        ctrl.InitXControl(this.Mode);
                        ScrollStack.Children.Add(ctrl.XView);
                        this.Form.ControlDictionary.Add(ctrl.Name, ctrl);
                    }
                }
                this.XView = ScrollStack;
            }
            catch (Exception ex)
            {
                Log.Write("Form_CreateView---" + ex.Message);
            }
        }

        public void OnSaveClicked(object sender)
        {
            this.Form.NetworkType = this.NetworkType;
            FormSaveResponse saveResponse;

            Task.Run(() =>
            {
                Device.BeginInvokeOnMainThread(() => IsBusy = true);

                if (this.Mode == FormMode.REF)
                    saveResponse = this.Form.SaveFormWParent(this.RowId, ParentForm.TableName);
                else
                    saveResponse = this.Form.SaveForm(this.RowId);

                Device.BeginInvokeOnMainThread(() =>
                {
                    IsBusy = false;
                    DependencyService.Get<IToast>().Show(saveResponse.Message);
                    (Application.Current.MainPage as MasterDetailPage).Detail.Navigation.PopAsync(true);
                });
            });
        }

        public void FillControls(EbDataRow row)
        {
            foreach (var pair in this.Form.ControlDictionary)
            {
                var data = row[pair.Value.Name];

                if (data != null)
                    pair.Value.SetValue(data);
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
