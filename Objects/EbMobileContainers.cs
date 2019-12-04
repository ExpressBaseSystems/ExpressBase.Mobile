using ExpressBase.Mobile.Structures;
using System;
using System.Collections.Generic;
using System.Text;

namespace ExpressBase.Mobile
{
    public class DbTypedValue
    {
        public EbDbTypes Type { set; get; } = EbDbTypes.String;

        public object Value { set; get; }
    }

    public class EbMobileContainer : EbMobilePageBase
    {

    }

    public class EbMobileForm : EbMobileContainer
    {
        public override string Name { set; get; }

        public List<EbMobileControl> ChiledControls { get; set; }

        public string TableName { set; get; }

        public bool AutoDeployMV { set; get; }

        public string AutoGenMVRefid { set; get; }

        public string WebFormRefId { set; get; }

        public DbTypedValue GetDbType(string name, object value)
        {
            DbTypedValue TV = new DbTypedValue();

            foreach (EbMobileControl ctrl in ChiledControls)
            {
                if (!(ctrl is EbMobileTableLayout) && ctrl.Name == name)
                {
                    TV.Type = ctrl.EbDbType;
                    TV.Value = ctrl.SQLiteToActual(value);
                }
                else if (ctrl is EbMobileTableLayout)
                {
                    foreach (EbMobileTableCell cell in (ctrl as EbMobileTableLayout).CellCollection)
                    {
                        foreach (EbMobileControl tctrl in cell.ControlCollection)
                        {
                            if (ctrl.Name == name)
                            {
                                TV.Type = ctrl.EbDbType;
                                TV.Value = ctrl.SQLiteToActual(value);
                                return TV;
                            }
                        }
                    }
                }
            }
            return TV;
        }
    }

    public class EbMobileVisualization : EbMobileContainer
    {
        public string DataSourceRefId { set; get; }

        public EbScript OfflineQuery { set; get; }

        public EbMobileTableLayout DataLayout { set; get; }
    }
}
