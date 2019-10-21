using ExpressBase.Mobile.Common.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace ExpressBase.Mobile.Models
{
    class MenuData
    {
        public List<AppDataToMob> Applications { get; set; }
    }

    public class AppDataToMob
    {
        public int AppId { set; get; }

        public string AppName { set; get; }

        public string AppIcon { set; get; }

        public override string ToString()
        {
            return AppName;
        }
    }

    public class ObjectListToMob
    {
        public Dictionary<string, List<ObjWrap>> Objects { set; get; }

        public ObjectListToMob()
        {
            Objects = new Dictionary<string, List<ObjWrap>>();
        }

        public int Count { get; set; }
    }

    public class ObjWrap
    {
        public int Id { get; set; }

        public string ObjName { get; set; }

        public string VersionNumber { get; set; }

        public string Refid { get; set; }

        public int EbObjectType { get; set; }

        public int AppId { get; set; }

        public string Description { get; set; }

        public string EbType { get; set; }

        public string DisplayName { get; set; }

        public bool Favourite { get; set; } = false;
    }

    public class EbObjectWrapper
    {
        public int Id { get; set; }

        public int EbObjectType { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string Status { get; set; }

        public string VersionNumber { get; set; }

        public string Json { get; set; }

        public string RefId { get; set; }

        public string Apps { get; set; }

        public string DisplayName { get; set; }
    }


    public class EbObjectToMobResponse
    {
        public EbObjectWrapper ObjectWraper { set; get; }

        public byte[] ReportResult { get; set; }

        public EbDataSet TableResult { get; set; }
    }
}
