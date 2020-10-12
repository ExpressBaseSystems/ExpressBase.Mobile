using ExpressBase.Mobile.Structures;

namespace ExpressBase.Mobile
{
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

    public class EbMobileValidator : EbMobilePageBase
    {
        public bool IsDisabled { get; set; }

        public bool IsWarningOnly { get; set; }

        public EbScript Script { get; set; }

        public string FailureMSG { get; set; }

        public bool IsEmpty()
        {
            return Script == null || string.IsNullOrEmpty(Script.Code);
        }
    }
}
