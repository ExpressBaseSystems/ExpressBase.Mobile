using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Structures;
using System.Collections.Generic;
using System.Linq;

namespace ExpressBase.Mobile
{
    public class EbMobileDisplayPicture : EbMobileFileUpload
    {
        public override bool MultiSelect => false;

        public override bool EnableEdit { set; get; }

        public override EbDbTypes EbDbType { get { return EbDbTypes.String; } set { } }

        public override void InitXControl(FormMode Mode, NetworkMode Network)
        {
            base.InitXControl(Mode, Network);
        }

        public override object GetValue()
        {
            return XamControl.GetFiles(this.Name);
        }

        public override MobileTableColumn GetMobileTableColumn()
        {
            return new MobileTableColumn
            {
                Name = this.Name,
                Type = this.EbDbType
            };
        }

        public override bool SetValue(object value)
        {
            if (value != null)
            {
                XamControl.SetValue(this.NetworkType, value as FUPSetValueMeta, this.Name);
            }
            return true;
        }
    }
}
