using ExpressBase.Mobile.Helpers;
using System.Collections.Generic;
using Xamarin.Forms;

namespace ExpressBase.Mobile.CustomControls
{

    public interface IEbCustomControl
    {
        int BorderThickness { set; get; }

        float BorderRadius { set; get; }

        string BorderColor { set; get; }
    }

    public class TextBox : Entry, IEbCustomControl
    {
        public int BorderThickness { set; get; }

        public float BorderRadius { set; get; }

        public string BorderColor { set; get; }

        public TextBox() { }
    }

    public class TextArea : Editor, IEbCustomControl
    {
        public int BorderThickness { set; get; }

        public float BorderRadius { set; get; }

        public string BorderColor { set; get; }

        public TextArea() { }
    }

    public class NumericTextBox : Entry, IEbCustomControl
    {
        public int BorderThickness { set; get; }

        public float BorderRadius { set; get; }

        public string BorderColor { set; get; }

        public NumericTextBox() { }
    }

    public class CustomDatePicker : DatePicker, IEbCustomControl
    {
        public int BorderThickness { set; get; }

        public float BorderRadius { set; get; }

        public string BorderColor { set; get; }

        public CustomDatePicker() { }
    }

    public class CustomCheckBox : CheckBox, IEbCustomControl
    {
        public int BorderThickness { set; get; }

        public float BorderRadius { set; get; }

        public string BorderColor { set; get; }

        public CustomCheckBox() { }
    }

    public class CustomPicker : Picker, IEbCustomControl
    {
        public int BorderThickness { set; get; }

        public float BorderRadius { set; get; }

        public string BorderColor { set; get; }

        public CustomPicker() { }
    }

    public class CustomSearchBar : SearchBar, IEbCustomControl
    {
        public int BorderThickness { set; get; }

        public float BorderRadius { set; get; }

        public string BorderColor { set; get; }

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

        public CustomImageWraper()
        {

        }

        public CustomImageWraper(string name)
        {
            Name = name;
            ClassId = name;
        }
    }

    public class InputGroup : Frame, IEbCustomControl
    {
        public int BorderThickness { set; get; }

        public float BorderRadius { set; get; }

        public new string BorderColor { set; get; }

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
