using ExpressBase.Mobile.CustomControls;
using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Structures;
using System;
using Xamarin.Forms;

namespace ExpressBase.Mobile
{
    public class EbMobileTextBox : EbMobileControl
    {
        public override EbDbTypes EbDbType { get { return EbDbTypes.String; } set { } }

        public int MaxLength { get; set; }

        public Enums.TextTransform TextTransform { get; set; }

        public TextMode TextMode { get; set; }

        public bool AutoCompleteOff { get; set; }

        public bool AutoSuggestion { get; set; }

        public override void InitXControl(FormMode Mode, NetworkMode Network)
        {
            base.InitXControl(Mode, Network);

            if (TextMode == TextMode.MultiLine)
            {
                var textarea = new TextArea()
                {
                    IsReadOnly = this.ReadOnly,
                    BgColor = this.XBackground,
                    EnableFocus = true,
                    BorderOnFocus = App.Settings.Vendor.GetPrimaryColor()
                };
                textarea.Unfocused += TextChanged;
                this.XControl = textarea;
            }
            else
            {
                var textbox = new TextBox
                {
                    IsReadOnly = this.ReadOnly,
                    BgColor = this.XBackground,
                    EnableFocus = true,
                    BorderOnFocus = App.Settings.Vendor.GetPrimaryColor(),
                };
                textbox.Unfocused += TextChanged;
                this.XControl = textbox;
            }
        }

        private void TextChanged(object sender, FocusEventArgs e)
        {
            this.ValueChanged();
        }

        public override object GetValue()
        {
            if (TextMode == TextMode.MultiLine)
                return (this.XControl as TextArea).Text;
            else
                return (this.XControl as TextBox).Text;
        }

        public override void SetValue(object value)
        {
            if (value != null)
            {
                if (TextMode == TextMode.MultiLine)
                    (this.XControl as TextArea).Text = value.ToString();
                else
                    (this.XControl as TextBox).Text = value.ToString();
            }
        }

        public override void Reset()
        {
            if (TextMode == TextMode.MultiLine)
                (this.XControl as TextArea).ClearValue(TextBox.TextProperty);
            else
                (this.XControl as TextBox).ClearValue(TextBox.TextProperty);
        }

        public override bool Validate()
        {
            string value = GetValue() as string;

            if (Required && string.IsNullOrEmpty(value))
                return false;

            return true;
        }
    }
}
