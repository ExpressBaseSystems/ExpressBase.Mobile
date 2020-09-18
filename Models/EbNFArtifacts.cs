using Newtonsoft.Json;
using System.Collections.Generic;
using Xamarin.Forms;

namespace ExpressBase.Mobile.Models
{
    public enum PNSPlatforms
    {
        GCM = 1,
        APNS = 2,
        WNS = 3,
        ALL = 4
    }

    public enum EbAppVendors
    {
        ExpressBase = 1,
        MoveOn = 2,
        kudumbaShree = 3
    }

    public enum EbNFLinkTypes
    {
        Page = 1,
        Action = 2
    }

    public class DeviceRegistration
    {
        public PNSPlatforms Platform { get; set; }

        public string Handle { get; set; }

        public List<string> Tags { get; set; }

        public EbAppVendors Vendor { set; get; }

        public DeviceRegistration()
        {
            Platform = Device.RuntimePlatform switch
            {
                Device.iOS => PNSPlatforms.APNS,
                Device.Android => PNSPlatforms.GCM,
                Device.UWP => PNSPlatforms.WNS,
                _ => default,
            };
        }
    }

    public class EbNFRegisterResponse
    {
        public string Message { set; get; }

        public bool Status { set; get; }

        public EbNFRegisterResponse() { }

        public EbNFRegisterResponse(string message)
        {
            Message = message;
        }
    }

    public class EbNFRequest : DeviceRegistration
    {
        public string PayLoad { set; get; }

        public void SetPayload(EbNFDataTemplate template)
        {
            this.PayLoad = JsonConvert.SerializeObject(template);
        }
    }

    public class EbNFResponse
    {
        public bool Status { set; get; }

        public string Message { set; get; }

        public EbNFResponse() { }

        public EbNFResponse(string message)
        {
            Message = message;
        }
    }

    public class EbNFDataTemplate
    {

    }

    public class EbNFData
    {
        public string Title { set; get; }

        public string Message { set; get; }

        public EbNFLink Link { set; get; }
    }

    public class EbNFLink
    {
        public EbNFLinkTypes LinkType { set; get; }

        public string LinkRefId { set; get; }

        public int ActionId { set; get; }

        public Param LinkParameters { set; get; }
    }

    public class EbNFDataTemplateAndroid : EbNFDataTemplate
    {
        [JsonProperty("data")]
        public EbNFData Data { set; get; }

        public EbNFDataTemplateAndroid()
        {
            Data = new EbNFData();
        }
    }
}
