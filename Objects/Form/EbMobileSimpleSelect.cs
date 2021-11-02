using ExpressBase.Mobile.Constants;
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

        public bool AutoFill { get; set; }

        public EbScript OfflineQuery { set; get; }

        public List<Param> Parameters { set; get; }

        public EbXTextBox SearchBox { set; get; }

        public bool IsSimpleSelect => string.IsNullOrEmpty(this.DataSourceRefId);

        private ComboBoxLabel selected;

        private EbXPicker picker;

        private Color Background => this.ReadOnly ? Color.FromHex("eeeeee") : Color.Transparent;

        public override View Draw(FormMode Mode, NetworkMode Network)
        {
            TapGestureRecognizer recognizer = new TapGestureRecognizer();
            recognizer.Tapped += OnIconClicked;

            if (IsSimpleSelect)
                InitSimpleSelect(recognizer);
            else
            {
                InitPowerSelect(recognizer);
                if (this.AutoFill) this.AutoFillValue();
            }
            return base.Draw(Mode, Network);
        }

        private void OnIconClicked(object sender, EventArgs e)
        {
            if (this.ReadOnly) return;

            if (IsSimpleSelect)
                picker?.Focus();
            else
                SearchBox?.Focus();
        }

        private void InitSimpleSelect(TapGestureRecognizer gesture)
        {
            picker = new EbXPicker
            {
                Title = $"Select {this.Label}",
                ItemsSource = this.Options,
                ItemDisplayBinding = new Binding("DisplayName"),
                IsEnabled = !this.ReadOnly,
                BorderColor = Color.Transparent
            };
            picker.SelectedIndexChanged += (sender, e) => this.ValueChanged();

            Label icon = new Label
            {
                Style = (Style)HelperFunctions.GetResourceValue("PSIconLabel"),
                GestureRecognizers = { gesture }
            };

            XControl = new InputGroup(picker, icon) { XBackgroundColor = Background, HasShadow = false };
        }

        private void InitPowerSelect(TapGestureRecognizer gesture)
        {
            SearchBox = new EbXTextBox
            {
                IsReadOnly = this.ReadOnly,
                Placeholder = $"Search {this.Label}...",
                FontSize = 15,
                BorderColor = Color.Transparent
            };
            SearchBox.Focused += async (sender, args) => await OnSearchBoxFocused(); ;
            Label icon = new Label
            {
                Style = (Style)HelperFunctions.GetResourceValue("SSIconLabel"),
                GestureRecognizers = { gesture }
            };

            XControl = new InputGroup(SearchBox, icon) { XBackgroundColor = Background };
        }

        private async Task OnSearchBoxFocused()
        {
            await App.Navigation.NavigateModalByRenderer(new PowerSelectView(this));
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

        public object GetDisplayValue()
        {
            if (!IsSimpleSelect && this.selected != null)
            {
                return this.selected.Text;
            }
            return null;
        }

        public override void SetValue(object value)
        {
            if (value == null) return;

            if (IsSimpleSelect)
                picker.SelectedItem = this.Options.Find(i => i.Value == value.ToString());
            else
                this.SetPowerSelect(value.ToString());
        }

        public override void Reset()
        {
            if (IsSimpleSelect)
                picker.ClearValue(EbXPicker.SelectedItemProperty);
            else
            {
                this.selected = null;
                SearchBox.ClearValue(EbXTextBox.TextProperty);
            }
        }

        public override bool Validate() => base.Validate();

        public void SelectionCallback(ComboBoxLabel comboBox)
        {
            this.selected = comboBox;
            this.SearchBox.Text = comboBox.Text;
            this.ValueChanged();

            App.Navigation.PopModalByRenderer(true);
        }

        private async void SetPowerSelect(string value)
        {
            EbDataTable dt = null;

            if (this.NetworkType == NetworkMode.Offline)
            {
                dt = GetDataFromLocal(value);
            }
            else if (this.NetworkType == NetworkMode.Online)
            {
                Param param = new Param
                {
                    Name = this.ValueMember.ColumnName,
                    Type = ((int)this.ValueMember.Type).ToString(),
                    Value = value
                };
                dt = await GetDataFromCloud(new List<Param> { param });
            }

            if (dt != null && dt.Rows.Any())
            {
                SetPowerSelectValue(dt.Rows[0]);
            }
        }

        private void SetPowerSelectValue(EbDataRow row)
        {
            string displayMember = row[DisplayMember.ColumnName]?.ToString();
            object valueMember = row[ValueMember.ColumnName];

            selected = new ComboBoxLabel
            {
                Text = displayMember,
                Value = valueMember,
                Row = row
            };
            SearchBox.Text = displayMember;
        }

        private async void AutoFillValue()
        {
            try
            {
                EbDataTable dt = await GetDataFromCloud(null);

                if (dt != null && dt.Rows.Any())
                {
                    SetPowerSelectValue(dt.Rows[0]);
                }
            }
            catch (Exception ex)
            {
                EbLog.Error("Error at [AutoFill] : " + ex.Message);
            }
        }

        private EbDataTable GetDataFromLocal(string value)
        {
            try
            {
                string sql = HelperFunctions.B64ToString(this.OfflineQuery.Code).TrimEnd(CharConstants.SEMICOLON);

                string WrpdQuery = $"SELECT * FROM ({sql}) AS WR WHERE WR.{this.ValueMember.ColumnName} = {value} LIMIT 1";

                return App.DataDB.DoQuery(WrpdQuery);
            }
            catch (Exception ex)
            {
                EbLog.Info("power select failed to resolve display member from local");
                EbLog.Error(ex.Message);
            }
            return null;
        }

        private async Task<EbDataTable> GetDataFromCloud(List<Param> parameters)
        {
            try
            {
                MobileDataResponse response = await DataService.Instance.GetDataAsync(this.DataSourceRefId, 1, 0, parameters, null, null, false);

                if (response.Data != null && response.Data.Tables.HasLength(2))
                {
                    return response.Data.Tables[1];
                }
            }
            catch (Exception ex)
            {
                EbLog.Info("power select failed to resolve display member from live");
                EbLog.Error(ex.Message);
            }
            return null;
        }

        public override void SetAsReadOnly(bool disable)
        {
            base.SetAsReadOnly(disable);

            Color bg = disable ? EbMobileControl.ReadOnlyBackground : Color.Transparent;

            (this.XControl as InputGroup).XBackgroundColor = bg;
        }

        public override void SetValidation(bool status, string message)
        {
            base.SetValidation(status, message);

            Color border = status ? EbMobileControl.DefaultBorder : EbMobileControl.ValidationError;

            (this.XControl as InputGroup).BorderColor = border;
        }

        public object GetDisplayName4DG(string valueMember)
        {
            EbDataTable dt = null;

            if (this.NetworkType == NetworkMode.Offline)
            {
                dt = GetDataFromLocal(valueMember);
            }
            else if (this.NetworkType == NetworkMode.Online)
            {
                Param param = new Param
                {
                    Name = this.ValueMember.ColumnName,
                    Type = ((int)this.ValueMember.Type).ToString(),
                    Value = valueMember
                };
                try
                {
                    MobileDataResponse response = DataService.Instance.GetData(this.DataSourceRefId, 1, 0, new List<Param> { param }, null, null, false);

                    if (response.Data != null && response.Data.Tables.HasLength(2))
                    {
                        dt = response.Data.Tables[1];
                    }
                }
                catch (Exception ex)
                {
                    EbLog.Info("power select failed to resolve display member from live");
                    EbLog.Error(ex.Message);
                }
            }

            if (dt != null && dt.Rows.Any())
            {
                return dt.Rows[0][DisplayMember.ColumnName];
            }
            return null;
        }

        public object getColumn(string colname)
        {
            if (this.selected != null)
            {
                return this.selected?.Row[colname];
            }
            return null;
        }
    }

    public class EbMobileSSOption : EbMobilePageBase
    {
        public string EbSid { get; set; }

        public override string DisplayName { get; set; }

        public string Value { get; set; }
    }
}
