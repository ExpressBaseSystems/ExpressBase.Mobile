using System;
using System.Collections.Generic;
using System.Text;

namespace ExpressBase.Mobile
{
    public class EbNumeric : EbControlUI
    {
        public EbNumeric() { }

        public int MaxLength { get; set; }

        public int DecimalPlaces { get; set; }

        public decimal Value { get; set; }

        public bool AutoIncrement { get; set; }

        private string PlaceHolder
        {
            get
            {
                return (this.DecimalPlaces == 0) ? "0" : "0." + new String('0', this.DecimalPlaces);
            }
        }

        public bool AllowNegative { get; set; }

        public int MaxLimit { get; set; }

        public int MinLimit { get; set; }

        public bool IsCurrency { get; set; }

        public bool AutoCompleteOff { get; set; }

        public bool ShowIcon { get; set; }
    }
}
