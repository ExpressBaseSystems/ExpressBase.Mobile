using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Enums;
using System.Collections.Generic;

namespace ExpressBase.Mobile
{
    /*
     Namespace containing interfaces eb mobile page objects
     */

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

    /// <summary>
    /// Interface for Vertical and Horrizontal alignment
    /// only using in mobile platform
    /// </summary>
    public interface IMobileAlignment
    {
        MobileHorrizontalAlign HorrizontalAlign { set; get; }

        MobileVerticalAlign VerticalAlign { set; get; }
    }

    /// <summary>
    /// Interface for setting span on grid in listview (dynamic frame)
    /// only using in mobile platform
    /// </summary>
    public interface IGridSpan
    {
        int RowSpan { set; get; }

        int ColumnSpan { set; get; }
    }

    public interface IFileUploadControl
    {

    }
}
