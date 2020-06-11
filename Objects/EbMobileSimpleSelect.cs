using ExpressBase.Mobile.CustomControls;
using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Services;
using ExpressBase.Mobile.Structures;
using ExpressBase.Mobile.Views.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
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
                        return this.ValueMember.Type;
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
        public TextBox SearchBox { set; get; }

        public bool IsSimpleSelect
        {
            get
            {
                if (string.IsNullOrEmpty(this.DataSourceRefId))
                    return true;
                return false;
            }
        }

        private ComboBoxLabel selected;

        private CustomPicker picker;

        public override void InitXControl(FormMode Mode, NetworkMode Network)
        {
            base.InitXControl(Mode, Network);

            var bg = this.ReadOnly ? Color.FromHex("eeeeee") : Color.Transparent;

            if (IsSimpleSelect)
            {
                picker = new CustomPicker
                {
                    Title = $"Select {this.Label}",
                    FontSize = 15,
                    TextColor = Color.FromHex("333942"),
                    ItemsSource = this.Options,
                    ItemDisplayBinding = new Binding("DisplayName"),
                    IsEnabled = !this.ReadOnly,
                    BorderColor = Color.Transparent
                };
                var icon = new Label
                {
                    Style = (Style)HelperFunctions.GetResourceValue("PSIconLabel")
                };
                this.XControl = new InputGroup(picker, icon) { BgColor = bg };
            }
            else
            {
                SearchBox = new TextBox
                {
                    IsReadOnly = this.ReadOnly,
                    Placeholder = $"Seach {this.Label}...",
                    FontSize = 14,
                    BorderColor = Color.Transparent
                };
                SearchBox.Focused += SearchBox_Focused;
                var icon = new Label
                {
                    Style = (Style)HelperFunctions.GetResourceValue("SSIconLabel")
                };
                this.XControl = new InputGroup(SearchBox, icon) { BgColor = bg };
            }
        }

        private void SearchBox_Focused(object sender, FocusEventArgs e)
        {
            var selectView = new PowerSelectView(this);
            (Application.Current.MainPage as MasterDetailPage).Detail.Navigation.PushModalAsync(selectView);
        }

        public override object GetValue()
        {
            if (IsSimpleSelect)
            {
                if (picker.SelectedItem != null)
                    return (picker.SelectedItem as EbMobileSSOption).Value;
                else
                    return null;
            }
            else
                return this.selected?.Value;
        }

        public override bool SetValue(object value)
        {
            if (value == null)
                return false;

            if (IsSimpleSelect)
                picker.SelectedItem = this.Options.Find(i => i.Value == value.ToString());
            else
            {
                EbDataTable dt = this.GetDisplayFromValue(value.ToString());
                if (dt != null && dt.Rows.Any())
                {
                    var display_membertext = dt.Rows[0][DisplayMember.ColumnName].ToString();
                    selected = new ComboBoxLabel { Text = display_membertext, Value = value };
                    SearchBox.Text = display_membertext;
                }
            }
            return true;
        }

        public override void Reset()
        {
            if (IsSimpleSelect)
                picker.ClearValue(CustomPicker.SelectedItemProperty);
            else
            {
                this.selected = null;
                SearchBox.ClearValue(TextBox.TextProperty);
            }
        }

        public void SelectionCallback(ComboBoxLabel comboBox)
        {
            this.selected = comboBox;
            this.SearchBox.Text = comboBox.Text;
            (Application.Current.MainPage as MasterDetailPage).Detail.Navigation.PopModalAsync();
        }

        private EbDataTable GetDisplayFromValue(string value)
        {
            EbDataTable dt = null;
            try
            {
                if (this.NetworkType == NetworkMode.Offline)
                {
                    string sql = HelperFunctions.B64ToString(this.OfflineQuery.Code).TrimEnd(';');

                    string WrpdQuery = $"SELECT * FROM ({sql}) AS WR WHERE WR.{this.ValueMember.ColumnName} = {value} LIMIT 1";

                    dt = App.DataDB.DoQuery(WrpdQuery);
                }
                else if (this.NetworkType == NetworkMode.Online)
                {
                    Param p = new Param
                    {
                        Name = this.ValueMember.ColumnName,
                        Type = ((int)this.ValueMember.Type).ToString(),
                        Value = value
                    };
                    var response = DataService.Instance.GetData(this.DataSourceRefId, new List<Param> { p }, null, 0, 0, false);

                    if (response.Data != null && response.Data.Tables.Count >= 2)
                        dt = response.Data.Tables[1];
                }
            }
            catch (Exception ex)
            {
                EbLog.Write($"*********Exception on geting display value*********");

                EbLog.Write($"DisplayMember:{this.DisplayMember.ColumnName}");
                EbLog.Write($"ValueMember:{this.ValueMember.ColumnName}" + ex.Message);
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
