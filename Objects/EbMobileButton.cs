using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Helpers;
using System;
using System.Text.RegularExpressions;
using Xamarin.Forms;

namespace ExpressBase.Mobile
{
    public class EbMobileButton : EbMobileControl, IMobileUIStyles, IMobileLink, INonPersistControl
    {
        public string LinkRefId { get; set; }

        public string Text { set; get; }

        public EbFont Font { get; set; }

        public bool RenderTextAsIcon { get; set; }

        public int Width { set; get; }

        public int Height { set; get; }

        public int RowSpan { set; get; }

        public int ColumnSpan { set; get; }

        public string BackgroundColor { get; set; }

        public int BorderThickness { get; set; }

        public string BorderColor { get; set; }

        public int BorderRadius { get; set; }

        public MobileHorrizontalAlign HorrizontalAlign { set; get; }

        public MobileVerticalAlign VerticalAlign { set; get; }

        public bool HideInContext { set; get; }

        public override void InitXControl()
        {
            base.InitXControl();
        }

        public override void InitXControl(FormMode Mode, NetworkMode Network)
        {
            base.InitXControl(Mode, Network);

            this.XControl = CreateView();
        }

        public Button CreateView()
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
            SetFontStyle(btn);

            return btn;
        }

        public void SetText(Button btn)
        {
            if (this.RenderTextAsIcon)
            {
                btn.FontFamily = (OnPlatform<string>)HelperFunctions.GetResourceValue("FontAwesome");

                if (string.IsNullOrEmpty(this.Text))
                    btn.Text = "\\uf192";
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
                        btn.Text = "\\uf192";
                    }
                }
            }
            else
                btn.Text = this.Text ?? "Button";
        }

        public void SetFontStyle(Button btn)
        {
            if (Font != null)
            {
                btn.FontSize = Font.Size;
                btn.TextColor = Color.FromHex(Font.Color);
            }
        }
    }
}
