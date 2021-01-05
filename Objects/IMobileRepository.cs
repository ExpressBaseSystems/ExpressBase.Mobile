using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Enums;
using System.Collections.Generic;

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

    public interface IMobileLink
    {
        string LinkRefId { set; get; }

        WebFormDVModes FormMode { set; get; }

        List<EbMobileDataColToControlMap> LinkFormParameters { get; set; }
    }

    public interface IGridAlignment
    {
        int RowSpan { set; get; }

        int ColumnSpan { set; get; }

        MobileHorrizontalAlign HorrizontalAlign { set; get; }

        MobileVerticalAlign VerticalAlign { set; get; }
    }

    public interface IFileUploadControl
    {

    }

    public interface IMobileUIControl
    {
        string BackgroundColor { set; get; }

        int BorderRadius { set; get; }

        string BorderColor { set; get; }

        int BorderThickness { set; get; }

        EbThickness Padding { set; get; }

        int Height { set; get; }

        int Width { set; get; }
    }
}
