using System;
using System.Collections.Generic;
using System.Text;

namespace ExpressBase.Mobile
{
    public enum NetworkMode
    {
        Online,
        Offline,
        Mixed
    }

    public enum SortOrder
    {
        Ascending = 0,
        Descending = 1
    }

    public enum NumericBoxTypes
    {
        TextType = 0,
        ButtonType = 1
    }

    public enum DataColumnRenderType
    {
        Text = 1,
        Image = 2,
        MobileNumber = 3,
        Map = 4,
        Email = 5
    }

    public enum MobileHorrizontalAlign
    {
        Left,
        Center,
        Right,
        Fill
    }

    public enum MobileVerticalAlign
    {
        Top,
        Center,
        Bottom,
        Fill
    }

    public enum StackOrientation
    {
        Horizontal,
        Vertical,
    }

    public enum MobileVisualizationType
    {
        Dynamic,
        Static
    }
}
