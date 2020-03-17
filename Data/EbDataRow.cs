using ExpressBase.Mobile.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace ExpressBase.Mobile.Data
{
    public class EbDataRow : List<object>
    {
        internal RowColletion Rows { get; set; }

        public EbDataTable Table
        {
            get
            {
                return this.Rows.Table;
            }
        }

        public EbDataRow() { }

        internal EbDataRow(int size) : base(size)
        {
            for (int i = 0; i < size; i++)
                base.Add(null);
        }

        new public void Add(object o)
        {
            base.Add(o);
        }

        public bool IsDBNull(int index)
        {
            if (index > -1)
                return base[index] == DBNull.Value;
            return false;
        }

        new public object this[int index]
        {
            get
            {
                if (index > -1)
                {
                    if (base[index] == DBNull.Value)
                    {
                        if (this.Rows.Table.Columns[index].Type == EbDbTypes.String)
                            return string.Empty;
                        else if (this.Rows.Table.Columns[index].Type == EbDbTypes.Int32)
                            return 0;
                        else if (this.Rows.Table.Columns[index].Type == EbDbTypes.Int64)
                            return 0;
                        else if (this.Rows.Table.Columns[index].Type == EbDbTypes.Boolean)
                            return false;
                        else if (this.Rows.Table.Columns[index].Type == EbDbTypes.Decimal)
                            return 0;
                        else if (this.Rows.Table.Columns[index].Type == EbDbTypes.Date)
                            return DateTime.MinValue;
                    }
                    else
                        return base[index];
                }
                else
                    return null;
                return (index > -1) ? (base[index]) : null;
            }
            set
            {
                if (index > -1)
                    base[index] = value;
            }
        }

        public object this[string columnname]
        {
            get
            {
                EbDataColumn col = this.Rows.Table.Columns[columnname];
                if (col != null)
                    return this[col.ColumnIndex];
                return null;
            }
            set { this[this.Rows.Table.Columns[columnname].ColumnIndex] = value; }
        }
    }

    public class RowColletion : List<EbDataRow>
    {
        public EbDataTable Table { get; private set; }

        [OnDeserialized]
        public void OnDeserializedMethod(StreamingContext context)
        {
            foreach (EbDataRow dr in this)
                dr.Rows = this;
        }

        public RowColletion() { }

        public RowColletion(EbDataTable table)
        {
            this.Table = table;
        }

        new public void Add(EbDataRow dr)
        {
            dr.Rows = this;
            base.Add(dr);
        }

        public void Remove(int index)
        {
            base.RemoveAt(index);
        }

        new public void Remove(EbDataRow row)
        {
            base.Remove(row);
        }
    }
}
