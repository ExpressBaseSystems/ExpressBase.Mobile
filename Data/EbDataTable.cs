using ExpressBase.Mobile.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpressBase.Mobile.Data
{
    public class EbDataTable
    {
        internal EbDataSet DataSet { get; set; }

        public string TableName { get; set; }

        public ColumnColletion Columns { get; set; }

        public RowColletion Rows { get; set; }

        public EbDataTable()
        {
            this.Columns = new ColumnColletion(this);
            this.Rows = new RowColletion(this);
        }

        public EbDataTable(string tablename)
        {
            this.TableName = tablename;
            this.Columns = new ColumnColletion(this);
            this.Rows = new RowColletion(this);
        }

        public EbDataRow NewDataRow()
        {
            //EbDataRow dr = new EbDataRow(this.Columns.Count);
            return new EbDataRow();
        }

        public EbDataRow NewDataRow2()
        {
            return new EbDataRow(this.Columns.Count);
        }

        public EbDataTable GetEmptyTable()
        {
            EbDataTable __newTable = new EbDataTable();
            EbDataColumn[] dataColumns = new EbDataColumn[this.Columns.Count];
            this.Columns.CopyTo(dataColumns);

            foreach(EbDataColumn col in dataColumns)
                __newTable.Columns.Add(col);

            return __newTable;
        }

        public EbDataColumn NewDataColumn(int index, string name, EbDbTypes type)
        {
            return new EbDataColumn(index, name, type);
        }
    }

    public class TableColletion : List<EbDataTable>
    {
        internal EbDataSet DataSet { get; set; }

        public TableColletion() { }

        public TableColletion(EbDataSet dataset)
        {
            this.DataSet = dataset;
        }

        public void Add(string tablename, EbDataTable dt)
        {
            if (dt.TableName == null || tablename == null)
            {
                tablename = "Table" + (this.DataSet.Tables.Count + 1).ToString();
                dt.TableName = tablename;
                dt.DataSet = this.DataSet;
            }

            base.Add(dt);
        }

        new public void Add(EbDataTable dt)
        {
            this.Add(dt.TableName, dt);
        }

        public void Remove(string tablename)
        {
            EbDataTable _toRemoveTable = null;

            foreach (EbDataTable table in this)
            {
                if (table.TableName == tablename)
                {
                    _toRemoveTable = table;
                    break;
                }
            }

            base.Remove(_toRemoveTable);
        }

        new public void Remove(EbDataTable dt)
        {
            base.Remove(dt);
        }

		public EbDataTable this[string tablename]
		{
			get
			{
				foreach (EbDataTable table in this)
				{
					if (table.TableName == tablename)
						return table;
				}

				return null;
			}
		}
    }
}
