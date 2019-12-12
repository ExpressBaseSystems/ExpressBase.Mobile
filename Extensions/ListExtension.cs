using ExpressBase.Mobile.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ExpressBase.Mobile.Extensions
{
    public static class ListExtension
    {
        public static List<PageTypeGroup> ToUIGroup(this IList<MobilePagesWraper> Pages)
        {
            List<PageTypeGroup> TypeList = new List<PageTypeGroup>();

            //foreach (MobilePagesWraper wrpr in Pages)
            //{
            //    string _title = wrpr.Title;

            //    PageTypeGroup grp = TypeList.Find(o => o.GroupHeader == _title);
            //    if (grp == null)
            //    {
            //        var gr = new PageTypeGroup { GroupHeader = _title };
            //        gr.Add(wrpr);
            //        TypeList.Add(gr);
            //    }
            //    else
            //    {
            //        grp.Pages.Add(wrpr);
            //    }
            //}
            return TypeList;
        }
    }
}
