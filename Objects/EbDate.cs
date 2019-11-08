using System;
using System.Collections.Generic;
using System.Text;

namespace ExpressBase.Mobile
{
    public class EbDate : EbControlUI
    {
        public DateTime Min { get; set; }

        public DateTime Max { get; set; }

        public DateTime Value { get; set; }

        public string PlaceHolder { get; set; }

        public bool AutoCompleteOff { get; set; }

        public override bool DoNotPersist { get; set; }

        public bool IsNullable { get; set; }
    }
}
