using ExpressBase.Mobile.Behavior;
using ExpressBase.Mobile.CustomControls;
using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;

namespace ExpressBase.Mobile
{
    public class EbMobileNumericBox : EbMobileControl
    {
        public override EbDbTypes EbDbType { get { return EbDbTypes.Decimal; } set { } }

        public int DecimalPlaces { get; set; }

        public int MaxLimit { get; set; }

        public int MinLimit { get; set; }

        public NumericBoxTypes RenderType { get; set; }

        public bool AllowNegative { get; set; }

        public List<EbMobileNumBoxButtons> IncrementButtons { get; set; }

        private EbXTextBox XValueBox;

        private string decimalPadding;

        public override View Draw(FormMode Mode, NetworkMode Network)
        {
            decimalPadding = this.DecimalPlaces > 0 ? ".".PadRight(this.DecimalPlaces + 1, '0') : string.Empty;
            if (RenderType == NumericBoxTypes.ButtonType)
            {
                Grid grid = new Grid { ColumnSpacing = 4, IsEnabled = !this.ReadOnly };

                if (this.IncrementButtons == null || this.IncrementButtons.Count == 0)
                    this.IncrementButtons = new List<EbMobileNumBoxButtons>() { new EbMobileNumBoxButtons() { Value = -1 }, new EbMobileNumBoxButtons() { Value = 1 } };

                this.IncrementButtons = this.IncrementButtons.OrderBy(e => e.Value).ToList();
                Int16 index = 0;

                foreach (EbMobileNumBoxButtons btn in this.IncrementButtons)
                {
                    if (btn.Value > 0)
                        break;
                    this.AddIncrButton(grid, btn.Value, index++, "- ");
                }

                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
                XValueBox = new EbXTextBox
                {
                    Text = this.MinLimit + decimalPadding,
                    Keyboard = Keyboard.Numeric,
                    HorizontalTextAlignment = TextAlignment.Center,
                    IsReadOnly = this.ReadOnly,
                    XBackgroundColor = this.XBackground,
                    EnableFocus = true,
                    BorderOnFocus = App.Settings.Vendor.GetPrimaryColor()
                };
                XValueBox.TextChanged += this.NumericValueChanged;
                XValueBox.Focused += this.OnFocused;
                XValueBox.Unfocused += this.OnUnFocused;

                grid.Children.Add(XValueBox, index++, 0);
                foreach (EbMobileNumBoxButtons btn in this.IncrementButtons)
                {
                    if (btn.Value < 0)
                        continue;
                    this.AddIncrButton(grid, btn.Value, index++, "+ ");
                }

                this.XControl = grid;
            }
            else
            {
                EbXNumericTextBox numeric = new EbXNumericTextBox
                {
                    Text = "0" + decimalPadding,
                    IsReadOnly = this.ReadOnly,
                    XBackgroundColor = this.XBackground,
                    Keyboard = Keyboard.Numeric,
                    Behaviors = { new NumericBoxBehavior() },
                    EnableFocus = true,
                    BorderOnFocus = App.Settings.Vendor.GetPrimaryColor(),
                    HorizontalTextAlignment = TextAlignment.End
                };
                numeric.TextChanged += this.NumericValueChanged;
                numeric.Focused += this.OnFocused;
                numeric.Unfocused += this.OnUnFocused;

                this.XControl = numeric;
            }

            return base.Draw(Mode, Network);
        }

        private void OnFocused(object sender, FocusEventArgs arg)
        {
            Entry txtBox = sender as Entry;
            if (decimal.TryParse(txtBox.Text, out decimal res) && res == 0)
                txtBox.Text = string.Empty;
        }

        private void OnUnFocused(object sender, FocusEventArgs arg)
        {
            Entry txtBox = sender as Entry;
            SetValue(txtBox.Text);
        }

        private void AddIncrButton(Grid grid, int val, int i, string prefix)
        {
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            Button btn = new Button
            {
                //Style = (Style)HelperFunctions.GetResourceValue("NumericBoxIncrButton"),
                WidthRequest = 40,
                HeightRequest = 40,
                CornerRadius = 4,
                Padding = 0,
                BackgroundColor = App.Settings.Vendor.GetPrimaryColor(),
                TextColor = App.Settings.Vendor.GetPrimaryFontColor(),
                Text = prefix + Math.Abs(val),
                IsEnabled = !this.ReadOnly
            };
            btn.BindingContext = val;
            btn.Clicked += IncrBtn_Clicked;
            grid.Children.Add(btn, i++, 0);
        }

        private void NumericValueChanged(object sender, TextChangedEventArgs arg)
        {
            if (arg.OldTextValue == arg.NewTextValue || DoNotPropagateChange)
                return;
            decimal.TryParse(arg.OldTextValue, out decimal _old);
            decimal.TryParse(arg.NewTextValue, out decimal _new);
            if (_old == _new && _new == 0)
                return;
            if (CanSetValue(_new))
            {
                if (TrySetText(arg.NewTextValue))
                    ValueChanged();
            }
            else
                TrySetText(arg.OldTextValue);
        }

        //set valid numeric value without formatting
        public bool TrySetText(string value)
        {
            decimal _t, afterFormat;
            decimal.TryParse(value, out _t);
            string strval = GetDisplayValue(_t);
            decimal.TryParse(strval, out afterFormat);

            if (_t == afterFormat)
                return false;

            if (RenderType == NumericBoxTypes.ButtonType)
                XValueBox.Text = strval;
            else
                (XControl as EbXNumericTextBox).Text = strval;
            return true;
        }

        private void IncrBtn_Clicked(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            int val = Convert.ToInt32(btn.BindingContext);
            decimal value = val + Convert.ToDecimal(XValueBox.Text);
            if (CanSetValue(value))
            {
                SetValue(value);
                ValueChanged();
            }
        }

        public override object GetValue()
        {
            decimal value = 0;
            try
            {
                if (RenderType == NumericBoxTypes.ButtonType)
                {
                    value = Convert.ToDecimal(XValueBox.Text);
                }
                else
                {
                    value = Convert.ToDecimal((XControl as EbXNumericTextBox).Text);
                }
            }
            catch (Exception ex)
            {
                EbLog.Info("Numeric box getvalue error !");
                EbLog.Error(ex.Message + ex.StackTrace);
            }
            return value;
        }

        private bool CanSetValue(decimal _num)
        {
            bool flag = true;
            if (!AllowNegative && _num < 0)
                flag = false;
            if (MinLimit != 0 && MinLimit > _num)
                flag = false;
            if (MaxLimit != 0 && MaxLimit < _num)
                flag = false;
            return flag;
        }

        public override void SetValue(object value)
        {
            decimal _t;
            decimal.TryParse(Convert.ToString(value), out _t);
            if (!CanSetValue(_t))
                return;

            string strval = GetDisplayValue(_t);

            if (RenderType == NumericBoxTypes.ButtonType)
                XValueBox.Text = strval;
            else
                (XControl as EbXNumericTextBox).Text = strval;
        }

        public override void SetAsReadOnly(bool disable)
        {
            base.SetAsReadOnly(disable);

            Color bg = disable ? EbMobileControl.ReadOnlyBackground : Color.Transparent;

            if (RenderType == NumericBoxTypes.TextType)
                (this.XControl as EbXNumericTextBox).XBackgroundColor = bg;
            else
                XValueBox.XBackgroundColor = bg;
        }

        public override void Reset()
        {
            if (RenderType == NumericBoxTypes.ButtonType)
                XValueBox.ClearValue(EbXTextBox.TextProperty);
            else
                (this.XControl as EbXNumericTextBox).ClearValue(EbXNumericTextBox.TextProperty);
        }

        public override bool Validate()
        {
            var value = this.GetValue();

            if (this.Required && Convert.ToDecimal(value) <= 0)
                return false;

            return true;
        }

        public override void SetValidation(bool status, string message)
        {
            base.SetValidation(status, message);

            Color border = status ? EbMobileControl.DefaultBorder : EbMobileControl.ValidationError;

            if (RenderType == NumericBoxTypes.TextType)
                (XControl as EbXNumericTextBox).BorderColor = border;
            else
                XValueBox.BorderColor = border;
        }

        public override string GetDisplayName4DG(object valueMember)
        {
            return GetDisplayValue(valueMember);
        }

        private string GetDisplayValue(object value)
        {
            double _t;
            if (decimalPadding == null)
                decimalPadding = DecimalPlaces > 0 ? ".".PadRight(DecimalPlaces + 1, '0') : string.Empty;
            double.TryParse(Convert.ToString(value), out _t);
            string strval = string.Format("{0:0" + decimalPadding + "}", _t);
            return strval;
        }
    }

    public class EbMobileNumBoxButtons : EbMobilePageBase
    {
        public string EbSid { get; set; }

        public int Value { set; get; }
    }
}
