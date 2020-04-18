﻿using ExpressBase.Mobile.Models;
using Xamarin.Forms;

namespace ExpressBase.Mobile.CustomControls
{
    public class CustomShadowFrame : Frame
    {
        public MobilePagesWraper PageWraper { set; get; }

        public static readonly BindableProperty RadiusProperty =
           BindableProperty.Create("Radius", typeof(float), typeof(CustomShadowFrame), 0f, BindingMode.Default);

        public static readonly BindableProperty CustomBorderColorProperty =
            BindableProperty.Create("CustomBorderColor", typeof(Color), typeof(CustomShadowFrame), default(Color), BindingMode.Default);

        public static readonly BindableProperty BorderWidthProperty =
            BindableProperty.Create("BorderWidth", typeof(int), typeof(CustomShadowFrame), default(int), BindingMode.Default);

        public static readonly BindableProperty ShadowOpacityProperty =
            BindableProperty.Create("ShadowOpacity", typeof(float), typeof(CustomShadowFrame), 0.12F, BindingMode.Default);

        public static readonly BindableProperty ShadowOffsetWidthProperty =
            BindableProperty.Create("ShadowOffsetWidth", typeof(float), typeof(CustomShadowFrame), 0f, BindingMode.Default);

        public static readonly BindableProperty ShadowOffSetHeightProperty =
            BindableProperty.Create("ShadowOffSetHeight", typeof(float), typeof(CustomShadowFrame), 4f, BindingMode.Default);

        // Gets or sets the radius of the Frame corners.
        public float Radius
        {
            get { return (float)GetValue(RadiusProperty); }
            set { this.SetValue(RadiusProperty, value); }
        }

        // Gets or sets the border color of the Frame.
        public Color CustomBorderColor
        {
            get { return (Color)GetValue(CustomBorderColorProperty); }
            set { this.SetValue(CustomBorderColorProperty, value); }
        }

        // Gets or sets the border width of the Frame.
        public int BorderWidth
        {
            get { return (int)GetValue(BorderWidthProperty); }
            set { this.SetValue(BorderWidthProperty, value); }
        }

        // Gets or sets the shadow opacity of the Frame.
        public float ShadowOpacity
        {
            get { return (float)GetValue(ShadowOpacityProperty); }
            set { this.SetValue(ShadowOpacityProperty, value); }
        }

        // Gets or sets the shadow offset width of the Frame.
        public float ShadowOffsetWidth
        {
            get { return (float)GetValue(ShadowOffsetWidthProperty); }
            set { this.SetValue(ShadowOffsetWidthProperty, value); }
        }

        // Gets or sets the shadow offset height of the Frame.
        public float ShadowOffSetHeight
        {
            get { return (float)GetValue(ShadowOffSetHeightProperty); }
            set { this.SetValue(ShadowOffSetHeightProperty, value); }
        }

        public CustomShadowFrame() { }

        public CustomShadowFrame(MobilePagesWraper wrpr)
        {
            PageWraper = wrpr;
            this.BackgroundColor = wrpr.IconBackground;
            this.Padding = 10;
            this.CornerRadius = 8;
            this.BorderColor = wrpr.IconBackground == Color.White ? Color.FromHex("fafafa") : wrpr.IconBackground;
        }
    }
}
