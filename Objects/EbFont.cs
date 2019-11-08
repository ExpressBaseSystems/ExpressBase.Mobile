using System;
using System.Collections.Generic;
using System.Text;

namespace ExpressBase.Mobile
{
    public class EbFont
    {
        public EbFont() { }

        public string FontName { get; set; } = "Times-Roman";

        public string CSSFontName
        {
            get
            {
                return FontFamilies.Find(e => e.SystemFontName == this.FontName).CSSFontName;
            }
            set { }
        }

        public int Size { get; set; }

        public FontStyle Style { get; set; }

        public string color { get; set; }

        public bool Caps { get; set; }

        public bool Strikethrough { get; set; }

        public bool Underline { get; set; }

        public static List<EbFontFamily> FontFamilies
        {
            get
            {
                return new List<EbFontFamily>
                {
                    new EbFontFamily{ CSSFontName ="Arapey",SystemFontName="Arapey"},
                    new EbFontFamily{ CSSFontName ="Arvo",SystemFontName="Arvo"},
                    new EbFontFamily{ CSSFontName ="Libre Baskerville",SystemFontName="Baskerville"},
                    new EbFontFamily{ CSSFontName ="Bentham",SystemFontName="Bentham"},
                    new EbFontFamily{ CSSFontName ="Cabin Condensed",SystemFontName="Cabin Condensed"},
                    new EbFontFamily{ CSSFontName ="Didact Gothic",SystemFontName="Century Gothic"},
                    new EbFontFamily{ CSSFontName ="Courier",SystemFontName="Courier"},
                    new EbFontFamily{ CSSFontName ="Crimson Text",SystemFontName="Crimson Text"},
                    new EbFontFamily{ CSSFontName ="EB Garamond",SystemFontName="EB Garamond"},
                    new EbFontFamily{ CSSFontName ="GFS Didot",SystemFontName="GFS Didot"},
                    new EbFontFamily{ CSSFontName ="Montserrat",SystemFontName="Gotham"},
                    new EbFontFamily{ CSSFontName ="Helvetica",SystemFontName="Helvetica"},
                    new EbFontFamily{ CSSFontName ="Libre Franklin",SystemFontName="Libre Franklin"},
                    new EbFontFamily{ CSSFontName ="Maven Pro",SystemFontName="Maven Pro"},
                    new EbFontFamily{ CSSFontName ="Merriweather",SystemFontName="Merriweather"},
                    new EbFontFamily{ CSSFontName ="News Cycle",SystemFontName="News Cycle"},
                    new EbFontFamily{ CSSFontName ="Puritan",SystemFontName="Puritan"},
                    new EbFontFamily{ CSSFontName ="Questrial",SystemFontName="Questrial"},
                    new EbFontFamily{ CSSFontName ="Times",SystemFontName="Times-Roman"},
                    new EbFontFamily{ CSSFontName ="Tinos",SystemFontName="Times"},
                    new EbFontFamily{ CSSFontName ="Heebo",SystemFontName="ZapfDingbats"}
                };
            }
        }
    }
    //JS OBJ:  {"Font":"Abhaya Libre","Fontsize":"16","Fontstyle":"normal","FontWeight":"bold","Fontcolor":"#000000","Caps":"none","Strikethrough":"line-through","Underline":"none"}

    public enum FontStyle
    {
        NORMAL = 0,
        ITALIC = 2,
        BOLD = 1,
        BOLDITALIC = 3
    }

    public class EbFontFamily
    {
        public string CSSFontName { set; get; }

        public string SystemFontName { set; get; }
    }
}
