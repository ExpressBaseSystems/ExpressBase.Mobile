using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Views.Dynamic;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ExpressBase.Mobile
{
    public class EbMobileButton : EbMobileControl, INonPersistControl, IMobileLink, IGridAlignment, IMobileUIControl
    {
        public string LinkRefId { get; set; }

        public WebFormDVModes FormMode { set; get; }

        public EbMobileDataColToControlMap FormId { set; get; }

        public List<EbMobileDataColToControlMap> LinkFormParameters { get; set; }

        public string Text { set; get; }

        public EbFont Font { get; set; }

        public bool RenderTextAsIcon { get; set; }

        public int Width { set; get; }

        public int Height { set; get; }

        public int RowSpan { set; get; }

        public int ColumnSpan { set; get; }

        public int BorderThickness { get; set; }

        public int BorderRadius { get; set; }

        public string BorderColor { get; set; }

        public bool Transparent { get; set; }

        public string BackgroundColor { get; set; }

        public MobileHorrizontalAlign HorrizontalAlign { set; get; }

        public MobileVerticalAlign VerticalAlign { set; get; }

        public EbThickness Padding { set; get; }

        public bool HideInContext { set; get; }

        public override View Draw()
        {
            Button btn = new Button
            {
                BackgroundColor = Color.FromHex(this.BackgroundColor),
                BorderWidth = this.BorderThickness,
                BorderColor = Color.FromHex(this.BorderColor),
                CornerRadius = this.BorderRadius,
                WidthRequest = this.Width,
                HeightRequest = this.Height,
                Padding = 0
            };

            SetText(btn);

            if (Font != null)
            {
                btn.FontSize = Font.Size;
                btn.TextColor = Color.FromHex(Font.Color);
            }
            return btn;
        }

        public override View Draw(EbDataRow row)
        {
            return Draw();
        }

        public void SetText(Button btn)
        {
            if (this.RenderTextAsIcon)
            {
                btn.FontFamily = (OnPlatform<string>)HelperFunctions.GetResourceValue("FontAwesome");

                if (string.IsNullOrEmpty(this.Text))
                    btn.Text = "\uf192";
                else
                {
                    try
                    {
                        if (this.Text.Length != 4)
                            throw new Exception();
                        btn.Text = Regex.Unescape("\\u" + this.Text);
                    }
                    catch
                    {
                        btn.Text = "\uf192";
                    }
                }
            }
            else
                btn.Text = this.Text ?? "Button";
        }

        public async virtual Task OnControlAction(EbDataRow row)
        {
            if (string.IsNullOrEmpty(this.LinkRefId))
                return;

            EbMobilePage page = EbPageHelper.GetPage(this.LinkRefId) ?? EbPageHelper.GetExternalPage(this.LinkRefId);

            if (page != null)
            {
                EbMobileContainer container = page.Container;

                if (container is EbMobileForm)
                {
                    if (this.FormMode == WebFormDVModes.New_Mode)
                        await App.Navigation.NavigateMasterAsync(new FormRender(page, this.LinkFormParameters, row));
                    else
                    {
                        try
                        {
                            var map = this.FormId;
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
                                    EbLog.Info("id has ivalid value" + id);
                                    throw new Exception("Form rendering exited! due to invalid id");
                                }
                                await App.Navigation.NavigateMasterAsync(new FormRender(page, id));
                            }
                        }
                        catch (Exception ex)
                        {
                            EbLog.Error(ex.Message);
                        }
                    }
                }
                else if (container is EbMobileVisualization)
                {
                    await App.Navigation.NavigateMasterAsync(new ListRender(page, row));
                }
            }
        }
    }
}
