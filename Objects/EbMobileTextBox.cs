using ExpressBase.Mobile.CustomControls;
using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Structures;
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
                var textarea = new EbXTextArea()
                {
                    IsReadOnly = this.ReadOnly,
                    XBackgroundColor = this.XBackground,
                    EnableFocus = true,
                    BorderOnFocus = App.Settings.Vendor.GetPrimaryColor()
                };
                textarea.Unfocused += TextChanged;
                this.XControl = textarea;
            }
            else
            {
                var textbox = new EbXTextBox
                {
                    IsReadOnly = this.ReadOnly,
                    XBackgroundColor = this.XBackground,
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
                return (this.XControl as EbXTextArea).Text;
            else
                return (this.XControl as EbXTextBox).Text;
        }

        public override void SetValue(object value)
        {
            if (value != null)
            {
                if (TextMode == TextMode.MultiLine)
                    (this.XControl as EbXTextArea).Text = value.ToString();
                else
                    (this.XControl as EbXTextBox).Text = value.ToString();
            }
        }

        public override void Reset()
        {
            if (TextMode == TextMode.MultiLine)
                (this.XControl as EbXTextArea).ClearValue(EbXTextBox.TextProperty);
            else
                (this.XControl as EbXTextBox).ClearValue(EbXTextBox.TextProperty);
        }

        public override bool Validate()
        {
            string value = GetValue() as string;

            if (Required && string.IsNullOrEmpty(value))
                return false;

            return true;
        }

        public override void SetAsReadOnly(bool disable)
        {
            base.SetAsReadOnly(disable);

            Color bg = disable ? EbMobileControl.ReadOnlyBackground : Color.Transparent;

            if (TextMode == TextMode.MultiLine)
                (this.XControl as EbXTextArea).XBackgroundColor = bg;
            else
                (this.XControl as EbXTextBox).XBackgroundColor = bg;
        }

        public override void SetValidation(bool status, string message)
        {
            base.SetValidation(status, message);

            Color border = status ? EbMobileControl.DefaultBorder : EbMobileControl.ValidationError;

            if (TextMode == TextMode.MultiLine)
                (this.XControl as EbXTextArea).BorderColor = border;
            else
                (this.XControl as EbXTextBox).BorderColor = border;
        }
    }
}
