using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using System.Runtime.Serialization;

namespace ExpressBase.Mobile
{
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

    public class EbTextBox : EbControlUI
    {
       
        public int MaxLength { get; set; }

        public TextTransform TextTransform { get; set; }

        public TextMode TextMode { get; set; }

        public int RowsVisible { get; set; }

        public string PlaceHolder { get; set; }

        public string FontFamilyT { get; set; }

        public string MetaOnly { get; set; }

        public string Text { get; set; }

        public bool AutoCompleteOff { get; set; }

        public bool AutoSuggestion { get; set; }

        public string TableName { get; set; }

        public List<string> Suggestions  { get; set; }

        public string MaxDateExpression { get; set; }

        public string MinDateExpression { get; set; }

        private string TextTransformString
        {
            get { return (((int)this.TextTransform > 0) ? "$('#{0}').keydown(function(event) { textTransform(this, {1}); }); $('#{0}').on('paste', function(event) { textTransform(this, {1}); });".Replace("{0}", this.Name).Replace("{1}", ((int)this.TextTransform).ToString()) : string.Empty); }
        }

        public override string ToolIconHtml { get { return "<i class='fa fa-i-cursor'></i>"; } set { } }
    }

    public enum RenderMode
    {
        Developer,
        User
    }
}
