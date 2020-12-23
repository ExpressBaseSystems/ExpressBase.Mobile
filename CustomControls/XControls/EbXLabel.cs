using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace ExpressBase.Mobile.CustomControls.XControls
{
    public class EbXLabel : Label
    {
        public static readonly BindableProperty BorderOnFocusProperty =
           BindableProperty.Create(nameof(BorderOnFocus), typeof(Color), typeof(EbXLabel));

        public static readonly BindableProperty XBackgroundColorProperty =
          BindableProperty.Create(nameof(XBackgroundColor), typeof(Color), typeof(EbXLabel));

        public static readonly BindableProperty BorderColorProperty =
           BindableProperty.Create(nameof(BorderColor), typeof(Color), typeof(EbXLabel), defaultValue: Color.Transparent);

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

        public EbXLabel() { }

        public EbXLabel(EbMobileDataColumn dc)
        {
            XBackgroundColor = Color.FromHex(dc.BackgroundColor ?? "#ffffff00");
            BorderColor = Color.FromHex(dc.BorderColor ?? "#ffffff00");
            BorderRadius = dc.BorderRadius;
            BorderThickness = dc.BorderThickness;
            Padding = dc.Padding == null ? 0 : dc.Padding.ConvertToXValue();

            if (dc.Width > 0) this.WidthRequest = dc.Width;
            if (dc.Height > 0) this.HeightRequest = dc.Height;
        }
    }
}
