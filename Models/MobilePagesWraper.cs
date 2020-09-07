using ExpressBase.Mobile.Helpers;
using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using Xamarin.Forms;

namespace ExpressBase.Mobile.Models
{
    public class MobilePagesWraper
    {
        private EbMobilePage page;

        public string DisplayName { set; get; }

        public string Name { set; get; }

        public string Version { set; get; }

        public string Json { set; get; }

        public string RefId { set; get; }

        [JsonIgnore]
        public bool IsHidden => page.HideFromMenu;

        [JsonIgnore]
        public string Category => page?.Category;

        [JsonIgnore]
        public string ObjectIcon => string.IsNullOrEmpty(page.Icon) ? this.GetDefaultIcon() : page.Icon;

        private EbMobilePage DeserializeJsonPage()
        {
            EbMobilePage mpage = null;
            try
            {
                string regexed = EbSerializers.JsonToNETSTD(this.Json);
                mpage = EbSerializers.Json_Deserialize<EbMobilePage>(regexed);
            }
            catch (Exception ex)
            {
                EbLog.Info("DeserializeJsonPage error inside pagewrapper");
                EbLog.Error(ex.Message);
            }
            return mpage;
        }

        public string GetDefaultIcon()
        {
            if (page.Container is EbMobileForm)
                return "f298";
            else if (page.Container is EbMobileVisualization)
                return "f03a";//"f022";
            else if (page.Container is EbMobileDashBoard)
                return "f0e4";
            else if (page.Container is EbMobilePdf)
                return "f1c1";
            else
                return "f0e4";
        }

        public Color GetIconColor()
        {
            return string.IsNullOrEmpty(page.IconColor) ? Color.FromHex("0046bb") : Color.FromHex(page.IconColor);
        }

        public Color GetIconBackground()
        {
            return string.IsNullOrEmpty(page.IconBackground) ? Color.White : Color.FromHex(page.IconBackground);
        }

        public EbMobilePage GetPage()
        {
            return page ?? DeserializeJsonPage();
        }

        [OnDeserialized()]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            page = this.DeserializeJsonPage();
        }
    }
}
