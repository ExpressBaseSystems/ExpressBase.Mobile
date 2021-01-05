using ExpressBase.Mobile.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpressBase.Mobile.Data
{
    public class EbDataSet
    {
        public TableColletion Tables { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public string RowNumbers { get; set; }

        public EbDataSet()
        {
            //if (this.Tables == null) //Hack for deserialization issue Tables nullified by constructor call.. Need neater fix.
                this.Tables = new TableColletion(this);
        }

        internal EbDataSet ToDataSet()
        {
            throw new NotImplementedException();
        }

        public bool TryGetTable(int index, out EbDataTable dt)
        {
            dt = null;

            if(Tables.HasLength(index))
            {
                dt = Tables[index];
                return true;
            }

            return false;
        }
    }
}
