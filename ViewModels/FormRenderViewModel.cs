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
using Xamarin.Forms;

namespace ExpressBase.Mobile.DynamicRenders
{
    public class FormRenderViewModel : BaseViewModel
    {
        public IList<Element> Elements { set; get; }

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

        private MobVisRenderType RenderType { set; get; } = MobVisRenderType.Normal;

        private EbDataRow RowOnEdit { set; get; }

        private ColumnColletion ColumnsOnEdit { set; get; }

        private int RowId { set; get; } = 0;

        //new mode
        public FormRenderViewModel(EbMobilePage Page)
        {
            PageTitle = Page.DisplayName;
            Form = (Page.Container as EbMobileForm);
            this.Elements = new List<Element>();
            CreateView();

            //create tables or alter table
            this.CreateSchema();
        }

        //edit mode
        public FormRenderViewModel(EbMobileVisualization Visualization, EbDataRow CurrentRow, ColumnColletion Columns)
        {
            this.RenderType = Visualization.RenderAs;
            this.RowOnEdit = CurrentRow;
            this.ColumnsOnEdit = Columns;
            try
            {
                this.Elements = new List<Element>();
                EbMobilePage _page = HelperFunctions.GetPage(Visualization.LinkRefId);
                this.Form = (_page.Container as EbMobileForm);
                PageTitle = _page.DisplayName;

                if (RenderType == MobVisRenderType.Link)
                {
                    this.Mode = FormMode.EDIT;
                    this.RowId = Convert.ToInt32(this.RowOnEdit["id"]);
                    this.CreateView();
                    this.CreateSchema();
                }
                else if (RenderType == MobVisRenderType.Referencing)
                {

                }
                else
                {
                    throw new Exception("link settings not defined");
                }
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

            View.Children.Add(btn);
            Grid.SetRow(btn, 1);
        }

        private void EbCtrlToXamCtrl(EbMobileControl ctrl, StackLayout ContentStackTop, int Margin = 10)
        {
            var tempstack = new StackLayout { Margin = Margin };
            tempstack.Children.Add(new Label { Text = ctrl.Label });

            if (ctrl is EbMobileTableLayout)
            {
                this.PushFromTableLayout((ctrl as EbMobileTableLayout), ContentStackTop);
            }
            else if (ctrl is EbMobileFileUpload)
            {
                //FileInput uploader = new FileInput((ctrl as EbMobileFileUpload));
                //tempstack.Children.Add(uploader.Html);
                //this.Elements.Add(uploader);
            }
            else
            {
                var el = (View)Activator.CreateInstance(ctrl.XControlType, ctrl);
                if (this.RenderType == MobVisRenderType.Link)
                {
                    EbDataColumn _col = this.ColumnsOnEdit.Find(item => item.ColumnName == ctrl.Name);
                    if (_col != null)
                    {
                        (el as ICustomElement).SetValue(this.RowOnEdit[_col.ColumnIndex]);
                    }
                }
                tempstack.Children.Add(el);
                this.Elements.Add(el);
            }

            ContentStackTop.Children.Add(tempstack);
        }

        public void OnSaveClicked(object sender)
        {
            FormService Form = new FormService(this.Elements, this.Form);
            bool status = Form.Save(this.RowId);
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
    }
}
