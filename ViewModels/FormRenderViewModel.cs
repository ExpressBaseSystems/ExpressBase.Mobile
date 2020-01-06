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
        public IList<View> Elements { set; get; }

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
            this.Elements = new List<View>();
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
                this.Elements = new List<View>();
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
                this.Elements = new List<View>();
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

        private void EbCtrlToXamCtrl(EbMobileControl ctrl, StackLayout ContentStackTop, int Margin = 10)
        {
            try
            {
                var tempstack = new StackLayout { Margin = Margin, IsVisible = !(ctrl.Hidden) };
                tempstack.Children.Add(new Label { Text = ctrl.Label });

                if (ctrl is EbMobileTableLayout)
                {
                    this.PushFromTableLayout((ctrl as EbMobileTableLayout), ContentStackTop);
                }
                else if (ctrl is EbMobileFileUpload)
                {
                    FileInput uploader = new FileInput((ctrl as EbMobileFileUpload));
                    tempstack.Children.Add(uploader.Html);
                    this.Elements.Add(uploader);
                }
                else if (ctrl is EbMobileSimpleSelect)
                {
                    PowerSelect Select = new PowerSelect(ctrl);
                    tempstack.Children.Add(Select);
                    this.Elements.Add(Select);
                }
                else
                {
                    var el = (View)Activator.CreateInstance(ctrl.XControlType, ctrl);
                    if (this.Mode == FormMode.EDIT)
                    {
                        EbDataColumn _col = this.ColumnsOnEdit.Find(item => item.ColumnName == ctrl.Name);
                        if (_col != null)
                        {
                            (el as ICustomElement).SetValue(this.RowOnEdit[_col.ColumnIndex]);
                        }
                        (el as ICustomElement).SetAsReadOnly(true);
                    }
                    tempstack.Children.Add(el);
                    this.Elements.Add(el);
                }
                ContentStackTop.Children.Add(tempstack);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void OnSaveClicked(object sender)
        {
            FormService Form = new FormService(this.Elements, this.Form, this.Mode);
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
                foreach (Element el in this.Elements)
                {
                    (el as ICustomElement).SetAsReadOnly(false);
                }
            }
        }
    }
}
