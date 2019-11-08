using ExpressBase.Mobile.Constants;
using System;
using System.Collections.Generic;
using System.Text;

namespace ExpressBase.Mobile.Structures
{
    public struct EbObjectType
    {
        public readonly string Name;
        public readonly int IntCode;
        public readonly string BMW;
        public readonly bool IsUserFacing;
        public readonly string Icon;
        public readonly string Alias;

        public bool IsAvailableInBot { get; private set; }
        public bool IsAvailableInMobile { get; private set; }
        public bool IsAvailableInWeb { get; private set; }

        internal EbObjectType(string name, int code, string bmw, bool isUserFacing,string icon,string alias)
        {
            Name = name;    // 38 bytes max
            IntCode = code; //4 bytes
            BMW = bmw;      //6 bytes
            Icon = icon;
            Alias = alias ?? name;

            IsAvailableInBot = BMW.Contains(ObjectConstants.B_IN_BMW);
            IsAvailableInMobile = BMW.Contains(ObjectConstants.M_IN_BMW);
            IsAvailableInWeb = BMW.Contains(ObjectConstants.W_IN_BMW);
            IsUserFacing = isUserFacing;
        }

        // EbObjectType x = (EbObjectType)i;
        public static explicit operator EbObjectType(int i)
        {
            return EbObjectTypes.Get(i);
        }

        //int i = (int)o;
        public static explicit operator int(EbObjectType o)
        {
            return o.IntCode;
        }

        public static bool operator ==(int i, EbObjectType b)
        {
            return (i == b.IntCode);
        }

        public static bool operator !=(int i, EbObjectType b)
        {
            return (i != b.IntCode);
        }

        public override string ToString()
        {
            return this.Name;
        }
    }

    public struct EbObjectTypes
    {
        public const int iNull = -1;
        public const int iWebForm = 0;
        //public const int iDisplayBlock = 1;
        public const int iDataReader = 2;
        public const int iReport = 3;
        public const int iDataWriter = 4;
        //public const int iTable = 4;
        public const int iSqlFunction = 5;
        //public const int iSqlValidator = 6;
        public const int iJavascriptFunction = 7;
        //public const int iJavascriptValidator = 8;
        //public const int iDataVisualization = 11;
        public const int iFilterDialog = 12;
        //public const int iMobileForm = 13;
        public const int iUserControl = 14;
        public const int iEmailBuilder = 15;
        public const int iTableVisualization = 16;
        public const int iChartVisualization = 17;
        public const int iBotForm = 18;
        public const int iSmsBuilder = 19;
        public const int iApi = 20;
        public const int iGoogleMap = 21;
        public const int iDashBoard = 22;
        public const int iKanBan = 23;
        public const int iCalendarView = 24;
        public const int iCsharpFunction = 25;

        public static readonly EbObjectType Null = new EbObjectType("Null", iNull, ObjectConstants.XXX, false, "fa-exclamation-triangle",null);
        public static readonly EbObjectType WebForm = new EbObjectType(ObjectConstants.WEB_FORM, iWebForm, ObjectConstants.XXW, true, "fa-wpforms","Form");//true
        //public static readonly EbObjectType DisplayBlock = new EbObjectType(ObjectConstants.DISPLAY_BLOCK, iDisplayBlock, ObjectConstants.XXX, false);
        public static readonly EbObjectType DataReader = new EbObjectType(ObjectConstants.DATA_READER, iDataReader, ObjectConstants.BMW, false, "fa-database", "Data Reader");
        public static readonly EbObjectType DataWriter = new EbObjectType(ObjectConstants.DATA_WRITER, iDataWriter, ObjectConstants.BMW, false, "fa-database", "Data Writer");
        public static readonly EbObjectType Report = new EbObjectType(ObjectConstants.REPORT, iReport, ObjectConstants.XMW, true, "fa-file-pdf-o","PDF Report");
        //public static readonly EbObjectType Table = new EbObjectType(ObjectConstants.TABLE, iTable, ObjectConstants.BMW, false);
        public static readonly EbObjectType SqlFunction = new EbObjectType(ObjectConstants.SQL_FUNCTION, iSqlFunction, ObjectConstants.BMW, false, "fa-code", "Sql Function");
        //public static readonly EbObjectType SqlValidator = new EbObjectType(ObjectConstants.SQL_VALIDATOR, iSqlValidator, ObjectConstants.BMW, false);
        public static readonly EbObjectType JavascriptFunction = new EbObjectType(ObjectConstants.JAVASCRIPT_FUNCTION, iJavascriptFunction, ObjectConstants.BMW, false, "fa-file-code-o", "JavaScript Function");
        //public static readonly EbObjectType JavascriptValidator = new EbObjectType(ObjectConstants.JAVASCRIPT_VALIDATOR, iJavascriptValidator, ObjectConstants.BMW, false);
        //public static readonly EbObjectType DataVisualization = new EbObjectType(ObjectConstants.DATA_VISUALIZATION, iDataVisualization, ObjectConstants.BMW, false);
        public static readonly EbObjectType FilterDialog = new EbObjectType(ObjectConstants.FILTER_DIALOG, iFilterDialog, ObjectConstants.BMW, false, "fa-filter", "Filter Dialog");
        //public static readonly EbObjectType MobileForm = new EbObjectType(ObjectConstants.MOBILE_FORM, iMobileForm, ObjectConstants.BMW, false);
        public static readonly EbObjectType UserControl = new EbObjectType(ObjectConstants.USER_CONTROL, iUserControl, ObjectConstants.BMW, false, "fa-puzzle-piece", "User Control");
        public static readonly EbObjectType EmailBuilder = new EbObjectType(ObjectConstants.EMAIL_BUILDER, iEmailBuilder, ObjectConstants.XMW, false, "fa-envelope-o", "Email Builder");
        public static readonly EbObjectType TableVisualization = new EbObjectType(ObjectConstants.TABLE_VISUALIZATION, iTableVisualization, ObjectConstants.BMW, true,"fa-table","Table View");
        public static readonly EbObjectType ChartVisualization = new EbObjectType(ObjectConstants.CHART_VISUALIZATION, iChartVisualization, ObjectConstants.BMW, true, "fa-bar-chart","Chart View");
        public static readonly EbObjectType BotForm = new EbObjectType(ObjectConstants.BOT_FORM, iBotForm, ObjectConstants.BXX, true, "fa-wpforms", "Bot Form");
        public static readonly EbObjectType SmsBuilder = new EbObjectType(ObjectConstants.SMS_BUILDER, iSmsBuilder, ObjectConstants.XXW, false, "fa-commenting-o", "Sms Builder");
        public static readonly EbObjectType Api = new EbObjectType(ObjectConstants.API, iApi, ObjectConstants.XXW, false, "fa-code",null);
        public static readonly EbObjectType GoogleMap = new EbObjectType(ObjectConstants.GOOGLEMAP, iGoogleMap, ObjectConstants.BMW, true, "fa-map-marker", "Google MapView");
        public static readonly EbObjectType DashBoard = new EbObjectType(ObjectConstants.DASHBOARD, iDashBoard, ObjectConstants.BMW, true, "fa-tachometer", null);

        public static readonly EbObjectType KanBan = new EbObjectType(ObjectConstants.KANBAN, iKanBan, ObjectConstants.XXW, false, "fa-tag", null);
        public static readonly EbObjectType CalendarView = new EbObjectType(ObjectConstants.CALENDARVIEW, iCalendarView, ObjectConstants.BMW, false, "fa-calendar", "Calendar View");
        public static readonly EbObjectType CsharpFunction = new EbObjectType(ObjectConstants.CSHARPFUNCTION, iCsharpFunction, ObjectConstants.XXW, false, "fa-hashtag", "C# Function");

        public static EbObjectType Get(int intcode)
        {
            foreach (EbObjectType o in Enumerator)
            {
                if (o.IntCode == intcode)
                    return o;
            }

            return Null;
        }

        public static IEnumerable<EbObjectType> Enumerator
        {
            get
            {
                return new[] {
                    WebForm,
                    //DisplayBlock,
                    DataReader,
                    Report,
                    DataWriter,
                    //Table,
                    SqlFunction,
                    //SqlValidator,
                    //JavascriptValidator,
                    //DataVisualization,
                    FilterDialog,
                    //MobileForm,
                    UserControl,
                    EmailBuilder,
                    TableVisualization,
                    ChartVisualization,
                    SmsBuilder,
                    Api,
                    GoogleMap,
                    BotForm,
                    KanBan,
                    CalendarView,
                    DashBoard,
                    JavascriptFunction,
                    CsharpFunction
                };
            }
        }
    }
}
