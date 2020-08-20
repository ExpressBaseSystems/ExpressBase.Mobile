using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Structures;
using System;
using System.Collections.Generic;
using System.Text;

namespace ExpressBase.Mobile
{
    public interface INonPersistControl { }

    public interface ILinesEnabled
    {
        string TableName { set; get; }

        List<EbMobileControl> ChildControls { set; get; }

        EbDataTable GetLocalData(string parentTable, int rowid);

        string GetQuery(string parentTable);

        List<SingleColumn> GetColumnValues(ColumnColletion columns, EbDataRow row);
    }

    public interface ILayoutControl { }

    public interface IMobileDataPart
    {
        int TableIndex { get; set; }

        int ColumnIndex { get; set; }

        string ColumnName { get; set; }

        EbDbTypes Type { get; set; }
    }

    public interface IMobileUIStyles
    {
        string BackgroundColor { set; get; }

        int BorderThickness { set; get; }

        string BorderColor { set; get; }

        int BorderRadius { set; get; }
    }

    public interface IMobileLink
    {
        string LinkRefId { set; get; }
    }
}
