﻿using ExpressBase.Mobile.CustomControls;
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
        public TextBox SearchBox { set; get; }

        public ComboBoxLabel Selected { set; get; }

        public CustomPicker Picker { set; get; }

        public override void InitXControl(FormMode Mode)
        {
            var bg = this.ReadOnly ? Color.FromHex("eeeeee") : Color.Transparent;

            if (string.IsNullOrEmpty(this.DataSourceRefId))
            {
                Picker = new CustomPicker
                {
                    Title = $"Select {this.Label}",
                    FontSize = 15,
                    TextColor = (Color)HelperFunctions.GetResourceValue("Gray-900"),
                    ItemsSource = this.Options,
                    ItemDisplayBinding = new Binding("DisplayName"),
                    IsEnabled = !this.ReadOnly,
                    BorderColor = Color.Transparent
                };
                var icon = new Label
                {
                    Padding = 10,
                    FontSize = 16,
                    VerticalOptions = LayoutOptions.Center,
                    Text = "\uf078",
                    FontFamily = (OnPlatform<string>)HelperFunctions.GetResourceValue("FontAwesome")
                };
                this.XControl = new InputGroup(Picker, icon) { BgColor = bg };
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
                    Padding = 10,
                    FontSize = 16,
                    VerticalOptions = LayoutOptions.Center,
                    Text = "\uf002",
                    FontFamily = (OnPlatform<string>)HelperFunctions.GetResourceValue("FontAwesome")
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
            if (string.IsNullOrEmpty(this.DataSourceRefId))
            {
                if (Picker.SelectedItem != null)
                    return (Picker.SelectedItem as EbMobileSSOption).Value;
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
                Picker.SelectedItem = this.Options.Find(i => i.Value == value.ToString());
            else
            {
                EbDataTable dt = this.GetDisplayFromValue(value.ToString());
                if (dt != null && dt.Rows.Any())
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
                    var response = RestServices.Instance.PullReaderData(this.DataSourceRefId, new List<Param> { p }, 0, 0, false);

                    if (response.Data != null && response.Data.Tables.Count >= 2)
                        dt = response.Data.Tables[1];
                }
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
