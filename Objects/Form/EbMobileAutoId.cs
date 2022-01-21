using ExpressBase.Mobile.CustomControls;
using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Structures;
using Xamarin.Forms;

namespace ExpressBase.Mobile
{
    public class EbMobileAutoId : EbMobileControl
    {
        public override bool ReadOnly { get { return true; } }

        public override EbDbTypes EbDbType { get { return EbDbTypes.String; } set { } }

        public int SerialLength { get; set; }

        public EbScript PrefixExpr { get; set; }

        public override View Draw(FormMode Mode, NetworkMode Network)
        {
            XControl = new EbXTextBox
            {
                IsReadOnly = true,
                XBackgroundColor = EbMobileControl.ReadOnlyBackground
            };
            return base.Draw(Mode, Network);
        }

        public override void SetValue(object value)
        {
            (this.XControl as EbXTextBox).Text = value?.ToString();
        }

        public override object GetValue()
        {
            return (this.XControl as EbXTextBox).Text;
        }

        public override bool Validate()
        {
            bool IsNull = string.IsNullOrWhiteSpace((this.XControl as EbXTextBox).Text);

            if (FormRenderMode != FormMode.EDIT && IsNull && NetworkType == NetworkMode.Offline)
                return false;

            return true;
        }

        public void InitAutoId(string Table)
        {
            string query = PrefixExpr?.GetCode();
            if (!string.IsNullOrWhiteSpace(query) && NetworkType == NetworkMode.Offline)
            {
                string prefix = null, idSyncd = null, idMax = null, idNxt = null;

                EbDataTable dt = App.DataDB.DoQuery(query);
                if (dt.Rows.Count > 0)
                    prefix = dt.Rows[0][0]?.ToString();

                dt = App.DataDB.DoQuery($"SELECT val FROM eb_latest_autoid WHERE key = '{Table}_{Name}'");
                if (dt.Rows.Count > 0)
                    idSyncd = dt.Rows[0][0]?.ToString();

                dt = App.DataDB.DoQuery($"SELECT MAX({Name}) FROM {Table} WHERE {Name} LIKE '{prefix}%' LIMIT 1");
                if (dt.Rows.Count > 0)
                    idMax = dt.Rows[0][0]?.ToString();

                if (!string.IsNullOrWhiteSpace(idSyncd) && !string.IsNullOrWhiteSpace(idMax))
                {
                    if (string.Compare(idSyncd, idMax, true) > 0)
                        idNxt = idSyncd;
                    else
                        idNxt = idMax;
                }
                else if (string.IsNullOrWhiteSpace(idSyncd) && !string.IsNullOrWhiteSpace(idMax))
                    idNxt = idMax;
                else if (!string.IsNullOrWhiteSpace(idSyncd) && string.IsNullOrWhiteSpace(idMax))
                    idNxt = idSyncd;
                else if (string.IsNullOrWhiteSpace(idSyncd) && string.IsNullOrWhiteSpace(idMax))
                    idNxt = prefix + "0".PadLeft(SerialLength, '0');

                int serialval;
                int.TryParse(idNxt.Substring(prefix.Length), out serialval);
                serialval++;
                idNxt = prefix + serialval.ToString().PadLeft(SerialLength, '0');

                SetValue(idNxt);
            }
        }
    }
}
