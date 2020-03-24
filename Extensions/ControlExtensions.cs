using ExpressBase.Mobile.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace ExpressBase.Mobile.Extensions
{
    public static class ControlExtensions
    {
        public static EbMobileForm ResolveDependency(this EbMobileForm sourceForm)
        {
            try
            {
                if (!string.IsNullOrEmpty(sourceForm.AutoGenMVRefid))
                {
                    var autogenvis = HelperFunctions.GetPage(sourceForm.AutoGenMVRefid);

                    if (autogenvis != null)
                    {
                        string linkref = (autogenvis.Container as EbMobileVisualization).LinkRefId;

                        if (!string.IsNullOrEmpty(linkref))
                        {
                            var linkpage = HelperFunctions.GetPage(linkref);

                            if (linkpage != null && linkpage.Container is EbMobileVisualization)
                            {
                                if (!string.IsNullOrEmpty((linkpage.Container as EbMobileVisualization).LinkRefId))
                                {
                                    var innerlink = HelperFunctions.GetPage((linkpage.Container as EbMobileVisualization).LinkRefId);

                                    if (innerlink != null && innerlink.Container is EbMobileForm)
                                        return (innerlink.Container as EbMobileForm);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return null;
        }
    }
}
