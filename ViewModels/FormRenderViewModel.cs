using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Models;
using System;
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
            try
            {
                Form = (Page.Container as EbMobileForm);
                CreateView();
                this.Form.CreateTableSchema();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
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
                this.Form = (Page.Container as EbMobileForm);
                PageTitle = Page.DisplayName;

                this.Mode = FormMode.EDIT;
                this.RowId = Convert.ToInt32(this.RowOnEdit["id"]);
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
        public FormRenderViewModel(EbMobilePage CurrentForm, EbMobilePage ParentPage, EbDataRow CurrentRow)
        {
            SaveButtonVisible = true;
            EditButtonVisible = false;
            this.Mode = FormMode.REF;
            ParentForm = (ParentPage.Container as EbMobileForm);
            RowOnEdit = CurrentRow;
            try
            {
                this.Form = (CurrentForm.Container as EbMobileForm);
                PageTitle = CurrentForm.DisplayName;

                this.CreateView();
                this.Form.CreateTableSchema();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
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
                    ctrl.InitXControl();

                    if (this.Mode == FormMode.EDIT)
                    {
                        EbDataColumn _col = this.ColumnsOnEdit[ctrl.Name];
                        if (_col != null)
                        {
                            ctrl.SetValue(this.RowOnEdit[_col.ColumnIndex]);
                        }
                        else if (ctrl is EbMobileFileUpload)
                        {
                            (ctrl as EbMobileFileUpload).RenderOnEdit(this.Form.TableName, Convert.ToInt32(this.RowOnEdit["id"]));
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
                status = this.Form.SaveWithParentId(this.RowOnEdit, ParentForm.TableName);
            else
                status = this.Form.Save(this.RowId);

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
