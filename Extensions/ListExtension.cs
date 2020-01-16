using ExpressBase.Mobile.Data;
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

        public static Dictionary<string, SingleTable> GroupByControl(this IList<FileWrapper> Files, List<ApiFileData> ApiFiles)
        {
            Dictionary<string, SingleTable> data = new Dictionary<string, SingleTable>();

            if (ApiFiles.Count > 0)
            {
                foreach (FileWrapper file in Files)
                {
                    ApiFileData _apiFile = ApiFiles.Find(af => af.FileName == file.FileName);

                    if (_apiFile != null && _apiFile.FileRefId > 0)
                    {
                        if (!data.ContainsKey(file.ControlName))
                            data.Add(file.ControlName, new SingleTable());

                        var row = new SingleRow();
                        row.Columns.Add(new SingleColumn
                        {
                            Name = _apiFile.FileName,
                            Value = _apiFile.FileRefId
                        });

                        data[file.ControlName].Add(row);
                    }
                }
            }
            return data;
        }
    }
}
