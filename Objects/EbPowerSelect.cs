using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using System.Runtime.Serialization;

namespace ExpressBase.Mobile
{
    public enum DefaultSearchFor
    {
        BeginingWithKeyword,
        EndingWithKeyword,
        ExactMatch,
        Contains,
    }

    public class EbPowerSelect : EbControlUI
    {

        public bool IsInsertable { get; set; }


        public EbSimpleSelect EbSimpleSelect { set; get; }

        //public override EbDbTypes EbDbType
        //{
        //    get
        //    {
        //        return (this.MultiSelect) ? EbDbTypes.String : EbDbTypes.Decimal;
        //    }
        //    set { }
        //}

        public string DataSourceId { get; set; }

        public string FormRefId { get; set; }

        //public DVColumnCollection Columns { get; set; }

        //public DVColumnCollection DisplayMembers { get; set; }

       //public DVBaseColumn DisplayMember { get; set; }

        //public DVBaseColumn ValueMember { get; set; }

        public int DropdownWidth { get; set; }

        public int DropdownHeight { get; set; }

        public int Value { get; set; }

        public int MinSeachLength { get; set; }

        public string Text { get; set; }

        public override bool Required { get; set; }

        public bool MultiSelect
        {
            get
            {
                return this.MaxLimit != 1;
            }
            set { }
        }

        public int MaxLimit { get; set; }

        public int MinLimit { get; set; }

        public DefaultSearchFor DefaultSearchFor { get; set; }

        public int NumberOfFields { get; set; }

        public bool IsDynamic { get; set; }

        public int[] values { get; set; }

        public override bool IsReadOnly { get => this.ReadOnly; }

        public bool RenderAsSimpleSelect { get; set; }
    }
}
