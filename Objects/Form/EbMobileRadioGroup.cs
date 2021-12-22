using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Structures;
using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace ExpressBase.Mobile
{
    public class EbMobileRadioGroup : EbMobileControl
    {
        public override EbDbTypes EbDbType { get { return EbDbTypes.String; } set { } }

        public EbMobileRGValueType ValueType { get; set; }

        public List<EbMobileRGOption> Options { get; set; }

        public bool HorizontalAlign { get; set; }

        public EbMobileRadioGroup() { }

        public override View Draw(FormMode Mode, NetworkMode Network)
        {
            IList<View> children;
            if (this.HorizontalAlign)
            {
                XControl = new FlexLayout();
                children = (XControl as FlexLayout).Children;
            }
            else
            {
                XControl = new StackLayout();
                children = (XControl as StackLayout).Children;
            }

            string gpname = "rgopt_" + Guid.NewGuid().ToString("N");
            foreach (EbMobileRGOption opt in this.Options)
            {
                StackLayout inner = new StackLayout()
                {
                    Orientation = Xamarin.Forms.StackOrientation.Horizontal,
                    Padding = new Thickness(0, 0, 10, 0)
                };
                RadioButton btn = new RadioButton()
                {
                    GroupName = gpname,
                    TextColor = (Color)HelperFunctions.GetResourceValue("Primary_Color")
                };
                btn.CheckedChanged += (sender, arg) => { if (arg.Value) this.ValueChanged(); };

                TapGestureRecognizer recognizer = new TapGestureRecognizer();
                recognizer.Tapped += OnLabelClicked;
                Label label = new Label()
                {
                    Text = opt.DisplayName,
                    VerticalOptions = new LayoutOptions() { Alignment = LayoutAlignment.Center },
                    GestureRecognizers = { recognizer },
                    BindingContext = btn
                };

                opt.XButton = btn;
                inner.Children.Add(btn);
                inner.Children.Add(label);
                children.Add(inner);
            }
            return base.Draw(Mode, Network);
        }

        private void OnLabelClicked(object sender, EventArgs e)
        {
            Label label = (Label)sender;
            RadioButton btn = label.BindingContext as RadioButton;
            btn.IsChecked = true;
        }

        public override object GetValue()
        {
            foreach (EbMobileRGOption opt in this.Options)
            {
                if (opt.XButton.IsChecked)
                    return opt.Value;
            }
            return null;
        }

        public override void SetValue(object value)
        {
            foreach (EbMobileRGOption opt in this.Options)
            {
                if (Convert.ToString(value) == opt.Value)
                {
                    opt.XButton.IsChecked = true;
                }
            }
        }

        public override void Reset()
        {
            foreach (EbMobileRGOption opt in this.Options)
            {
                if (opt.XButton.IsChecked)
                    opt.XButton.IsChecked = false;
            }
        }

        public override bool Validate()
        {
            foreach (EbMobileRGOption opt in this.Options)
            {
                if (opt.XButton.IsChecked)
                    return true;
            }
            return false;
        }
    }

    public enum EbMobileRGValueType
    {
        Integer = 11,
        String = 16
    }

    public class EbMobileRGOption : EbMobilePageBase
    {
        public string EbSid { get; set; }

        public override string DisplayName { get; set; }

        public string Value { get; set; }

        public RadioButton XButton { get; set; }
    }
}
