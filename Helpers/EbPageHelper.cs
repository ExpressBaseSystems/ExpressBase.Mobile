﻿using ExpressBase.Mobile.CustomControls;
using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Services;
using ExpressBase.Mobile.Views.Dynamic;
using ExpressBase.Mobile.Views.Shared;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace ExpressBase.Mobile.Helpers
{
    public class EbPageHelper
    {
        public static async Task<ContentPage> ResolveByContext(EbMobileVisualization vis, EbDataRow row, EbMobilePage page)
        {
            ContentPage renderer = null;
            EbMobileContainer container = page.Container;
            try
            {
                if (container is EbMobileForm form)
                {
                    if (vis.FormMode == WebFormDVModes.New_Mode)
                    {
                        string msg = await GetFormRenderInvalidateMsg(page.NetworkMode);
                        if (msg != null && !form.RenderAsFilterDialog)
                            renderer = new Redirect(msg, MessageType.disconnected);
                        else
                            renderer = new FormRender(page, vis.LinkFormParameters, row);
                    }
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
                    renderer = new DashBoardRender(page);
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
        public static EbObject GetWebObjects(string Refid)
        {
            if (string.IsNullOrEmpty(Refid))
                return null;
            try
            {
                WebObjectsWraper wrpr = App.Settings.WebObjects?.Find(item => item.RefId == Refid);
                return wrpr?.GetObject();
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
                    form.DisplayName = mpage.DisplayName;
                    if (mpage.NetworkMode == NetworkMode.Offline || mpage.NetworkMode == NetworkMode.Mixed)
                    {
                        ls.Add(form);
                    }
                }
            }
            return ls;
        }

        public static async Task<string> ValidateFormRendering(EbMobileForm form, Loader loader, EbDataRow context = null)
        {
            string failMsg = null;

            if (!Utils.IsNetworkReady(form.NetworkType))
            {
                failMsg = "Not connected to internet!";
            }
            else if (!string.IsNullOrEmpty(form.RenderValidatorRefId))
            {
                try
                {
                    if (loader != null) loader.Message = "Validating...";
                    List<Param> cParams = form.GetRenderValidatorParams(context);

                    cParams.Add(new Param
                    {
                        Name = "eb_loc_id",
                        Type = "11",
                        Value = App.Settings.CurrentLocation.LocId.ToString()
                    });

                    MobileDataResponse data = await DataService.Instance.GetDataAsync(form.RenderValidatorRefId, 0, 0, cParams, null, null, false);

                    if (data.HasData() && data.TryGetFirstRow(1, out EbDataRow row))
                    {
                        if (bool.TryParse(row[0]?.ToString(), out bool b) && b)
                        {
                            EbLog.Info("Form render validation return true");
                        }
                        else
                        {
                            failMsg = form.MessageOnFailed;
                            EbLog.Info("Form render validation return false");
                        }
                    }
                    else
                    {
                        failMsg = "Validation api returned empty data";
                        EbLog.Info("before render api returned empty row collection");
                    }
                }
                catch (Exception ex)
                {
                    failMsg = "Exception in validation api: " + ex.Message;
                    EbLog.Error("Error at form render validation api call");
                    EbLog.Info(ex.Message);
                }
            }

            if (failMsg == null && form.AutoSyncOnLoad && !form.RenderAsFilterDialog)
            {
                LocalDBServie service = new LocalDBServie();
                if (!App.Settings.SyncInProgress)
                {
                    App.Settings.SyncInProgress = true;
                    failMsg = await service.PushData(loader);
                    App.Settings.SyncInProgress = false;
                }
                else
                {
                    failMsg = "Internal error. (SyncInProgress is true)";
                    EbLog.Info("ValidateFormRendering -> SyncInProgress is true");
                }
            }

            return failMsg;
        }

        public static async Task<string> GetFormRenderInvalidateMsg(NetworkMode mode, EbMobileForm Form = null)
        {
            string msg = null;
            if (mode == NetworkMode.Offline)
            {
                LastSyncInfo syncInfo = App.Settings.SyncInfo;
                if (syncInfo == null || !syncInfo.PullSuccess)
                    msg = "Sync required (Last pull failed)";
                else if (syncInfo.LastSyncTs.AddDays(1) < DateTime.Now)
                    msg = $"Sync required (Last sync was {(int)(DateTime.Now - syncInfo.LastSyncTs).TotalHours} hours back)";
            }
            if (Form != null && msg == null)
            {
                EbMobileGeoLocation ctrl = (EbMobileGeoLocation)Form.ChildControls.Find(e => e is EbMobileGeoLocation);
                if (ctrl != null && ctrl.CurrentLocRequired && ctrl.CurrentLocation == null)
                {
                    bool hasPermission = await AppPermission.GPSLocation();
                    Location current = null;
                    if (hasPermission)
                        current = await EbGeoLocationHelper.GetCurrentLocationAsync();
                    if (current == null)
                        msg = "Location permission required";
                }
            }
            return msg;
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

        public static async Task<EbPageRenderer> GetRenderer(EbMobilePage page)
        {
            EbPageRenderer renderor = new EbPageRenderer();

            EbMobileContainer container = page.Container;

            try
            {
                if (container is EbMobileForm mobileForm)
                {
                    mobileForm.NetworkType = page.NetworkMode;
                    renderor.Message = await ValidateFormRendering(mobileForm, null);
                    renderor.IsReady = renderor.Message == null;
                    renderor.Renderer = new FormRender(page);
                }
                else if (container is EbMobileVisualization viz)
                {
                    if (viz.Type == MobileVisualizationType.Dynamic)
                        renderor.Renderer = new ListRender(page);
                    else
                        renderor.Renderer = new StaticListRender(page);
                }
                else if (container is EbMobileDashBoard)
                {
                    renderor.Renderer = new DashBoardRender(page);
                }
                else if (container is EbMobileDashBoard)
                {
                    renderor.Renderer = new PdfRender(page);
                }
                else
                {
                    renderor.Message = "Inavlid container type";
                    renderor.IsReady = false;
                }
            }
            catch (Exception ex)
            {
                EbLog.Error(ex.Message);

                renderor.IsReady = false;
                renderor.Message = "Unable to load page";
            }

            if (renderor.Renderer != null)
                renderor.IsReady = true;

            return renderor;
        }

        private static bool _isTapped { get; set; }

        public static bool IsShortTap()
        {
            if (_isTapped)
                return true;
            _isTapped = true;
            Task task = Task.Run(async () =>
            {
                await Task.Delay(1000);
                _isTapped = false;
            });
            return false;
        }

        ///temp implementation
        public static bool HasEditPermission(User UserObj, string RefId)
        {
            if (UserObj.Roles.Contains(SystemRoles.SolutionOwner.ToString()) ||
                UserObj.Roles.Contains(SystemRoles.SolutionAdmin.ToString()) ||
                UserObj.Roles.Contains(SystemRoles.SolutionPM.ToString()))
                return true;

            try
            {
                //Permission string format => 020-00-00982-02:5
                string[] refidParts = RefId.Split('-');
                string objType = refidParts[2].PadLeft(2, '0');
                string objId = refidParts[3].PadLeft(5, '0');
                string operation = "02";//Edit
                string perm = objType + '-' + objId + '-' + operation;
                string temp = UserObj.Permissions.Find(p => p.Contains(perm));
                if (temp != null)
                    return true;
            }
            catch (Exception e)
            {
                EbLog.Error(string.Format("Exception when checking user permission: {0}  RefId = {1}", e.Message, RefId));
            }

            return false;
        }
    }

}
