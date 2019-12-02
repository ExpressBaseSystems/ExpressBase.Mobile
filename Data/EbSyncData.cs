using ExpressBase.Mobile.Models;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace ExpressBase.Mobile.Data
{
    public class EbSyncData
    {
        public string DeviceId { set; get; }

        public string AppVersion { set; get; }   

        public EbSyncTableCollection SyncData { set; get; }

        public EbSyncData()
        {
            INativeHelper helper = DependencyService.Get<INativeHelper>();
            AppVersion = string.Format("{0}-{1}", DeviceInfo.Platform, helper.AppVersion);
            DeviceId = helper.DeviceId;

            SyncData = new EbSyncTableCollection();
            this.PullLocalData();
        }

        private void PullLocalData()
        {
            try
            {
                EbDataTable table = App.DataDB.DoQuery(StaticQueries.GET_ALL_TABLE);

                foreach (EbDataRow row in table.Rows)
                {
                    EbSyncTable _ST = this.GetTable(row["name"].ToString());
                    if (_ST.Rows.Count > 0)
                        SyncData.Tables.Add(_ST);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private EbSyncTable GetTable(string tablename)
        {
            EbSyncTable _Table = new EbSyncTable { TableName = tablename };
            try
            {
                EbDataTable dt = App.DataDB.DoQuery(string.Format(StaticQueries.STARFROM_TABLE, tablename));

                foreach (EbDataRow _row in dt.Rows)
                {
                    if (Convert.ToInt32(_row["eb_del"]) <= 0)
                    {
                        EbSyncTableRow trow = new EbSyncTableRow();

                        for (int i = 0; i < _row.Count; i++)
                        {
                            trow.ColumnData.Add(dt.Columns[i].ColumnName, _row[i]);
                        }
                        _Table.Rows.Add(trow);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return _Table;
        }
    }

    public class EbSyncTableCollection
    {
        public List<EbSyncTable> Tables { set; get; }

        public EbSyncTableCollection()
        {
            this.Tables = new List<EbSyncTable>();
        }
    }

    public class EbSyncTable
    {
        public string TableName { set; get; }

        public List<EbSyncTableRow> Rows { set; get; }

        public EbSyncTable()
        {
            this.Rows = new List<EbSyncTableRow>();
        }
    }

    public class EbSyncTableRow
    {
        public bool Synced { set; get; } = false;

        public Dictionary<string, object> ColumnData { set; get; }

        public EbSyncTableRow()
        {
            ColumnData = new Dictionary<string, object>();
        }
    }
}
