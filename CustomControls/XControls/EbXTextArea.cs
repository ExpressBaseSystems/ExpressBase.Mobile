﻿using Xamarin.Forms;

namespace ExpressBase.Mobile.CustomControls
{
    public class EbXTextArea : Editor, IEbCustomControl
    {
        public static readonly BindableProperty BorderOnFocusProperty =
            BindableProperty.Create(nameof(BorderOnFocus), typeof(Color), typeof(EbXTextArea));

        public static readonly BindableProperty XBackgroundColorProperty =
          BindableProperty.Create(nameof(XBackgroundColor), typeof(Color), typeof(EbXTextArea));

        public static readonly BindableProperty BorderColorProperty =
           BindableProperty.Create(nameof(BorderColor), typeof(Color), typeof(EbXTextArea), defaultValue: Color.FromHex("cccccc"));

        public int BorderThickness { set; get; } = 1;

        public float BorderRadius { set; get; } = 10.0f;

        public Color BorderColor
        {
            get { return (Color)GetValue(BorderColorProperty); }
            set { SetValue(BorderColorProperty, value); }
        }

        public Color XBackgroundColor
        {
            get { return (Color)GetValue(XBackgroundColorProperty); }
            set { SetValue(XBackgroundColorProperty, value); }
        }

        public bool EnableFocus { set; get; }

        public Color BorderOnFocus
        {
            get { return (Color)GetValue(BorderOnFocusProperty); }
            set { SetValue(BorderOnFocusProperty, value); }
        }

        public EbXTextArea() { }
    }
}
