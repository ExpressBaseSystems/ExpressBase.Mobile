using ExpressBase.Mobile.Common.Structures;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace ExpressBase.Mobile.CustomControls
{
    public interface CustomElement
    {
        EbDbTypes DbType { set; get; }
    }

    public class TextBox : Entry, CustomElement
    {
        public TextBox() { }

        public EbDbTypes DbType { set; get; }
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

    public class CustomDatePicker : DatePicker, CustomElement
    {
        public EbDbTypes DbType { set; get; }

        public CustomDatePicker()
        {

        }
    }

    public class CustomSelect : Picker, CustomElement
    {
        public EbDbTypes DbType { set; get; }

        public CustomSelect()
        {

        }
    }
}
