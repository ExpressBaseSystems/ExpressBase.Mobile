using Xamarin.Forms;

namespace ExpressBase.Mobile.CustomControls
{
    public interface IEbCustomControl
    {
        int BorderThickness { set; get; }

        float BorderRadius { set; get; }

        Color BorderColor { set; get; }

        Color XBackgroundColor { set; get; }
    }

    public class HiddenEntry : Entry
    {

    }

    public class CustomDatePicker : DatePicker, IEbCustomControl
    {
        public static readonly BindableProperty XBackgroundColorProperty =
           BindableProperty.Create(nameof(XBackgroundColor), typeof(Color), typeof(CustomDatePicker));

        public int BorderThickness { set; get; } = 1;

        public float BorderRadius { set; get; } = 10.0f;

        public Color BorderColor { set; get; } = Color.FromHex("cccccc");

        public Color XBackgroundColor
        {
            get { return (Color)GetValue(XBackgroundColorProperty); }
            set { SetValue(XBackgroundColorProperty, value); }
        }

        public CustomDatePicker() { }
    }

    public class CustomTimePicker : TimePicker, IEbCustomControl
    {
        public static readonly BindableProperty XBackgroundColorProperty =
          BindableProperty.Create(nameof(XBackgroundColor), typeof(Color), typeof(CustomTimePicker));

        public int BorderThickness { set; get; } = 1;

        public float BorderRadius { set; get; } = 10.0f;

        public Color BorderColor { set; get; } = Color.FromHex("cccccc");

        public Color XBackgroundColor
        {
            get { return (Color)GetValue(XBackgroundColorProperty); }
            set { SetValue(XBackgroundColorProperty, value); }
        }

        public CustomTimePicker() { }
    }

    public class CustomCheckBox : CheckBox, IEbCustomControl
    {
        public static readonly BindableProperty XBackgroundColorProperty =
          BindableProperty.Create(nameof(XBackgroundColor), typeof(Color), typeof(CustomCheckBox));

        public int BorderThickness { set; get; }

        public float BorderRadius { set; get; }

        public Color BorderColor { set; get; }

        public Color XBackgroundColor
        {
            get { return (Color)GetValue(XBackgroundColorProperty); }
            set { SetValue(XBackgroundColorProperty, value); }
        }

        public CustomCheckBox() { }
    }

    public class CustomPicker : Picker, IEbCustomControl
    {
        public static readonly BindableProperty XBackgroundColorProperty =
          BindableProperty.Create(nameof(XBackgroundColor), typeof(Color), typeof(CustomPicker));

        public int BorderThickness { set; get; } = 1;

        public float BorderRadius { set; get; } = 10.0f;

        public Color BorderColor { set; get; } = Color.FromHex("cccccc");

        public Color XBackgroundColor
        {
            get { return (Color)GetValue(XBackgroundColorProperty); }
            set { SetValue(XBackgroundColorProperty, value); }
        }

        public CustomPicker() { }
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

    public class ImageCircle : Image
    {

    }

    public class ListViewSearchBar : SearchBar
    {
        public bool HideIcon { set; get; }

        public ListViewSearchBar() { }
    }

    public class EbBindableUnit
    {
        public object Value { set; get; }

        public EbBindableUnit(object value)
        {
            Value = value;
        }
    }
}
