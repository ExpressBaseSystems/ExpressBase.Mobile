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

    public enum SysContentType
    {
        File = 0,
        Directory = 1
    }

    public enum FontStyle
    {
        NORMAL = 0,
        ITALIC = 2,
        BOLD = 1,
        BOLDITALIC = 3
    }

    public enum FormMode
    {
        NEW = 0,
        EDIT = 1,
        REF = 2,
        PREFILL = 3
    }

    public enum WebFormDVModes
    {
        _SELECT_ = 0,
        View_Mode = 1,
        New_Mode = 2
    }

    public enum LogTypes
    {
        EXCEPTION,
        STACKTRACE,
        MESSAGE
    }

    public enum EbFileCategory
    {
        File = 0,
        Images = 1,
        External = 2,
        SolLogo = 3,
        Dp = 4,
        //ImageSmall,
        //ImageMed,
        //ImageLarge,
        LocationFile = 5
    }

    public enum MobilePlatform
    {
        wns,
        apns,
        gcm
    }

    public enum BuildEnvironement
    {
        Dev,
        Production
    }

    public enum SignInOtpType
    {
        Sms = 1,
        Email = 2
    }

    public enum LoginType
    {
        SSO,
        CREDENTIALS
    }
}
