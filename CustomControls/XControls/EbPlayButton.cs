using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ExpressBase.Mobile.CustomControls.XControls
{
    public class EbPlayButton : Button, IDynamicHeight
    {
        public bool CalcHeight { set; get; }

        public List<ApiFileResponse> AudioFiles { set; get; }

        public EbPlayButton() { }

        public EbPlayButton(EbMobileDataColumn dc)
        {
            BackgroundColor = Color.FromHex(dc.BackgroundColor ?? "#ffffff00");
            BorderColor = Color.FromHex(dc.BorderColor ?? "#ffffff00");
            CornerRadius = dc.BorderRadius;
            BorderWidth = dc.BorderThickness;

            if(dc.Font == null)
            {
                FontSize = 14;
                TextColor = Color.Green;
            }
            else
            {
                FontSize = dc.Font.Size <= 0 ? 14 : dc.Font.Size;
                TextColor = Color.FromHex(dc.Font.Color);
            }

            Padding = dc.Padding == null ? 0 : dc.Padding.ConvertToXValue();

            if (dc.Width > 0) this.WidthRequest = dc.Width;
            if (dc.Height > 0) this.HeightRequest = dc.Height;
        }

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
            try
            {
                AudioFiles = new List<ApiFileResponse>();

                string[] refids = value.ToString().Split(CharConstants.COMMA);

                foreach (string id in refids)
                {
                    ApiFileResponse resp = await GetAudioFileAsync($"{id.Trim()}.mp3");

                    if (resp != null && resp.HasContent)
                    {
                        AudioFiles.Add(resp);
                    }
                }
            }
            catch (Exception ex)
            {
                EbLog.Error("[GetFiles] error in list renderer vm, " + ex.Message);
            }
        }

        public async Task<ApiFileResponse> GetAudioFileAsync(string filename)
        {
            ApiFileResponse resp = null;
            try
            {
                RestClient client = new RestClient(App.Settings.RootUrl);
                RestRequest request = new RestRequest("api/get_file", Method.GET);

                request.AddHeader(AppConst.BTOKEN, App.Settings.BToken);
                request.AddHeader(AppConst.RTOKEN, App.Settings.RToken);

                request.AddParameter("category", (int)EbFileCategory.Audio);
                request.AddParameter("filename", filename);

                IRestResponse response = await client.ExecuteAsync(request);

                if (response.IsSuccessful)
                    resp = JsonConvert.DeserializeObject<ApiFileResponse>(response.Content);
            }
            catch (Exception ex)
            {
                EbLog.Error(ex.Message);
            }
            return resp;
        }
    }
}
