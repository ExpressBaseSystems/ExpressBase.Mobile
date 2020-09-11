using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Services;
using ExpressBase.Mobile.Views.Dynamic;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ExpressBase.Mobile.Helpers
{
    public class EbPageFinder
    {
        public static ContentPage ResolveByContext(EbMobileVisualization vis, EbDataRow row, EbMobilePage page)
        {
            ContentPage renderer = null;

            var container = page.Container;

            try
            {
                if (container is EbMobileForm)
                {
                    if (vis.FormMode == WebFormDVModes.New_Mode)
                        renderer = new FormRender(page, vis.LinkFormParameters, row);
                    else
                    {
                        var map = vis.FormId;
                        if (map == null)
                        {
                            EbLog.Info("form id should be set");
                            throw new Exception("Form rendering exited! due to null value for 'FormId'");
                        }
                        else
                        {
                            int id = Convert.ToInt32(row[map.ColumnName]);
                            if (id <= 0)
                            {
                                EbLog.Info("formid has ivalid value" + id);
                                throw new Exception("Form rendering exited! due to invalid formid");
                            }
                            renderer = new FormRender(page, id);
                        }
                    }
                }
                else if (container is EbMobileVisualization)
                {
                    renderer = new LinkedListRender(page, vis, row);
                }
                else if (container is EbMobileDashBoard)
                {
                    renderer = new DashBoardRender(page, row);
                }
            }
            catch (Exception ex)
            {
                EbLog.Error(ex.Message);
            }
            return renderer;
        }

        public static EbMobilePage GetPage(string Refid)
        {
            EbMobilePage page = null;

            if (string.IsNullOrEmpty(Refid))
                return null;
            try
            {
                MobilePagesWraper wrpr = App.Settings.MobilePages?.Find(item => item.RefId == Refid);
                page = wrpr?.GetPage();
            }
            catch (Exception ex)
            {
                EbLog.Error("Page not found" + ex.Message);
            }
            return page;
        }

        public static List<EbMobileForm> GetOfflineForms()
        {
            List<EbMobileForm> ls = new List<EbMobileForm>();

            var pages = App.Settings.MobilePages ?? new List<MobilePagesWraper>();

            foreach (MobilePagesWraper wraper in pages)
            {
                EbMobilePage mpage = wraper.GetPage();

                if (mpage != null && mpage.Container is EbMobileForm form)
                {
                    if (string.IsNullOrEmpty(form.WebFormRefId))
                        continue;
                    if (mpage.NetworkMode == NetworkMode.Offline || mpage.NetworkMode == NetworkMode.Mixed)
                    {
                        ls.Add(form);
                    }
                }
            }
            return ls;
        }

        public static async Task<bool> ValidateFormRendering(EbMobileForm form)
        {
            string refid = form.BeforeRenderDSRefid;
            bool status = true;

            if (!string.IsNullOrEmpty(refid))
            {
                try
                {
                    MobileVisDataRespnse data = await DataService.Instance.GetDataAsync(refid, 0, 0, null, null, null, false);

                    if (data.TryGetValue())
                    {
                        if (data.TryGetFirstRow(1, out var row))
                        {
                            var render = row[0];

                            if (render != null)
                                status = Convert.ToBoolean(render);
                            else
                                EbLog.Info("Form render validation return true");
                        }
                    }
                    else
                        EbLog.Info("before render returned null or empty");
                }
                catch (Exception ex)
                {
                    EbLog.Info("Error at form render validation api call");
                    EbLog.Info(ex.Message);
                }
            }
            return status;
        }
    }
}
