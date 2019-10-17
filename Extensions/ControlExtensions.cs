using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace ExpressBase.Mobile.Extensions
{
    public static class ControlExtensions
    {
        public static void AddToolBarItems(this IList<ToolbarItem> toolbaritems, IList<ToolbarItem> items)
        {
            foreach(ToolbarItem _item in items)
            {
                toolbaritems.Add(_item);
            }
        }
    }
}
