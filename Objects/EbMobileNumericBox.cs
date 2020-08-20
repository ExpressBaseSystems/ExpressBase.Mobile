using ExpressBase.Mobile.Behavior;
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

        public int MaxLength { get; set; }

        public int DecimalPlaces { get; set; }

        public int MaxLimit { get; set; }

        public int MinLimit { get; set; }

        public bool IsCurrency { get; set; }

        public NumericBoxTypes RenderType { get; set; }

        public bool IsAggragate { get; set; }

        private TextBox valueBox;

        private int valueBoxNumber = 0;

        public override void InitXControl(FormMode Mode, NetworkMode Network)
        {
            base.InitXControl(Mode, Network);

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

                valueBox = new TextBox { Text = valueBoxNumber.ToString(), IsReadOnly = true };
                grid.Children.Add(valueBox, 1, 0);

                var plus = new Button
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
                this.XControl = new NumericTextBox
                {
                    IsReadOnly = this.ReadOnly,
                    BgColor = this.ReadOnly ? Color.FromHex("eeeeee") : Color.White,
                    Keyboard = Keyboard.Numeric,
                    Behaviors = { new NumericBoxBehavior() }
                };
            }
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
            object value = null;
            try
            {
                if (RenderType == NumericBoxTypes.ButtonType)
                    value = valueBoxNumber.ToString();
                else
                    value = (this.XControl as NumericTextBox).Text;

                if (this.Required && Convert.ToInt32(value) <= 0)
                    value = null;
            }
            catch (Exception ex)
            {
                EbLog.Write("Numeric box getvalue error !");
                EbLog.Write(ex.Message + ex.StackTrace);
            }
            return value;
        }

        public override bool SetValue(object value)
        {
            if (value == null)
                return false;

            if (RenderType == NumericBoxTypes.ButtonType)
            {
                valueBoxNumber = Convert.ToInt32(value);
                valueBox.Text = valueBoxNumber.ToString();
            }
            else
                (this.XControl as NumericTextBox).Text = value.ToString();

            return true;
        }

        public override void Reset()
        {
            if (RenderType == NumericBoxTypes.ButtonType)
            {
                valueBox.ClearValue(TextBox.TextProperty);
                valueBoxNumber = 0;
            }
            else
                (this.XControl as NumericTextBox).ClearValue(NumericTextBox.TextProperty);
        }
    }
}
