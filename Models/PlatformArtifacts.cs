using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExpressBase.Mobile.Models
{
    public class AppCollection
    {
        public List<AppData> Applications { get; set; }
    }

    public class AppData
    {
        public int AppId { set; get; }

        public string AppName { set; get; }

        public string AppIcon { set; get; }

        public string Description { set; get; } = "No description";
    }

    public class MobilePageCollection
    {
        public List<MobilePagesWraper> Pages { set; get; }

        public List<EbMobilePage> FilteredList { set; get; }

        public EbDataSet Data { set; get; }

        public List<string> TableNames { set; get; }

        public List<string> DependencyTables { set; get; }

        public MobilePageCollection()
        {
            this.FilteredList = new List<EbMobilePage>();
            this.Pages = new List<MobilePagesWraper>();
        }

        public MobilePageCollection(Type FilterType)
        {
            this.FilteredList = new List<EbMobilePage>();
            this.Pages = Settings.Objects;

            foreach (MobilePagesWraper pages in this.Pages)
            {
                EbMobilePage mpage = pages.JsonToPage();
                if (mpage != null && mpage.Container.GetType() == FilterType)
                {
                    this.FilteredList.Add(mpage);
                }
            }
        }

        public EbMobileForm ResolveDependency(EbMobilePage CurrentForm)
        {
            if (DependencyTables == null)
                DependencyTables = new List<string>();

            EbMobileForm _Form = CurrentForm.Container as EbMobileForm;

            if (!string.IsNullOrEmpty(_Form.AutoGenMVRefid))
            {
                var autogenvis = HelperFunctions.GetPage(_Form.AutoGenMVRefid);

                string linkref = (autogenvis.Container as EbMobileVisualization).LinkRefId;

                if (!string.IsNullOrEmpty(linkref))
                {
                    var linkpage = HelperFunctions.GetPage(linkref);

                    if(linkpage.Container is EbMobileVisualization)
                    {
                        if(!string.IsNullOrEmpty((linkpage.Container as EbMobileVisualization).LinkRefId))
                        {
                            var innerlink = HelperFunctions.GetPage((linkpage.Container as EbMobileVisualization).LinkRefId);

                            if(innerlink.Container is EbMobileForm)
                            {
                                DependencyTables.Add((innerlink.Container as EbMobileForm).TableName);
                                return (innerlink.Container as EbMobileForm);
                            }
                        }
                    }
                }
            }
            return null;
        }
    }

    public class PageTypeGroup : List<MobilePagesWraper>
    {
        public string GroupHeader { get; set; }

        public IList<MobilePagesWraper> Pages => this;
    }

    public class MobilePagesWraper
    {
        private EbMobilePage _page;

        public string ObjectIcon
        {
            get
            {
                if (_page == null)
                    _page = this.JsonToPage();

                if (_page.Container is EbMobileForm)
                {
                    return "form.png";
                }
                else if (_page.Container is EbMobileVisualization)
                {
                    return "list.png";
                }
                else
                {
                    return "list.png";
                }
            }
        }

        public bool IsVisble
        {
            get
            {
                if (_page == null)
                    _page = this.JsonToPage();
                return !(_page.HideFromMenu);
            }
        }

        public string DisplayName { set; get; }

        public string Name { set; get; }

        public string Version { set; get; }

        public string Json { set; get; }

        public string RefId { set; get; }

        public EbMobilePage JsonToPage()
        {
            if (string.IsNullOrEmpty(Json))
                return null;
            string regexed = EbSerializers.JsonToNETSTD(this.Json);
            return EbSerializers.Json_Deserialize<EbMobilePage>(regexed);
        }
    }

    public class ValidateSidResponse
    {
        public bool IsValid { set; get; }

        public byte[] Logo { set; get; }
    }

    public class VisualizationLiveData
    {
        public string Message { set; get; }

        public EbDataSet Data { set; get; }
    }

    public class ApiFileData
    {
        public string FileName { set; get; }

        public string FileType { set; get; }

        public int FileRefId { set; get; }
    }
}
