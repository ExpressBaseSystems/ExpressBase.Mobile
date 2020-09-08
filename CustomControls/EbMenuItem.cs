using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using System;
using System.Text.RegularExpressions;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace ExpressBase.Mobile.CustomControls
{
    [Preserve(AllMembers = true)]
    public class EbMenuItem : Frame
    {
        public MobilePagesWraper PageWraper { set; get; }

        public EbMenuItem() { }

        private readonly Grid content;

        public EbMenuItem(MobilePagesWraper wrpr)
        {
            this.PageWraper = wrpr;

            content = new Grid
            {
                RowDefinitions = {
                    new RowDefinition { Height = new GridLength(80) },
                    new RowDefinition { Height = GridLength.Auto }
                }
            };
            this.Content = content;

            this.AppendIcon();
            this.AppendLabel();
        }

        private void AppendIcon()
        {
            var iconFrame = new Frame
            {
                BackgroundColor = PageWraper.GetIconBackground(),
                Style = (Style)HelperFunctions.GetResourceValue("MenuIconFrame"),
                Content = new Label
                {
                    Text = this.GetIcon(),
                    TextColor = PageWraper.GetIconColor(),
                    Style = (Style)HelperFunctions.GetResourceValue("MenuIconLabel")
                }
            };
            content.Children.Add(iconFrame);
            Grid.SetRow(iconFrame, 0);
        }

        private void AppendLabel()
        {
            Label name = new Label
            {
                Text = PageWraper.DisplayName,
                Style = (Style)HelperFunctions.GetResourceValue("MenuDisplayNameLabel")
            };

            content.Children.Add(name);
            Grid.SetRow(name, 1);
        }

        private string GetIcon()
        {
            string labelIcon;
            try
            {
                if (PageWraper.ObjectIcon.Length != 4)
                    throw new Exception();
                labelIcon = Regex.Unescape("\\u" + PageWraper.ObjectIcon);
            }
            catch (Exception ex)
            {
                labelIcon = Regex.Unescape("\\u" + PageWraper.GetDefaultIcon());
                EbLog.Error("font icon format is invalid." + ex.Message);
            }
            return labelIcon;
        }
    }
}
