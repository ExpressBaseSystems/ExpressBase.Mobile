using System;
using System.Collections.Generic;
using System.Text;

namespace ExpressBase.Mobile.Enums
{
    public enum FileClass
    {
        files = 2,
        image = 1
    }

    public enum EbDateType
    {
        Date = 5,
        Time = 17,
        DateTime = 6,
    }

    public enum ScriptingLanguage
    {
        JS = 0,
        CSharp = 1,
        SQL = 2
    }

    public enum TextTransform
    {
        Normal,
        lowercase,
        UPPERCASE,
    }

    public enum TextMode
    {
        SingleLine = 0,
        Email = 2,
        Password = 1,
        Color = 3,
        MultiLine = 4
    }

    public enum TimeShowFormat
    {
        Hour_Minute_Second_12hrs,
        Hour_Minute_Second_24hrs,
        Hour_Minute_12hrs,
        Hour_Minute_24hrs,
        Hour_12hrs,
        Hour_24hrs,
    }

    public enum DateShowFormat
    {
        Year_Month_Date,
        Year_Month,
        Year,
    }
}
