using ExpressBase.Mobile.CustomControls;
using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Services;
using ExpressBase.Mobile.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ExpressBase.Mobile.ViewModels
{
    public class FormRenderViewModel : BaseViewModel
    {
        public IList<XCustomControl> XControls { set; get; }

        private EbMobileForm Form { set; get; }

        private Grid _dyView { set; get; }

        private FormMode Mode { set; get; } = FormMode.NEW;

        public Grid View
        {
            get
            {
                return _dyView;
            }
            set
            {
                _dyView = value;
            }
        }

        private bool _saveButtonVisible;
        public bool SaveButtonVisible
        {
            get
            {
                return this._saveButtonVisible;
            }
            set
            {
                if (this._saveButtonVisible == value)
                {
                    return;
                }
                this._saveButtonVisible = value;
                this.NotifyPropertyChanged();
            }
        }

        private bool _editButtonVisible;
        public bool EditButtonVisible
        {
            get
            {
                return this._editButtonVisible;
            }
            set
            {
                if (this._editButtonVisible == value)
                {
                    return;
                }
                this._editButtonVisible = value;
                this.NotifyPropertyChanged();
            }
        }

        private EbDataRow RowOnEdit { set; get; }

        private ColumnColletion ColumnsOnEdit { set; get; }

        private int RowId { set; get; } = 0;

        public Command EnableEditCommand { set; get; }

        public EbMobileForm ParentForm { set; get; }

        //new mode
        public FormRenderViewModel(EbMobilePage Page)
        {
            SaveButtonVisible = true;
            PageTitle = Page.DisplayName;
            Form = (Page.Container as EbMobileForm);
            this.XControls = new List<XCustomControl>();
            CreateView();

            //create tables or alter table
            this.CreateSchema();
        }

        //edit mode
        public FormRenderViewModel(EbMobilePage Page, EbDataRow CurrentRow, ColumnColletion Columns)
        {
            SaveButtonVisible = false;
            EditButtonVisible = true;

            this.RowOnEdit = CurrentRow;
            this.ColumnsOnEdit = Columns;
            try
            {
                this.XControls = new List<XCustomControl>();
                this.Form = (Page.Container as EbMobileForm);
                PageTitle = Page.DisplayName;

                this.Mode = FormMode.EDIT;
                this.RowId = Convert.ToInt32(this.RowOnEdit["id"]);
                this.CreateView();
                this.CreateSchema();

                EnableEditCommand = new Command(EnableEditClick);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        //referenced mode
        public FormRenderViewModel(EbMobilePage CurrentForm, EbMobilePage ParentPage, EbDataRow CurrentRow)
        {
            SaveButtonVisible = true;
            EditButtonVisible = false;
            this.Mode = FormMode.REF;
            ParentForm = (ParentPage.Container as EbMobileForm);
            RowOnEdit = CurrentRow;
            try
            {
                this.XControls = new List<XCustomControl>();
                this.Form = (CurrentForm.Container as EbMobileForm);
                PageTitle = CurrentForm.DisplayName;

                this.CreateView();
                this.CreateSchema();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void CreateView()
        {
            View = new Grid();
            View.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });
            View.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

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
                    XCustomControl XCtrl = (XCustomControl)Activator.CreateInstance(ctrl.XControlType, ctrl);

                    if (this.Mode == FormMode.EDIT)
                    {
                        EbDataColumn _col = this.ColumnsOnEdit.Find(item => item.ColumnName == ctrl.Name);
                        if (_col != null)
                        {
                            XCtrl.SetValue(this.RowOnEdit[_col.ColumnIndex]);
                        }
                        XCtrl.SetAsReadOnly(true);
                    }

                    this.XControls.Add(XCtrl);
                    ContentStackTop.Children.Add(XCtrl.XView);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void OnSaveClicked(object sender)
        {
            FormService Form = new FormService(this.XControls, this.Form, this.Mode);
            bool status = false;
            if (this.Mode == FormMode.REF)
                status = Form.Save(this.RowOnEdit, ParentForm.TableName);
            else
                status = Form.Save(this.RowId);

            if (status && this.RowId == 0)
            {
                DependencyService.Get<IToast>().Show("Data pushed successfully :)");
                (Application.Current.MainPage as MasterDetailPage).Detail.Navigation.PopAsync(true);
            }
            else if (status && this.RowId > 0)
            {
                DependencyService.Get<IToast>().Show("Changes saved successfully :)");
                (Application.Current.MainPage as MasterDetailPage).Detail.Navigation.PopToRootAsync(true);
            }
            else
            {
                DependencyService.Get<IToast>().Show("Something went wrong!");
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

        private void CreateSchema()
        {
            SQLiteTableSchema Schema = this.GetSQLiteSchema(this.Form.ChiledControls);
            Schema.TableName = this.Form.TableName;
            new CommonServices().CreateLocalTable4Form(Schema);
        }

        SQLiteTableSchema GetSQLiteSchema(List<EbMobileControl> Controls)
        {
            SQLiteTableSchema Schema = new SQLiteTableSchema();

            foreach (EbMobileControl ctrl in Controls)
            {
                if (ctrl is EbMobileTableLayout || ctrl is EbMobileFileUpload)
                {
                    continue;
                }
                else
                {
                    Schema.Columns.Add(new SQLiteColumSchema
                    {
                        ColumnName = ctrl.Name,
                        ColumnType = ctrl.SQLiteType
                    });
                }
            }
            Schema.AppendDefault();//eb_colums

            return Schema;
        }

        private void EnableEditClick(object sender)
        {
            if (!SaveButtonVisible)
            {
                Task.Run(() => { Device.BeginInvokeOnMainThread(() => SaveButtonVisible = true); });
                foreach (XCustomControl XCtrl in this.XControls)
                {
                    XCtrl.SetAsReadOnly(false);
                }
            }
        }
    }
}
