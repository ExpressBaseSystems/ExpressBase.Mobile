using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace ExpressBase.Mobile.Helpers
{
    public class HelperFunctions
    {
        public static EbMobilePage GetPage(string Refid)
        {
            string _objlist = Store.GetValue(AppConst.OBJ_COLLECTION);
            List<MobilePagesWraper> _list = JsonConvert.DeserializeObject<List<MobilePagesWraper>>(_objlist);
            MobilePagesWraper Wrpr = _list.Find(item => item.RefId == Refid);

            string regexed = EbSerializers.JsonToNETSTD(Wrpr.Json);
            return EbSerializers.Json_Deserialize<EbMobilePage>(regexed);
        }

        public static string WrapSelectQuery(string sql)
        {
            return string.Format("SELECT * FROM ({0}) AS WRAPER LIMIT 100;", sql.TrimEnd(';'));
        }

        public static object GetResourceValue(string keyName)
        {
            // Search all dictionaries
            if (Application.Current.Resources.TryGetValue(keyName, out var retVal)) { }
            return retVal;
        }
    }
}
