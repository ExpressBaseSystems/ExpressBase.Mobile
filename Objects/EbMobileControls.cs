using ExpressBase.Mobile.Behavior;
using ExpressBase.Mobile.CustomControls;
using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Structures;
using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace ExpressBase.Mobile
{
    public class EbMobileControl : EbMobilePageBase
    {
        public virtual string Label { set; get; }

        public virtual EbDbTypes EbDbType { get { return EbDbTypes.String; } set { } }

        public virtual bool Hidden { set; get; }

        public virtual bool Unique { get; set; }

        public virtual bool ReadOnly { get; set; }

        public virtual bool DoNotPersist { get; set; }

        public virtual bool Required { get; set; }

        public string SQLiteType
        {
            get
            {
                if (this.EbDbType == EbDbTypes.String)
                    return "TEXT";
                else if (this.EbDbType == EbDbTypes.Int16 || this.EbDbType == EbDbTypes.Int32)
                    return "INT";
                else if (this.EbDbType == EbDbTypes.Decimal || this.EbDbType == EbDbTypes.Double)
                    return "REAL";
                else if (this.EbDbType == EbDbTypes.Date || this.EbDbType == EbDbTypes.DateTime)
                    return "DATETIME";
                else if (this.EbDbType == EbDbTypes.Boolean)
                    return "INT";
                else
                    return "TEXT";
            }
        }

        //mobile prop
        public virtual object SQLiteToActual(object value) { return value; }

        //mobile prop
        public View XControl { set; get; }

        public virtual void InitXControl(FormMode mode, NetworkMode network)
        {
            this.FormMode = mode;
            this.NetworkType = network;
        }

        public virtual StackLayout XView
        {
            get
            {
                var formatted = new FormattedString { Spans = { new Span { Text = this.Label } } };

                if (this.Required)
                    formatted.Spans.Add(new Span { Text = " *", FontSize = 16, TextColor = Color.Red });

                return new StackLayout
                {
                    Padding = new Thickness(15, 10, 15, 10),
                    IsVisible = !(this.Hidden),
                    Children =
                    {
                        new Label { FormattedText =  formatted },
                        XControl
                    }
                };
            }
        }

        public FormMode FormMode { set; get; }

        public NetworkMode NetworkType { set; get; }

        public virtual object GetValue() { return null; }

        public virtual bool SetValue(object value) { return false; }

        public virtual void SetAsReadOnly(bool Enable)
        {
            if (Enable == true)
                this.XControl.IsEnabled = false;
            else
                this.XControl.IsEnabled = true;
        }

        public virtual void Reset() { }

        public virtual MobileTableColumn GetMobileTableColumn()
        {
            object value = this.GetValue();
            if (value == null)
                return null;

            return new MobileTableColumn
            {
                Name = this.Name,
                Type = this.EbDbType,
                Value = value
            };
        }
    }

    public class EbMobileTextBox : EbMobileControl
    {
        public override EbDbTypes EbDbType { get { return EbDbTypes.String; } set { } }

        public int MaxLength { get; set; }

        public TextTransform TextTransform { get; set; }

        public TextMode TextMode { get; set; }

        public bool AutoCompleteOff { get; set; }

        public bool AutoSuggestion { get; set; }

        public override void InitXControl(FormMode Mode, NetworkMode Network)
        {
            base.InitXControl(Mode, Network);

            Color bg = this.ReadOnly ? Color.FromHex("eeeeee") : Color.White;
            if (TextMode == TextMode.MultiLine)
            {
                XControl = new TextArea()
                {
                    HeightRequest = 100,
                    IsReadOnly = this.ReadOnly,
                    BgColor = bg
                };
            }
            else
                XControl = new TextBox
                {
                    IsReadOnly = this.ReadOnly,
                    BgColor = bg
                };
        }

        public override object GetValue()
        {
            if (TextMode == TextMode.MultiLine)
                return (this.XControl as TextArea).Text;
            else
                return (this.XControl as TextBox).Text;
        }

        public override bool SetValue(object value)
        {
            if (value == null)
                return false;

            if (TextMode == TextMode.MultiLine)
                (this.XControl as TextArea).Text = value.ToString();
            else
                (this.XControl as TextBox).Text = value.ToString();

            return true;
        }

        public override void Reset()
        {
            if (TextMode == TextMode.MultiLine)
                (this.XControl as TextArea).ClearValue(TextBox.TextProperty);
            else
                (this.XControl as TextBox).ClearValue(TextBox.TextProperty);
        }
    }

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

        private readonly Style ButtonStyles = new Style(typeof(Button))
        {
            Setters =
            {
                new Setter{ Property = Button.VerticalOptionsProperty,Value = LayoutOptions.Center },
                new Setter{ Property = Button.PaddingProperty,Value = 0 },
                new Setter{ Property = Button.HeightRequestProperty,Value = 40 },
                new Setter{ Property = Button.WidthRequestProperty,Value = 40 },
                new Setter{ Property = Button.CornerRadiusProperty,Value = 20 },
                new Setter{ Property = Button.BorderColorProperty,Value = Color.FromHex("cccccc") },
                new Setter{ Property = Button.BackgroundColorProperty,Value = Color.FromHex("eeeeee") },
                new Setter{ Property = Button.BorderWidthProperty,Value = 1 },
                new Setter
                {
                    Property = Button.FontFamilyProperty,
                    Value = (OnPlatform<string>)HelperFunctions.GetResourceValue("FontAwesome")
                },
            }
        };

        private TextBox ValueBox { set; get; }

        private int ValueBoxNumber { set; get; } = 0;

        public override void InitXControl(FormMode Mode, NetworkMode Network)
        {
            base.InitXControl(Mode, Network);

            if (RenderType == NumericBoxTypes.ButtonType)
            {
                Grid grid = new Grid { ColumnSpacing = 10, IsEnabled = !this.ReadOnly };
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                var minus = new Button { Style = ButtonStyles, Text = "\uf068", IsEnabled = !this.ReadOnly };
                minus.Clicked += Minus_Clicked;
                grid.Children.Add(minus, 0, 0);

                ValueBox = new TextBox { Text = ValueBoxNumber.ToString(), IsReadOnly = true };
                grid.Children.Add(ValueBox, 1, 0);

                var plus = new Button { Style = ButtonStyles, Text = "\uf067", IsEnabled = !this.ReadOnly };
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
            ValueBoxNumber++;
            ValueBox.Text = ValueBoxNumber.ToString();
        }

        private void Minus_Clicked(object sender, EventArgs e)
        {
            if (ValueBoxNumber == 0) return;

            ValueBoxNumber--;
            ValueBox.Text = ValueBoxNumber.ToString();
        }

        public override object GetValue()
        {
            if (RenderType == NumericBoxTypes.ButtonType)
                return ValueBoxNumber.ToString();
            else
                return (this.XControl as NumericTextBox).Text;
        }

        public override bool SetValue(object value)
        {
            if (value == null)
                return false;

            if (RenderType == NumericBoxTypes.ButtonType)
            {
                ValueBoxNumber = Convert.ToInt32(value);
                ValueBox.Text = ValueBoxNumber.ToString();
            }
            else
                (this.XControl as NumericTextBox).Text = value.ToString();

            return true;
        }

        public override void Reset()
        {
            if (RenderType == NumericBoxTypes.ButtonType)
            {
                ValueBox.ClearValue(TextBox.TextProperty);
                ValueBoxNumber = 0;
            }
            else
                (this.XControl as NumericTextBox).ClearValue(NumericTextBox.TextProperty);
        }
    }

    public class EbMobileDateTime : EbMobileControl
    {
        public EbDateType EbDateType { get; set; }

        public override EbDbTypes EbDbType { get { return (EbDbTypes)this.EbDateType; } set { } }

        public bool IsNullable { get; set; }

        public TimeShowFormat ShowTimeAs_ { get; set; }

        public DateShowFormat ShowDateAs_ { get; set; }

        public CustomDatePicker Picker { set; get; }

        public override object SQLiteToActual(object value)
        {
            if (this.EbDbType == EbDbTypes.Date)
                return Convert.ToDateTime(value).Date.ToString("yyyy-MM-dd");
            else if (this.EbDbType == EbDbTypes.DateTime)
                return Convert.ToDateTime(value).Date.ToString("yyyy-MM-dd HH:mm:ss");

            return value.ToString();
        }

        public override void InitXControl(FormMode Mode, NetworkMode Network)
        {
            base.InitXControl(Mode, Network);

            var bg = this.ReadOnly ? Color.FromHex("eeeeee") : Color.Transparent;

            Picker = new CustomDatePicker
            {
                IsEnabled = !this.ReadOnly,
                Date = DateTime.Now,
                Format = "yyyy-MM-dd",
                BorderColor = Color.Transparent
            };

            var icon = new Label
            {
                Padding = 10,
                FontSize = 18,
                VerticalOptions = LayoutOptions.Center,
                Text = "\uf073",
                FontFamily = (OnPlatform<string>)HelperFunctions.GetResourceValue("FontAwesome")
            };

            this.XControl = new InputGroup(Picker, icon) { BgColor = bg };
        }

        public override object GetValue()
        {
            return Picker.Date.ToString("yyyy-MM-dd");
        }

        public override bool SetValue(object value)
        {
            if (value == null)
                return false;
            Picker.Date = Convert.ToDateTime(value);
            return true;
        }

        public override void Reset()
        {
            Picker.ClearValue(CustomDatePicker.DateProperty);
        }
    }

    public class EbMobileBoolean : EbMobileControl
    {
        public override EbDbTypes EbDbType { get { return EbDbTypes.BooleanOriginal; } set { } }

        public override object SQLiteToActual(object value)
        {
            if (Convert.ToInt32(value) == 0)
                return false;
            else
                return true;
        }

        public override void InitXControl(FormMode Mode,NetworkMode Network)
        {
            base.InitXControl(Mode, Network);

            this.XControl = new CheckBox();
        }

        public override object GetValue()
        {
            return (this.XControl as CheckBox).IsChecked;
        }

        public override bool SetValue(object value)
        {
            if (value == null)
                return false;

            int val = Convert.ToInt32(value);
            (this.XControl as CheckBox).IsChecked = (val == 0) ? false : true;
            return true;
        }

        public override void Reset()
        {
            (this.XControl as CheckBox).ClearValue(CheckBox.IsCheckedProperty);
        }
    }

    public class EbMobileTableLayout : EbMobileControl
    {
        public EbMobileTableLayout()
        {
            this.CellCollection = new List<EbMobileTableCell>();
        }

        public int RowCount { set; get; }

        public int ColumCount { set; get; }

        public List<EbMobileTableCell> CellCollection { set; get; }

        public override bool Hidden { set; get; }

        public override bool Unique { get; set; }
    }

    public class EbMobileTableCell : EbMobilePageBase
    {
        public EbMobileTableCell()
        {
            this.ControlCollection = new List<EbMobileControl>();
        }

        public int RowIndex { set; get; }

        public int ColIndex { set; get; }

        public int Width { set; get; }

        public List<EbMobileControl> ControlCollection { set; get; }
    }

    public class EbMobileDataColumn : EbMobileControl
    {
        public string TextFormat { get; set; }

        public int TableIndex { get; set; }

        public int ColumnIndex { get; set; }

        public string ColumnName { get; set; }

        public EbDbTypes Type { get; set; }

        public EbFont Font { get; set; }
    }

    public class EbMobileAutoId : EbMobileControl
    {
        public override bool ReadOnly { get { return true; } }

        public override EbDbTypes EbDbType { get { return EbDbTypes.String; } set { } }

        public override void InitXControl(FormMode Mode,NetworkMode Network)
        {
            base.InitXControl(Mode, Network);

            this.XControl = new TextBox
            {
                IsReadOnly = true,
                BgColor = Color.FromHex("eeeeee")
            };
        }

        public override bool SetValue(object value)
        {
            if (value == null)
                return false;

            (this.XControl as TextBox).Text = value.ToString();
            return true;
        }
    }
}
