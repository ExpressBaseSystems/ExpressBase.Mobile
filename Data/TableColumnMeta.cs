using ExpressBase.Mobile.Structures;
using System;
using System.Collections.Generic;
using System.Text;

namespace ExpressBase.Mobile.Data
{
    public class WebformData
    {
        public Dictionary<string, SingleTable> MultipleTables { get; set; }

        public Dictionary<string, SingleTable> ExtendedTables { get; set; }

        public Dictionary<string, bool> DisableDelete { get; set; }

        public Dictionary<string, bool> DisableCancel { get; set; }

        public string MasterTable { get; set; }

        public WebformData()
        {
            MultipleTables = new Dictionary<string, SingleTable>();
            ExtendedTables = new Dictionary<string, SingleTable>();
            DisableDelete = new Dictionary<string, bool>();
            DisableCancel = new Dictionary<string, bool>();
        }
    }

    public class SingleTable : List<SingleRow>
    {
        public string ParentTable { get; set; }

        public string ParentRowId { get; set; }
    }

    public class SingleRow
    {
        public string RowId { get; set; }

        public bool IsUpdate { get; set; }

        public List<SingleColumn> Columns { get; set; }

        public SingleRow()
        {
            Columns = new List<SingleColumn>();
        }
    }

    public class SingleColumn
    {
        public string Name { get; set; }

        public dynamic Value { get; set; }

        public int Type { get; set; }

        public bool AutoIncrement { get; set; }

        public SingleColumn() { }
    }

    public class WebFormSaveResponse {

        public int RowAffected { get; set; }

        public int RowId { get; set; }

        public WebformData FormData { get; set; }

        public int AfterSaveStatus { get; set; }
    }
}
