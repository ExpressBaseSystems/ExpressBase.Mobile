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

        public EbXLabel(IMobileUIControl uiControl)
        {
            if(!string.IsNullOrEmpty(uiControl.BackgroundColor))
                XBackgroundColor = Color.FromHex(uiControl.BackgroundColor);

            if (!string.IsNullOrEmpty(uiControl.BorderColor))
                BorderColor = Color.FromHex(uiControl.BorderColor);

            BorderRadius = uiControl.BorderRadius;
            BorderThickness = uiControl.BorderThickness;
            Padding = uiControl.Padding == null ? 0 : uiControl.Padding.ConvertToXValue();

            if (uiControl.Width > 0) WidthRequest = uiControl.Width;
            if (uiControl.Height > 0) HeightRequest = uiControl.Height;
        }
    }
}
