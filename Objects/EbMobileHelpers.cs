using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Structures;
using System;
using System.Collections.Generic;
using System.Text;

namespace ExpressBase.Mobile
{
    public enum DataColumnRenderType
    {
        Text = 1,
        Image = 2,
        MobileNumber = 3
    }

    public enum MobileTextAlign
    {
        Left,
        Center,
        Right,
    }

    public class EbMobileDataColumn : EbMobileControl, INonPersistControl
    {
        public int TableIndex { get; set; }

        public int ColumnIndex { get; set; }

        public string ColumnName { get; set; }

        public EbDbTypes Type { get; set; }

        public DataColumnRenderType RenderAs { set; get; }

        public string TextFormat { get; set; }

        public EbFont Font { get; set; }

        public int RowSpan { set; get; }

        public int ColumnSpan { set; get; }

        public MobileTextAlign TextAlign { set; get; }

        public string GetContent(object value)
        {
            if (!string.IsNullOrEmpty(TextFormat))
                return TextFormat.Replace("{value}", value?.ToString());
            else
                return value?.ToString();
        }
    }

    public class EbMobileDataColToControlMap : EbMobilePageBase
    {
        public string ColumnName { set; get; }

        public EbDbTypes Type { get; set; }

        public EbMobileControlMeta FormControl { set; get; }
    }

    public class EbMobileControlMeta : EbMobilePageBase
    {
        public string ControlName { set; get; }

        public string ControlType { set; get; }
    }

    public class EbCTCMapper : EbMobilePageBase
    {
        public string ColumnName { set; get; }

        public string ControlName { set; get; }
    }

    public class EbThickness
    {
        public int Left { set; get; }

        public int Top { set; get; }

        public int Right { set; get; }

        public int Bottom { set; get; }

        public EbThickness() { }

        public EbThickness(int value)
        {
            Left = Top = Right = Bottom = value;
        }

        public EbThickness(int left, int top, int right, int bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }
    }
}
