using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ExpressBase.Mobile.ViewModels
{
    public class FormRenderViewModel : BaseViewModel
    {
        private EbMobileForm Form { set; get; }

        private Grid _dyView { set; get; }

        private FormMode Mode { set; get; } = FormMode.NEW;

        public Grid View
        {
            get { return _dyView; }
            set { _dyView = value; }
        }

        private bool _saveButtonVisible;
        public bool SaveButtonVisible
        {
            get { return this._saveButtonVisible; }
            set
            {
                this._saveButtonVisible = value;
                this.NotifyPropertyChanged();
            }
        }

        private bool _editButtonVisible;
        public bool EditButtonVisible
        {
            get { return this._editButtonVisible; }
            set
            {
                this._editButtonVisible = value;
                this.NotifyPropertyChanged();
            }
        }

        private int RowId { set; get; } = 0;

        public Command EnableEditCommand { set; get; }

        public EbMobileForm ParentForm { set; get; }

        public EbDataTable DataOnEdit { set; get; }

        //new mode
        public FormRenderViewModel(EbMobilePage Page)
        {
            SaveButtonVisible = true;
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
            SaveButtonVisible = false;
            EditButtonVisible = true;
            try
            {
                this.RowId = RowIdLocal;
                this.Form = (Page.Container as EbMobileForm);
                PageTitle = Page.DisplayName;

                this.DataOnEdit = GetDataOnEdit();

                this.CreateView();
                this.Form.CreateTableSchema();

                EnableEditCommand = new Command(EnableEditClick);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        //referenced mode
        public FormRenderViewModel(EbMobilePage Page, EbMobilePage ParentPage, int ParentId)
        {
            SaveButtonVisible = true;
            EditButtonVisible = false;
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
            View = new Grid()
            {
                RowDefinitions =
                {
                    new RowDefinition { Height = GridLength.Star },
                    new RowDefinition { Height = GridLength.Auto }
                }
            };

            ScrollView InnerScroll = new ScrollView { Orientation = ScrollOrientation.Vertical };
            StackLayout ScrollStack = new StackLayout { Spacing = 0 };

            foreach (var ctrl in this.Form.ChiledControls)
            {
                this.EbCtrlToXamCtrl(ctrl, ScrollStack);
            }

            InnerScroll.Content = ScrollStack;
            View.Children.Add(InnerScroll);

            Button btn = new Button
            {
                Text = (this.Mode == FormMode.EDIT) ? "Save Changes" : "Save",
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.EndAndExpand,
                BackgroundColor = Color.FromHex("#508bf9"),
                TextColor = Color.White,
                Command = new Command(OnSaveClicked)
            };
            btn.SetBinding(Button.IsVisibleProperty, new Binding("SaveButtonVisible"));

            View.Children.Add(btn);
            Grid.SetRow(btn, 1);
        }

        private void EbCtrlToXamCtrl(EbMobileControl ctrl, StackLayout ContentStackTop)
        {
            try
            {
                if (ctrl is EbMobileTableLayout)
                {
                    this.PushFromTableLayout((ctrl as EbMobileTableLayout), ContentStackTop);
                }
                else
                {
                    ctrl.InitXControl(this.Mode);

                    if (this.Mode == FormMode.EDIT)
                    {
                        EbDataColumn _col = this.DataOnEdit.Columns[ctrl.Name];
                        if (_col != null)
                        {
                            ctrl.SetValue(this.DataOnEdit.Rows[0][_col.ColumnIndex]);
                        }
                        else if (ctrl is EbMobileFileUpload)
                        {
                            (ctrl as EbMobileFileUpload).RenderOnEdit(this.Form.TableName, this.RowId);
                        }
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

        private void EnableEditClick(object sender)
        {
            if (!SaveButtonVisible)
            {
                Task.Run(() => { Device.BeginInvokeOnMainThread(() => SaveButtonVisible = true); });
                foreach (EbMobileControl Ctrl in this.Form.FlatControls)
                {
                    Ctrl.SetAsReadOnly(false);
                }
            }
        }
    }
}
