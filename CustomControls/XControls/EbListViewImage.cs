using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Services;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ExpressBase.Mobile.CustomControls
{
    public class EbListViewImage : Image, IDynamicHeight
    {
        public bool CalcHeight { set; get; }

        public void SetDimensions(EbMobileDataColumn dc)
        {
            if (dc.VerticalAlign == MobileVerticalAlign.Fill)
                this.CalcHeight = true;
            else
                this.HeightRequest = dc.Height;

            if (dc.HorrizontalAlign != MobileHorrizontalAlign.Fill)
                this.WidthRequest = dc.Width;
        }

        public async void SetValue(object value)
        {
            if (value == null)
            {
                this.Source = ImageSource.FromFile("image_avatar.jpg");
                return;
            }

            string refid = value.ToString().Split(CharConstants.COMMA)[0];
            string fileName = $"{App.Settings.ISid}-{refid}.jpg";

            try
            {
                byte[] file = DataService.Instance.GetLocalFile(fileName);

                if (file == null)
                {
                    ApiFileResponse resp = await this.GetImageAsync($"{refid}.jpg");
                    if (resp.HasContent)
                    {
                        this.Source = ImageSource.FromStream(() => { return new MemoryStream(resp.Bytea); });
                        this.CacheImage(fileName, resp.Bytea);
                    }
                }
                else
                    this.Source = ImageSource.FromStream(() => { return new MemoryStream(file); });
            }
            catch (Exception)
            {
                this.Source ??= ImageSource.FromFile("image_avatar.jpg");
                EbLog.Error("failed to load image");
            }
        }

        public async Task<ApiFileResponse> GetImageAsync(string filename)
        {
            ApiFileResponse resp = null;
            try
            {
                RestClient client = new RestClient(App.Settings.RootUrl);

                RestRequest request = new RestRequest("api/get_file", Method.GET);
                // auth Headers for api
                request.AddHeader(AppConst.BTOKEN, App.Settings.BToken);
                request.AddHeader(AppConst.RTOKEN, App.Settings.RToken);

                request.AddParameter("category", (int)EbFileCategory.Images);
                request.AddParameter("filename", filename);

                IRestResponse response = await client.ExecuteAsync(request);

                if (response.StatusCode == HttpStatusCode.OK)
                    resp = JsonConvert.DeserializeObject<ApiFileResponse>(response.Content);
            }
            catch (Exception ex)
            {
                EbLog.Error(ex.Message);
            }
            return resp;
        }

        private void CacheImage(string filename, byte[] fileBytea)
        {
            HelperFunctions.WriteFilesLocal(filename, fileBytea);
        }
    }
}
