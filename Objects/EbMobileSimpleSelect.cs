using ExpressBase.Mobile.CustomControls;
using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Extensions;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Services;
using ExpressBase.Mobile.Structures;
using ExpressBase.Mobile.Views.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;

namespace ExpressBase.Mobile
{
    /*
     Powerselect and simple select control
     */
    public class EbMobileSimpleSelect : EbMobileControl
    {
        #region Properties

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

        public bool EnablePreload { get; set; }

        public EbScript OfflineQuery { set; get; }

        public List<Param> Parameters { set; get; }

        //mobile props
        public TextBox SearchBox { set; get; }

        /// <summary>
        /// Check simple select or power select
        /// </summary>
        public bool IsSimpleSelect
        {
            get
            {
                if (string.IsNullOrEmpty(this.DataSourceRefId))
                    return true;
                return false;
            }
        }

        #endregion

        #region Local

        private ComboBoxLabel selected;

        private CustomPicker picker;

        private Color Background => this.ReadOnly ? Color.FromHex("eeeeee") : Color.Transparent;

        #endregion

        #region Methods

        public override void InitXControl(FormMode Mode, NetworkMode Network)
        {
            base.InitXControl(Mode, Network);

            TapGestureRecognizer recognizer = new TapGestureRecognizer();
            recognizer.Tapped += Icon_Tapped;

            if (IsSimpleSelect)
                InitSimpleSelect(recognizer);
            else
                InitPowerSelect(recognizer);
        }

        private void Icon_Tapped(object sender, EventArgs e)
        {
            if (IsSimpleSelect)
                picker?.Focus();
            else
                SearchBox?.Focus();
        }

        private void InitSimpleSelect(TapGestureRecognizer gesture)
        {
            picker = new CustomPicker
            {
                Title = $"Select {this.Label}",
                ItemsSource = this.Options,
                ItemDisplayBinding = new Binding("DisplayName"),
                IsEnabled = !this.ReadOnly,
                BorderColor = Color.Transparent
            };
            Label icon = new Label
            {
                Style = (Style)HelperFunctions.GetResourceValue("PSIconLabel"),
                GestureRecognizers = { gesture }
            };
            this.XControl = new InputGroup(picker, icon) { BgColor = Background };
        }

        private void InitPowerSelect(TapGestureRecognizer gesture)
        {
            SearchBox = new TextBox
            {
                IsReadOnly = this.ReadOnly,
                Placeholder = $"Search {this.Label}...",
                BorderColor = Color.Transparent
            };
            SearchBox.Focused += SearchBox_Focused;
            Label icon = new Label
            {
                Style = (Style)HelperFunctions.GetResourceValue("SSIconLabel"),
                GestureRecognizers = { gesture }
            };
            this.XControl = new InputGroup(SearchBox, icon) { BgColor = Background };
        }

        private void SearchBox_Focused(object sender, FocusEventArgs e)
        {
            PowerSelectView powerSelect = new PowerSelectView(this);
            App.RootMaster.Detail.Navigation.PushModalAsync(powerSelect);
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

        public override void SetValue(object value)
        {
            if (value != null)
            {
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
            }
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

        public override bool Validate()
        {
            return base.Validate();
        }

        public void SelectionCallback(ComboBoxLabel comboBox)
        {
            this.selected = comboBox;
            this.SearchBox.Text = comboBox.Text;
            App.RootMaster.Detail.Navigation.PopModalAsync();
        }

        private EbDataTable GetDisplayFromValue(string value)
        {
            EbDataTable dt = null;
            try
            {
                if (this.NetworkType == NetworkMode.Offline)
                {
                    dt = ResolveFromLocal(value);
                }
                else if (this.NetworkType == NetworkMode.Online)
                {
                    dt = ResolveFromLive(value);
                }
            }
            catch (Exception ex)
            {
                EbLog.Message("Error at GetDisplayFromValue in powerselect");
                EbLog.Error(ex.Message);
            }
            return dt ?? new EbDataTable();
        }

        private EbDataTable ResolveFromLocal(string value)
        {
            EbDataTable dt = null;
            try
            {
                string sql = HelperFunctions.B64ToString(this.OfflineQuery.Code).TrimEnd(';');

                string WrpdQuery = $"SELECT * FROM ({sql}) AS WR WHERE WR.{this.ValueMember.ColumnName} = {value} LIMIT 1";

                dt = App.DataDB.DoQuery(WrpdQuery);
            }
            catch (Exception ex)
            {
                EbLog.Message("power select failed to resolve display member from local");
                EbLog.Error(ex.Message);
            }
            return dt;
        }

        private EbDataTable ResolveFromLive(string value)
        {
            EbDataTable dt = null;
            try
            {
                Param p = new Param
                {
                    Name = this.ValueMember.ColumnName,
                    Type = ((int)this.ValueMember.Type).ToString(),
                    Value = value
                };

                VisualizationLiveData response = DataService.Instance.GetData(this.DataSourceRefId, new List<Param> { p }, null, 0, 0, false);

                if (response.Data != null && response.Data.Tables.HasLength(2))
                    dt = response.Data.Tables[1];
            }
            catch (Exception ex)
            {
                EbLog.Message("power select failed to resolve display member from live");
                EbLog.Error(ex.Message);
            }
            return dt;
        }

        #endregion
    }

    public class EbMobileSSOption : EbMobilePageBase
    {
        public string EbSid { get; set; }

        public override string DisplayName { get; set; }

        public string Value { get; set; }
    }
}
