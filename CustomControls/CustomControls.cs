using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Structures;
using Plugin.Media;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace ExpressBase.Mobile.CustomControls
{
    public class TextBox : Entry
    {
        public TextBox() { }
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
}
