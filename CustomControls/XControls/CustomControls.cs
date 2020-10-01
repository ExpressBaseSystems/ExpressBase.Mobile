using Xamarin.Forms;

namespace ExpressBase.Mobile.CustomControls
{
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
