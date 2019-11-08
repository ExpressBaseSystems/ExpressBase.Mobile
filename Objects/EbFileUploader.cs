using System;
using System.Collections.Generic;
using System.Text;
using ExpressBase.Mobile.Enums;

namespace ExpressBase.Mobile
{
    public class EbFileUploader : EbControlUI
    {
        public override bool DoNotPersist { get; set; }

        public override bool Unique { get; set; }

        public EbFileUploader()
        {
            this.Categories = new List<EbFupCategories>();
        }

        public override bool IsReadOnly { get => this.ReadOnly; }

        public FileClass FileType { set; get; }

        public List<EbFupCategories> Categories { set; get; }

        public bool IsMultipleUpload { set; get; }

        public bool EnableTag { set; get; }

        public bool EnableCrop { set; get; }

        public int MaxFileSize { set; get; }

        public bool ResizeViewPort { set; get; }

        public bool DisableUpload { set; get; }
    }

    public class EbFupCategories : EbControl
    {
        public EbFupCategories()
        {

        }

        public string CategoryTitle { set; get; }
    }
}
