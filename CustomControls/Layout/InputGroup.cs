﻿using Xamarin.Forms;

namespace ExpressBase.Mobile.CustomControls
{
    public class InputGroup : Frame, IEbCustomControl
    {
        public static readonly BindableProperty XBackgroundColorProperty =
           BindableProperty.Create(nameof(XBackgroundColor), typeof(Color), typeof(InputGroup));

        public static readonly new BindableProperty BorderColorProperty =
           BindableProperty.Create(nameof(BorderColor), typeof(Color), typeof(InputGroup), defaultValue: Color.FromHex("cccccc"));

        public int BorderThickness { set; get; } = 1;

        public float BorderRadius { set; get; } = 10.0f;

        public new Color BorderColor
        {
            get { return (Color)GetValue(BorderColorProperty); }
            set { SetValue(BorderColorProperty, value); }
        }

        public Color XBackgroundColor
        {
            get { return (Color)GetValue(XBackgroundColorProperty); }
            set { SetValue(XBackgroundColorProperty, value); }
        }

        public View Input { set; get; }

        public View Icon { set; get; }

        public InputGroup() { }

        public InputGroup(View input, View icon)
        {
            Input = input;
            Icon = icon;
            this.Padding = 0;
            Init();
        }

        public void Init()
        {
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            grid.Children.Add(Input);
            Grid.SetColumn(Input, 0);

            grid.Children.Add(Icon);
            Grid.SetColumn(Icon, 1);
            this.Content = grid;
        }
    }
}
