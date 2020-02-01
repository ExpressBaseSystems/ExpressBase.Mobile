using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ExpressBase.Mobile.Extensions
{
    public static class ListExtension
    {
        public static Dictionary<string, List<MobilePagesWraper>> Group(this IList<MobilePagesWraper> Pages)
        {
            Dictionary<string, List<MobilePagesWraper>> dict = new Dictionary<string, List<MobilePagesWraper>>();

            foreach (MobilePagesWraper wrpr in Pages)
            {
                if (wrpr.IsHidden)
                    continue;

                if (!dict.ContainsKey(wrpr.ContainerType))
                    dict.Add(wrpr.ContainerType, new List<MobilePagesWraper>());
                dict[wrpr.ContainerType].Add(wrpr);
            }
            return dict;
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
