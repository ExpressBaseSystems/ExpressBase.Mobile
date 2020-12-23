using System.Collections.Generic;

namespace ExpressBase.Mobile
{
    public class EbMobileDashBoard : EbMobileContainer
    {
        public string DataSourceRefId { set; get; }

        public EbScript OfflineQuery { set; get; }

        public EbThickness Padding { set; get; }

        public int Spacing { set; get; }

        public List<EbMobileDashBoardControl> ChildControls { get; set; }

        public EbMobileDashBoard()
        {
            
        }
    }
}
