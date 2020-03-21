using ExpressBase.Mobile.CustomControls;
using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Services;
using ExpressBase.Mobile.Structures;
using ExpressBase.Mobile.Views.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ExpressBase.Mobile
{
    public class EbMobileSimpleSelect : EbMobileControl
    {
        public override EbDbTypes EbDbType
        {
            get
            {
                if (!string.IsNullOrEmpty(DataSourceRefId))
                {
                    if (this.ValueMember != null)
                        return this.ValueMember.EbDbType;
                    else
                        return EbDbTypes.String;
                }
                else
                    return EbDbTypes.String;
            }
            set { }
        }

        public List<EbMobileSSOption> Options { set; get; }

        public bool IsMultiSelect { get; set; }

        public string DataSourceRefId { get; set; }

        public List<EbMobileDataColumn> Columns { set; get; }

        public EbMobileDataColumn DisplayMember { set; get; }

        public EbMobileDataColumn ValueMember { set; get; }

        public int MinSearchLength { set; get; } = 3;

        public EbScript OfflineQuery { set; get; }

        public List<Param> Parameters { set; get; }

        //mobile props
        public CustomSearchBar SearchBox { set; get; }

        public ComboBoxLabel Selected { set; get; }

        public override void InitXControl(FormMode Mode)
        {
            if (string.IsNullOrEmpty(this.DataSourceRefId))
            {
                this.XControl = new CustomPicker
                {
                    Title = "Select",
                    TitleColor = Color.DarkBlue,
                    ItemsSource = this.Options,
                    ItemDisplayBinding = new Binding("DisplayName")
                };
            }
            else
            {
                SearchBox = new CustomSearchBar
                {
                    Placeholder = "Seach " + this.Label + "...",
                    FontSize = 14
                };

                SearchBox.Focused += SearchBox_Focused;
                this.XControl = SearchBox;
            }
        }

        private void SearchBox_Focused(object sender, FocusEventArgs e)
        {
            var selectView = new PowerSelectView(this);
            (Application.Current.MainPage as MasterDetailPage).Detail.Navigation.PushModalAsync(selectView);
        }

        public override object GetValue()
        {
            if (string.IsNullOrEmpty(this.DataSourceRefId))
            {
                if ((this.XControl as CustomPicker).SelectedItem != null)
                    return ((this.XControl as CustomPicker).SelectedItem as EbMobileSSOption).Value;
                else
                    return null;
            }
            else
                return this.Selected?.Value;
        }

        public override bool SetValue(object value)
        {
            if (value == null)
                return false;

            if (string.IsNullOrEmpty(this.DataSourceRefId))
                (this.XControl as CustomPicker).SelectedItem = this.Options.Find(i => i.Value == value.ToString());
            else
            {
                EbDataTable dt = this.GetDisplayFromValue(value.ToString());
                if (dt.Rows.Any())
                {
                    var display_membertext = dt.Rows[0][DisplayMember.ColumnName].ToString();
                    Selected = new ComboBoxLabel { Text = display_membertext, Value = value };
                    SearchBox.Text = display_membertext;
                }
            }
            return true;
        }

        public void SelectionCallback(ComboBoxLabel comboBox)
        {
            this.Selected = comboBox;
            this.SearchBox.Text = comboBox.Text;
            (Application.Current.MainPage as MasterDetailPage).Detail.Navigation.PopModalAsync();
        }

        private EbDataTable GetDisplayFromValue(string value)
        {
            EbDataTable dt;
            try
            {
                string sql = HelperFunctions.B64ToString(this.OfflineQuery.Code).TrimEnd(';');

                string WrpdQuery = $"SELECT * FROM ({sql}) AS WR WHERE WR.{this.ValueMember.ColumnName} = {value} LIMIT 1";

                dt = App.DataDB.DoQuery(WrpdQuery);
            }
            catch (Exception ex)
            {
                Log.Write(ex.Message);
                dt = new EbDataTable();
            }
            return dt;
        }      
    }

    public class EbMobileSSOption : EbMobilePageBase
    {
        public string EbSid { get; set; }

        public override string DisplayName { get; set; }

        public string Value { get; set; }
    }
}
