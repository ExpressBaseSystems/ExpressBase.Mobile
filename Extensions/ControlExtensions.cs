using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Structures;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace ExpressBase.Mobile.Extensions
{
    public static class ControlExtensions
    {
        public static EbMobileForm ResolveDependency(this EbMobileForm sourceForm)
        {
            try
            {
                var autogenvis = HelperFunctions.GetPage(sourceForm.AutoGenMVRefid);

                if (autogenvis != null)
                {
                    string linkref = (autogenvis.Container as EbMobileVisualization).LinkRefId;

                    if (!string.IsNullOrEmpty(linkref))
                    {
                        var linkpage = HelperFunctions.GetPage(linkref);

                        if (linkpage != null && linkpage.Container is EbMobileVisualization viz)
                        {
                            var innerlink = HelperFunctions.GetPage(viz.LinkRefId);

                            if (innerlink != null && innerlink.Container is EbMobileForm mf)
                                return mf;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                EbLog.Info("Failed to resolve form dependencies");
                EbLog.Error(ex.Message);
            }
            return null;
        }

        public static EbDataSet ToDataSet(this WebformData data)
        {
            EbDataSet ds = new EbDataSet();
            try
            {
                foreach (KeyValuePair<string, SingleTable> st in data.MultipleTables)
                {
                    EbDataTable dt = new EbDataTable { TableName = st.Key };
                    for (int i = 0; i < st.Value.Count; i++)
                    {
                        EbDataRow dr = dt.NewDataRow();
                        for (int k = 0; k < st.Value[i].Columns.Count; k++)
                        {
                            SingleColumn sc = st.Value[i].Columns[k];

                            if (i == 0)
                            {
                                EbDataColumn dc = new EbDataColumn
                                {
                                    ColumnIndex = k,
                                    Type = (EbDbTypes)sc.Type,
                                    ColumnName = sc.Name
                                };
                                dt.Columns.Add(dc);
                            }
                            dr.Add((object)sc.Value);
                        }
                        dt.Rows.Add(dr);
                    }
                    ds.Tables.Add(dt);
                }
            }
            catch (Exception ex)
            {
                EbLog.Info("WebformData to dataset operation failed");
                EbLog.Error(ex.StackTrace);
            }
            return ds;
        }

        public static Dictionary<string, List<FileMetaInfo>> ToFilesMeta(this WebformData data)
        {
            Dictionary<string, List<FileMetaInfo>> files = new Dictionary<string, List<FileMetaInfo>>();
            try
            {
                foreach (KeyValuePair<string, SingleTable> st in data.ExtendedTables)
                {
                    foreach (SingleRow row in st.Value)
                    {
                        if (row.Columns.Count > 0)
                        {
                            SingleColumn info = row.Columns[0];
                            List<FileMetaInfo> fileMeta = JsonConvert.DeserializeObject<List<FileMetaInfo>>(info.Value.ToString());
                            files.Add(st.Key, fileMeta);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                EbLog.Error(ex.Message);
                EbLog.Error(ex.StackTrace);
            }
            return files;
        }
    }
}
