using ExpressBase.Mobile.CustomControls;
using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Extensions;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Services;
using ExpressBase.Mobile.ViewModels.BaseModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ExpressBase.Mobile.ViewModels.Dynamic
{
    public class FormRenderViewModel : DynamicBaseViewModel
    {
        public EbMobileForm Form { set; get; }

        public FormMode Mode { set; get; }

        private int RowId { set; get; } = 0;

        public EbMobileForm ParentForm { set; get; }

        public EbDataSet DataOnEdit { set; get; }

        public Dictionary<string, List<FileMetaInfo>> FilesOnEdit { set; get; }

        public Command SaveCommand => new Command(async () => await OnSaveClicked());

        //new mode
        public FormRenderViewModel(EbMobilePage page) : base(page)
        {
            this.Mode = FormMode.NEW;
            try
            {
                this.Form = this.Page.Container as EbMobileForm;
                this.CreateView();
                this.DeployTables();
            }
            catch (Exception ex)
            {
                Log.Write("Form render new mode---" + ex.Message);
            }
        }

        //edit
        public FormRenderViewModel(EbMobilePage page, int rowId) : base(page)
        {
            this.Mode = FormMode.EDIT;
            try
            {
                this.RowId = rowId;
                this.Form = page.Container as EbMobileForm;

                this.CreateView();
                this.DeployTables();
            }
            catch (Exception ex)
            {
                Log.Write("Form render edit mode---" + ex.Message);
            }
        }

        //prefill mode
        public FormRenderViewModel(EbMobilePage page, EbDataRow currentRow) : base(page)
        {
            this.Mode = FormMode.PREFILL;
            try
            {
                this.Form = page.Container as EbMobileForm;

                this.CreateView();
                this.FillControlsFlat(currentRow);
                this.DeployTables();
            }
            catch (Exception ex)
            {
                Log.Write("Form render prefill mode---" + ex.Message);
            }
        }

        //referenced mode
        public FormRenderViewModel(EbMobilePage page, EbMobilePage parentPage, int parentId) : base(page)
        {
            this.Mode = FormMode.REF;
            ParentForm = (parentPage.Container as EbMobileForm);
            try
            {
                this.RowId = parentId;
                this.Form = page.Container as EbMobileForm;
                this.CreateView();
                this.DeployTables();
            }
            catch (Exception ex)
            {
                Log.Write("Form render reference mode" + ex.Message);
            }
        }

        public void DeployTables()
        {
            Task.Run(() =>
            {
                if (this.NetworkType != NetworkMode.Online)
                    this.Form.CreateTableSchema();
            });
        }

        private void CreateView()
        {
            try
            {
                StackLayout ScrollStack = new StackLayout { Spacing = 0 };

                foreach (EbMobileControl ctrl in this.Form.ChildControls)
                {
                    if (ctrl is EbMobileTableLayout)
                    {
                        foreach (EbMobileTableCell Tc in (ctrl as EbMobileTableLayout).CellCollection)
                        {
                            foreach (EbMobileControl tbctrl in Tc.ControlCollection)
                            {
                                this.Draw(ScrollStack, tbctrl);
                            }
                        }
                    }
                    else
                    {
                        this.Draw(ScrollStack, ctrl);
                    }
                }
                this.XView = ScrollStack;
            }
            catch (Exception ex)
            {
                Log.Write("Form_CreateView---" + ex.Message);
            }
        }

        private void Draw(StackLayout container, EbMobileControl ctrl)
        {
            try
            {
                ctrl.InitXControl(this.Mode, this.NetworkType);
                container.Children.Add(ctrl.XView);
                this.Form.ControlDictionary.Add(ctrl.Name, ctrl);
            }
            catch (Exception ex)
            {
                Log.Write(ex.Message);
            }
        }

        public async Task SetDataOnEdit()
        {
            try
            {
                if (this.Page.NetworkMode == NetworkMode.Offline)
                {
                    EbDataSet ds = new EbDataSet();

                    EbDataTable masterData = App.DataDB.DoQuery($"SELECT * FROM {this.Form.TableName} WHERE id = {this.RowId};");
                    masterData.TableName = this.Form.TableName;
                    ds.Tables.Add(masterData);

                    foreach (var pair in this.Form.ControlDictionary)
                    {
                        if (pair.Value is ILinesEnabled)
                        {
                            string linesQuery = $"SELECT * FROM {(pair.Value as ILinesEnabled).TableName} WHERE {this.Form.TableName}_id = {this.RowId};";
                            EbDataTable linesData = App.DataDB.DoQuery(linesQuery);
                            linesData.TableName = (pair.Value as ILinesEnabled).TableName;
                            ds.Tables.Add(linesData);
                        }
                    }
                    this.DataOnEdit = ds;
                }
                else if (this.Page.NetworkMode == NetworkMode.Online)
                {
                    if (string.IsNullOrEmpty(this.Form.WebFormRefId))
                        throw new Exception("webform refid is empty");

                    WebformData data = await RestServices.Instance.PullFormDataAsync(this.Page.RefId, this.RowId, Settings.LocationId);
                    this.DataOnEdit = data.ToDataSet();
                    this.FilesOnEdit = data.ToFilesMeta();
                }
            }
            catch (Exception e)
            {
                Log.Write("form_SetDataOnEdit---" + e.Message);
                this.DataOnEdit = new EbDataSet();
            }
        }

        public async Task OnSaveClicked()
        {
            IToast toast = DependencyService.Get<IToast>();

            if (this.NetworkType == NetworkMode.Online && !Settings.HasInternet)
                toast.Show("you are not connected to Internet");

            if (Form.Validate())
            {
                await Task.Run(() =>
                {
                    this.Form.NetworkType = this.NetworkType;
                    FormSaveResponse saveResponse;

                    Device.BeginInvokeOnMainThread(() => IsBusy = true);

                    if (this.Mode == FormMode.REF)
                        saveResponse = this.Form.SaveFormWParent(this.RowId, ParentForm.TableName);
                    else if (this.Mode == FormMode.EDIT)
                        saveResponse = this.Form.SaveForm(this.RowId);
                    else
                        saveResponse = this.Form.SaveForm(0);

                    Device.BeginInvokeOnMainThread(() =>
                    {
                        IsBusy = false;
                        toast.Show(saveResponse.Message);
                        (Application.Current.MainPage as MasterDetailPage).Detail.Navigation.PopAsync(true);
                    });
                });
            }
            else
                toast.Show("Fields required");
        }

        public void FillControlsValues()
        {
            try
            {
                EbDataTable masterData = DataOnEdit.Tables.Find(table => table.TableName == this.Form.TableName);
                if (masterData == null && !masterData.Rows.Any()) return;
                EbDataRow masterRow = masterData.Rows.FirstOrDefault();

                foreach (KeyValuePair<string, EbMobileControl> pair in this.Form.ControlDictionary)
                {
                    object data = masterRow[pair.Value.Name];

                    if (pair.Value is EbMobileFileUpload)
                    {
                        var fup = new FUPSetValueMeta
                        {
                            TableName = this.Form.TableName,
                            RowId = this.RowId
                        };

                        if (this.FilesOnEdit != null && this.FilesOnEdit.ContainsKey(pair.Value.Name))
                        {
                            fup.Files.AddRange(this.FilesOnEdit[pair.Value.Name]);
                        }
                        pair.Value.SetValue(fup);
                    }
                    else if (pair.Value is ILinesEnabled)
                    {
                        EbDataTable lines = DataOnEdit.Tables.Find(table => table.TableName == (pair.Value as ILinesEnabled).TableName);
                        pair.Value.SetValue(lines);
                    }
                    else
                        pair.Value.SetValue(data);

                    pair.Value.SetAsReadOnly(true);
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex.Message);
            }
        }

        private void FillControlsFlat(EbDataRow row)
        {
            try
            {
                foreach (KeyValuePair<string, EbMobileControl> pair in this.Form.ControlDictionary)
                {
                    object data = row[pair.Value.Name];
                    if (pair.Value is INonPersistControl || pair.Value is ILinesEnabled) continue;
                    else
                        pair.Value.SetValue(data);
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex.Message);
            }
        }
    }
}
