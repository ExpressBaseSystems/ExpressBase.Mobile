using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Structures;
using System;
using System.Collections.Generic;
using System.Text;

namespace ExpressBase.Mobile.Extensions
{
    public static class ControlExtensions
    {
        public static EbMobileForm ResolveDependency(this EbMobileForm sourceForm)
        {
            try
            {
                if (!string.IsNullOrEmpty(sourceForm.AutoGenMVRefid))
                {
                    var autogenvis = HelperFunctions.GetPage(sourceForm.AutoGenMVRefid);

                    if (autogenvis != null)
                    {
                        string linkref = (autogenvis.Container as EbMobileVisualization).LinkRefId;

                        if (!string.IsNullOrEmpty(linkref))
                        {
                            var linkpage = HelperFunctions.GetPage(linkref);

                            if (linkpage != null && linkpage.Container is EbMobileVisualization)
                            {
                                if (!string.IsNullOrEmpty((linkpage.Container as EbMobileVisualization).LinkRefId))
                                {
                                    var innerlink = HelperFunctions.GetPage((linkpage.Container as EbMobileVisualization).LinkRefId);

                                    if (innerlink != null && innerlink.Container is EbMobileForm)
                                        return (innerlink.Container as EbMobileForm);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
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
                Log.Write(ex.Message);
                Log.Write(ex.StackTrace);
            }
            return ds;
        }
    }
}
