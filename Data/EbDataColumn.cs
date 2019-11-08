using ExpressBase.Mobile.Structures;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace ExpressBase.Mobile.Data
{
    public class EbDataColumn
    {
        public EbDataColumn() { }

        public EbDataColumn(string columnname, EbDbTypes type)
        {
            this.ColumnName = columnname;
            this.Type = type;
        }
      
        public EbDataColumn(int index, string columnname, EbDbTypes type)
        {
            this.ColumnIndex = index;
            this.ColumnName = columnname;
            this.Type = type;
        }

        public int ColumnIndex { get; set; }

        public string ColumnName { get; set; }

        public EbDbTypes Type { get; set; }
    }

    public class ColumnColletion : List<EbDataColumn>
    {
        internal EbDataTable Table { get; set; }

        public ColumnColletion(){ }

        public ColumnColletion(EbDataTable table)
        {
            this.Table = table;
        }
       
        public EbDataColumn this[string columnname]
        {
            get
            {
                foreach (EbDataColumn column in this)
                {
                    if (column.ColumnName == columnname)
                        return column;
                }

                return null;
            }
        }

		new public void Add(EbDataColumn column)
		{
			base.Add(column);

			foreach (EbDataRow row in this.Table.Rows)
				row.Add(null);
		}

		public void BaseAdd(EbDataColumn column)
		{
			base.Add(column);
		}
	}
}
