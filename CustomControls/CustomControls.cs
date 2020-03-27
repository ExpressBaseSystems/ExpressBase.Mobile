using System.Collections.Generic;
using Xamarin.Forms;

namespace ExpressBase.Mobile.CustomControls
{
    public class TextBox : Entry
    {
        public int BorderThickness { set; get; } = 1;

        public float BorderRadius { set; get; } = 10.0f;

        public string BorderColor { set; get; } = "#cccccc";

        public bool HasBorder { set; get; } = true;

        public bool HasBackground { set; get; } = true;

        public TextBox() { }
    }

    public class TextArea : Editor
    {
        public TextArea() { }
    }

    public class NumericTextBox : Entry
    {
        public NumericTextBox() { }
    }

    public class XButton : Button
    {
        public XButton() { }
    }

    public class CustomToolBar
    {
        public IList<ToolbarItem> ToolBar { set; get; }

        public CustomToolBar()
        {
            this.ToolBar = new List<ToolbarItem>();
            this.ToolBar.Add(new ToolbarItem
            {
                Text = "item1"
            });
        }
    }

    public class CustomDatePicker : DatePicker
    {
        public CustomDatePicker() { }
    }

    public class CustomCheckBox : CheckBox
    {
        public CustomCheckBox() { }
    }

    public class CustomPicker : Picker
    {
        public CustomPicker() { }
    }

    public class CustomSearchBar : SearchBar
    {
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

    public class CustomImageWraper : StackLayout
    {
        public string Name { set; get; }

        public CustomImageWraper()
        {

        }

        public CustomImageWraper(string name)
        {
            Name = name;
        }
    }
}
