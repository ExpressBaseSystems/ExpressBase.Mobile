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
    public class EbPageHelper
    {
        public static ContentPage ResolveByContext(EbMobileVisualization vis, EbDataRow row, EbMobilePage page)
        {
            ContentPage renderer = null;
            EbMobileContainer container = page.Container;
            try
            {
                if (container is EbMobileForm)
                {
                    if (vis.FormMode == WebFormDVModes.New_Mode)
                        renderer = new FormRender(page, vis.LinkFormParameters, row);
                    else
                    {
                        EbMobileDataColToControlMap map = vis.FormId;
                        if (map == null)
                        {
                            EbLog.Info("form id must be set");
                            throw new Exception("Form rendering exited! due to null value for 'FormId'");
                        }
                        else
                        {
                            int id = Convert.ToInt32(row[map.ColumnName]);
                            if (id <= 0)
                            {
                                EbLog.Info($"formid has invalid value {id}, switching to new mode");
                                renderer = new FormRender(page, vis.LinkFormParameters, row);
                            }
                            else
                            {
                                EbLog.Info($"formid has value {id}, rendering edit mode");
                                renderer = new FormRender(page, id);
                            }
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
            if (string.IsNullOrEmpty(Refid))
                return null;
            try
            {
                MobilePagesWraper wrpr = App.Settings.MobilePages?.Find(item => item.RefId == Refid);
                return wrpr?.GetPage();
            }
            catch (Exception ex)
            {
                EbLog.Error("Page not found" + ex.Message);
            }
            return null;
        }

        public static EbMobilePage GetExternalPage(string Refid)
        {
            if (string.IsNullOrEmpty(Refid))
                return null;
            try
            {
                MobilePagesWraper wrpr = App.Settings.ExternalMobilePages?.Find(item => item.RefId == Refid);
                return wrpr?.GetPage();
            }
            catch (Exception ex)
            {
                EbLog.Error("external page not found, " + ex.Message);
            }
            return null;
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

        public static async Task<bool> ValidateFormRendering(EbMobileForm form, EbDataRow context = null)
        {
            if (string.IsNullOrEmpty(form.RenderValidatorRefId))
                return true;

            bool status = true;

            try
            {
                List<Param> cParams = form.GetRenderValidatorParams(context);

                cParams.Add(new Param
                {
                    Name = "eb_loc_id",
                    Type = "11",
                    Value = App.Settings.CurrentLocation.LocId.ToString()
                });

                MobileVisDataRespnse data = await DataService.Instance.GetDataAsync(form.RenderValidatorRefId, 0, 0, cParams, null, null, false);

                if (data.HasData() && data.TryGetFirstRow(1, out EbDataRow row))
                {
                    var render = row[0];

                    if (render != null)
                        status = Convert.ToBoolean(render);
                    else
                        EbLog.Info("Form render validation return true");
                }
                else
                    EbLog.Info("before render api returned empty row collection");
            }
            catch (Exception ex)
            {
                EbLog.Info("Error at form render validation api call");
                EbLog.Info(ex.Message);
            }
            return status;
        }

        public static ContentPage GetPageByContainer(EbMobilePage page)
        {
            ContentPage renderer = null;
            try
            {
                switch (page.Container)
                {
                    case EbMobileForm f:
                        renderer = new FormRender(page);
                        break;
                    case EbMobileVisualization v:
                        if (v.Type == MobileVisualizationType.Dynamic)
                            renderer = new ListRender(page);
                        else
                            renderer = new StaticListRender(page);
                        break;
                    case EbMobileDashBoard d:
                        renderer = new DashBoardRender(page);
                        break;
                    case EbMobilePdf p:
                        renderer = new PdfRender(page);
                        break;
                    default:
                        EbLog.Error("inavlid container type");
                        break;
                }
            }
            catch (Exception ex)
            {
                EbLog.Error(ex.Message);
            }
            return renderer;
        }
    }
}
