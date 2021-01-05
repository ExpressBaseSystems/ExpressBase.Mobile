using ExpressBase.Mobile.Enums;
using Xamarin.Forms;

namespace ExpressBase.Mobile.Extensions
{
    public static class XViewExtensions
    {
        public static void SetHorrizontalAlign(this View view, MobileHorrizontalAlign align)
        {
            switch (align)
            {
                case MobileHorrizontalAlign.Center:
                    view.HorizontalOptions = LayoutOptions.Center;
                    break;
                case MobileHorrizontalAlign.Right:
                    view.HorizontalOptions = LayoutOptions.End;
                    break;
                case MobileHorrizontalAlign.Left:
                    view.HorizontalOptions = LayoutOptions.Start;
                    break;
                case MobileHorrizontalAlign.Fill:
                    view.HorizontalOptions = LayoutOptions.FillAndExpand;
                    break;
                default:
                    break;
            }
        }

        public static void SetVerticalAlign(this View view, MobileVerticalAlign align)
        {
            switch (align)
            {
                case MobileVerticalAlign.Center:
                    view.VerticalOptions = LayoutOptions.Center;
                    break;
                case MobileVerticalAlign.Bottom:
                    view.VerticalOptions = LayoutOptions.End;
                    break;
                case MobileVerticalAlign.Top:
                    view.VerticalOptions = LayoutOptions.Start;
                    break;
                case MobileVerticalAlign.Fill:
                    view.VerticalOptions = LayoutOptions.FillAndExpand;
                    break;
                default:
                    break;
            }
        }

        public static void SetFont(this Label label, EbFont font, bool isHeader = false)
        {
            if (font != null)
            {
                label.FontSize = font.Size;
                label.TextColor = isHeader ? Color.White : Color.FromHex(font.Color);

                switch (font.Style)
                {
                    case FontStyle.BOLD:
                        label.FontAttributes = FontAttributes.Bold;
                        break;
                    case FontStyle.ITALIC:
                        label.FontAttributes = FontAttributes.Italic;
                        break;
                    case FontStyle.BOLDITALIC:
                        label.FontAttributes = FontAttributes.Italic;
                        break;
                    default:
                        label.FontAttributes = FontAttributes.None;
                        break;
                }

                if (font.Caps) label.TextTransform = Xamarin.Forms.TextTransform.Uppercase;

                if (font.Underline)
                    label.TextDecorations = TextDecorations.Underline;
                else if (font.Strikethrough)
                    label.TextDecorations = TextDecorations.Strikethrough;
                else
                    label.TextDecorations = TextDecorations.None;
            }
        }

        public static void SetTextWrap(this Label label, MobileTextWrap textwrap)
        {
            switch (textwrap)
            {
                case MobileTextWrap.HeadTruncation:
                    label.LineBreakMode = LineBreakMode.HeadTruncation;
                    break;
                case MobileTextWrap.CharacterWrap:
                    label.LineBreakMode = LineBreakMode.CharacterWrap;
                    break;
                case MobileTextWrap.MiddleTruncation:
                    label.LineBreakMode = LineBreakMode.MiddleTruncation;
                    break;
                case MobileTextWrap.TailTruncation:
                    label.LineBreakMode = LineBreakMode.TailTruncation;
                    break;
                case MobileTextWrap.WordWrap:
                    label.LineBreakMode = LineBreakMode.WordWrap;
                    break;
                default:
                    label.LineBreakMode = LineBreakMode.NoWrap;
                    break;
            }
        }

        public static void SetTextAlignment(this Label label, MobileTextAlign horriTextAlign, MobileTextAlign verticalTextAlign)
        {
            switch (horriTextAlign)
            {
                case MobileTextAlign.Center:
                    label.HorizontalTextAlignment = TextAlignment.Center;
                    break;
                case MobileTextAlign.End:
                    label.HorizontalTextAlignment = TextAlignment.End;
                    break;
                default:
                    label.HorizontalTextAlignment = TextAlignment.Start;
                    break;
            }

            switch (verticalTextAlign)
            {
                case MobileTextAlign.Center:
                    label.VerticalTextAlignment = TextAlignment.Center;
                    break;
                case MobileTextAlign.End:
                    label.VerticalTextAlignment = TextAlignment.End;
                    break;
                default:
                    label.VerticalTextAlignment = TextAlignment.Start;
                    break;
            }
        }
    }
}
