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
        Email = 5,
        Audio = 6
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

    public enum MobileTextWrap
    {
        NoWrap = 1,
        HeadTruncation = 2,
        CharacterWrap = 3,
        MiddleTruncation = 4,
        TailTruncation = 5,
        WordWrap = 6
    }

    public enum MobileTextAlign
    {
        Start = 1,
        Center = 2,
        End = 3
    }
}
