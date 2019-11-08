using System;
using System.Collections.Generic;
using System.Text;

namespace ExpressBase.Mobile
{
    public class EbSimpleSelect : EbControlUI
    {
        public string DataSourceId { get; set; }

        public string PlaceHolder { get; set; }

        public bool IsMultiSelect { get; set; }

        public bool IsSearchable { get; set; }

        public int MaxLimit { get; set; }

        public int MinLimit { get; set; }

        //public DVColumnCollection Columns { get; set; }

        //public DVBaseColumn ValueMember { get; set; }

        public List<EbSimpleSelectOption> Options { get; set; }
        
        //public DVBaseColumn DisplayMember { get; set; }

        public int Value { get; set; }

        public override bool IsReadOnly { get => this.ReadOnly; }

        public bool IsDynamic { get; set; }

    }

    public class EbSimpleSelectOption
    {
        public string EbSid { get; set; }

        public string Name { get; set; }
        
        public string Value { get; set; }

        public string DisplayName { get; set; }
    }
}
