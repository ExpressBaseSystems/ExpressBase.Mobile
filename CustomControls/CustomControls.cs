using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Helpers;
using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace ExpressBase.Mobile.CustomControls
{
    public interface IDynamicContent
    {
        Dictionary<string, string> PageContent { get; }

        void SetContentFromConfig();
    }

    public interface IRefreshable
    {
        void Refreshed();

        void UpdateRenderStatus();

        bool CanRefresh();
    }

    public interface IEbCustomControl
    {
        int BorderThickness { set; get; }

        float BorderRadius { set; get; }

        Color BorderColor { set; get; }

        Color BgColor { set; get; }
    }

    public class HiddenEntry : Entry
    {

    }

    public class TextBox : Entry, IEbCustomControl
    {
        public int BorderThickness { set; get; } = 1;

        public float BorderRadius { set; get; } = 10.0f;

        public Color BorderColor { set; get; } = Color.FromHex("cccccc");

        public Color BgColor { set; get; }

        public TextBox() { }
    }

    public class TextArea : Editor, IEbCustomControl
    {
        public int BorderThickness { set; get; } = 1;

        public float BorderRadius { set; get; } = 10.0f;

        public Color BorderColor { set; get; } = Color.FromHex("cccccc");

        public Color BgColor { set; get; }

        public TextArea() { }
    }

    public class NumericTextBox : Entry, IEbCustomControl
    {
        public int BorderThickness { set; get; } = 1;

        public float BorderRadius { set; get; } = 10.0f;

        public Color BorderColor { set; get; } = Color.FromHex("cccccc");

        public Color BgColor { set; get; }

        public NumericTextBox() { }
    }

    public class CustomDatePicker : DatePicker, IEbCustomControl
    {
        public int BorderThickness { set; get; } = 1;

        public float BorderRadius { set; get; } = 10.0f;

        public Color BorderColor { set; get; } = Color.FromHex("cccccc");

        public Color BgColor { set; get; }

        public CustomDatePicker() { }
    }

    public class CustomTimePicker : TimePicker, IEbCustomControl
    {
        public int BorderThickness { set; get; } = 1;

        public float BorderRadius { set; get; } = 10.0f;

        public Color BorderColor { set; get; } = Color.FromHex("cccccc");

        public Color BgColor { set; get; }

        public CustomTimePicker() { }
    }

    public class CustomCheckBox : CheckBox, IEbCustomControl
    {
        public int BorderThickness { set; get; }

        public float BorderRadius { set; get; }

        public Color BorderColor { set; get; }

        public Color BgColor { set; get; }

        public CustomCheckBox() { }
    }

    public class CustomPicker : Picker, IEbCustomControl
    {
        public int BorderThickness { set; get; } = 1;

        public float BorderRadius { set; get; } = 10.0f;

        public Color BorderColor { set; get; } = Color.FromHex("cccccc");

        public Color BgColor { set; get; }

        public CustomPicker() { }
    }

    public class CustomSearchBar : SearchBar, IEbCustomControl
    {
        public int BorderThickness { set; get; } = 1;

        public float BorderRadius { set; get; } = 10.0f;

        public Color BorderColor { set; get; } = Color.FromHex("cccccc");

        public Color BgColor { set; get; }

        public CustomSearchBar() { }
    }

    public class ComboBoxLabel : Label
    {
        public object Value { set; get; }

        public ComboBoxLabel() { }

        public ComboBoxLabel(int index)
        {
            this.Padding = new Thickness(5);

            if (index % 2 == 0)
            {
                this.BackgroundColor = Color.FromHex("ecf0f1");
            }
        }
    }

    public class CustomImageWraper : AbsoluteLayout
    {
        public string Name { set; get; }

        public CustomImageWraper() { }

        public CustomImageWraper(string name)
        {
            Name = name;
            ClassId = name;
            Margin = new Thickness(5);
        }

        public CustomImageWraper(string name, double width)
        {
            Name = name;
            ClassId = name;
            WidthRequest = width;
            Margin = new Thickness(5);
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);
            this.HeightRequest = width;
        }
    }

    public class InputGroup : Frame, IEbCustomControl
    {
        public int BorderThickness { set; get; } = 1;

        public float BorderRadius { set; get; } = 10.0f;

        public new Color BorderColor { set; get; } = Color.FromHex("cccccc");

        public Color BgColor { set; get; }

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

    public class ImageCircle : Image
    {

    }

    public class LSImageButton : ImageButton
    {
        public double InitialWidth { set; get; }
    }
}
