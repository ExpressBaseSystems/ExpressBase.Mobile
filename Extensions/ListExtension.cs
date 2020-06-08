using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace ExpressBase.Mobile.Extensions
{
    public static class ListExtension
    {
        public static Dictionary<string, List<MobilePagesWraper>> Group(this IEnumerable<MobilePagesWraper> Pages)
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

        public static bool IsNullOrEmpty<T>(this List<T> source)
        {
            return source == null || source.Count < 1;
        }

        public static bool HasLength<T>(this List<T> source, int index)
        {
            return (source.Count >= index);
        }

        public static Dictionary<string, EbMobileControl> ToControlDictionary(this List<EbMobileControl> controls)
        {
            Dictionary<string, EbMobileControl> _dict = new Dictionary<string, EbMobileControl>();
            try
            {
                foreach (EbMobileControl ctrl in controls)
                {
                    if (ctrl is EbMobileTableLayout)
                    {
                        foreach (EbMobileTableCell cell in (ctrl as EbMobileTableLayout).CellCollection)
                        {
                            foreach (EbMobileControl tctrl in cell.ControlCollection)
                                _dict.Add(tctrl.Name, tctrl);
                        }
                    }
                    else
                        _dict.Add(ctrl.Name, ctrl);
                }
            }
            catch (Exception ex)
            {
                EbLog.Write(ex.Message);
            }
            return _dict;
        }

        public static List<Param> ToParams(this List<DbParameter> parameters)
        {
            List<Param> p = new List<Param>();
            try
            {
                foreach (DbParameter dbp in parameters)
                {
                    p.Add(new Param
                    {
                        Name = dbp.ParameterName,
                        Type = dbp.DbType.ToString(),
                        Value = dbp.Value.ToString()
                    });
                }
            }
            catch (Exception ex)
            {
                EbLog.Write(ex.Message);
            }
            return p;
        }

        public static void AddRange<T>(this ObservableCollection<T> observable, IEnumerable<T> enumerable)
        {
            try
            {
                foreach (T item in enumerable)
                    observable.Add(item);
            }
            catch (Exception ex)
            {
                EbLog.Write(ex.Message);
            }
        }
    }
}
