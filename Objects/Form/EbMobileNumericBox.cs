﻿using ExpressBase.Mobile.Behavior;
using ExpressBase.Mobile.CustomControls;
using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Structures;
using System;
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

        private EbXTextBox valueBox;

        private int valueBoxNumber = 0;

        public override View Draw(FormMode Mode, NetworkMode Network)
        {
            if (RenderType == NumericBoxTypes.ButtonType)
            {
                Grid grid = new Grid { ColumnSpacing = 10, IsEnabled = !this.ReadOnly };
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                var minus = new Button
                {
                    Style = (Style)HelperFunctions.GetResourceValue("NumericBoxIncrButton"),
                    Text = "\uf068",
                    IsEnabled = !this.ReadOnly
                };
                minus.Clicked += Minus_Clicked;
                grid.Children.Add(minus, 0, 0);

                valueBox = new EbXTextBox { Text = valueBoxNumber.ToString(), IsReadOnly = true };
                valueBox.TextChanged += (sender, arg) => this.ValueChanged();

                grid.Children.Add(valueBox, 1, 0);

                Button plus = new Button
                {
                    Style = (Style)HelperFunctions.GetResourceValue("NumericBoxIncrButton"),
                    Text = "\uf067",
                    IsEnabled = !this.ReadOnly
                };
                plus.Clicked += Plus_Clicked;
                grid.Children.Add(plus, 2, 0);

                this.XControl = grid;
            }
            else
            {
                EbXNumericTextBox numeric = new EbXNumericTextBox
                {
                    IsReadOnly = this.ReadOnly,
                    XBackgroundColor = this.XBackground,
                    Keyboard = Keyboard.Numeric,
                    Behaviors = { new NumericBoxBehavior() },
                    EnableFocus = true,
                    BorderOnFocus = App.Settings.Vendor.GetPrimaryColor()
                };

                numeric.Unfocused += (sender, arg) => this.ValueChanged();
                XControl = numeric;
            }

            return base.Draw(Mode, Network);
        }

        private void Plus_Clicked(object sender, EventArgs e)
        {
            valueBoxNumber++;
            valueBox.Text = valueBoxNumber.ToString();
        }

        private void Minus_Clicked(object sender, EventArgs e)
        {
            if (valueBoxNumber == 0) return;

            valueBoxNumber--;
            valueBox.Text = valueBoxNumber.ToString();
        }

        public override object GetValue()
        {
            int value = 0;
            try
            {
                if (RenderType == NumericBoxTypes.ButtonType)
                    value = valueBoxNumber;
                else
                {
                    var text = (this.XControl as EbXNumericTextBox).Text;
                    value = Convert.ToInt32(text);
                }
            }
            catch (Exception ex)
            {
                EbLog.Info("Numeric box getvalue error !");
                EbLog.Error(ex.Message + ex.StackTrace);
            }
            return value;
        }

        public override void SetValue(object value)
        {
            if (value != null)
            {
                if (RenderType == NumericBoxTypes.ButtonType)
                {
                    valueBoxNumber = Convert.ToInt32(value);
                    valueBox.Text = valueBoxNumber.ToString();
                }
                else
                    (this.XControl as EbXNumericTextBox).Text = value.ToString();
            }
        }

        public override void SetAsReadOnly(bool disable)
        {
            base.SetAsReadOnly(disable);

            Color bg = disable ? EbMobileControl.ReadOnlyBackground : Color.Transparent;

            if (RenderType == NumericBoxTypes.TextType)
                (this.XControl as EbXNumericTextBox).XBackgroundColor = bg;
            else
                valueBox.XBackgroundColor = bg;
        }

        public override void Reset()
        {
            if (RenderType == NumericBoxTypes.ButtonType)
            {
                valueBox.ClearValue(EbXTextBox.TextProperty);
                valueBoxNumber = 0;
            }
            else
                (this.XControl as EbXNumericTextBox).ClearValue(EbXNumericTextBox.TextProperty);
        }

        public override bool Validate()
        {
            var value = this.GetValue();

            if (this.Required && Convert.ToInt32(value) <= 0)
                return false;

            return true;
        }

        public override void SetValidation(bool status, string message)
        {
            base.SetValidation(status, message);

            Color border = status ? EbMobileControl.DefaultBorder : EbMobileControl.ValidationError;

            if (RenderType == NumericBoxTypes.TextType)
                (this.XControl as EbXNumericTextBox).BorderColor = border;
            else
                valueBox.BorderColor = border;
        }
    }
}
