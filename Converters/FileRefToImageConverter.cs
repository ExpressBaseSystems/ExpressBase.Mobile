using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Services;
using System;
using System.Globalization;
using System.IO;
using Xamarin.Forms;

namespace ExpressBase.Mobile.Converters
{
    public class FileRefToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return ImageSource.FromFile("image_avatar.jpg");

            ImageSource source = null;

            string refid = value.ToString().Split(CharConstants.COMMA)[0];

            try
            {
                byte[] file = DataService.Instance.GetLocalFile($"{refid}.jpg");

                if (file == null)
                {
                    ApiFileResponse resp = DataService.Instance.GetFile(EbFileCategory.Images, $"{refid}.jpg");
                    if (resp.HasContent)
                    {
                        source = ImageSource.FromStream(() => { return new MemoryStream(resp.Bytea); });
                        this.CacheImage(refid, resp.Bytea);
                    }
                }
                else
                    source = ImageSource.FromStream(() => { return new MemoryStream(file); });
            }
            catch (Exception)
            {
                EbLog.Error("failed to load image ,getfile api error");
            }
            return source;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private void CacheImage(string filename, byte[] fileBytea)
        {
            HelperFunctions.WriteFilesLocal(filename + ".jpg", fileBytea);
        }
    }
}
