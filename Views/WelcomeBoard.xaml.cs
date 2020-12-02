using ExpressBase.Mobile.Configuration;
using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Views.Base;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ExpressBase.Mobile.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class WelcomeBoard : ContentPage, IDynamicContent
    {
        readonly AppVendor Vendor = App.Settings.Vendor;

        public Dictionary<string, string> PageContent { set; get; }

        public WelcomeBoard()
        {
            InitializeComponent();
        }

        private async void StartApplicationClicked(object sender, EventArgs e)
        {
            if (!Utils.HasInternet)
            {
                Utils.Alert_NoInternet();
                return;
            }

            if (Vendor.BuildType == Enums.AppBuildType.Embedded)
            {
                EbLayout.ShowLoader();
                try
                {
                    ValidateSidResponse response = await ValidateSid(Vendor.SolutionURL);

                    if (response != null && response.IsValid)
                    {
                        await CreateEmbeddedSolution(response);
                        await Store.SetValueAsync(AppConst.FIRST_RUN, "true");
                        await App.Navigation.InitializeNavigation();
                    }
                }
                catch (Exception)
                {
                    Utils.Toast("Unable to finish the request");
                }
                EbLayout.HideLoader();
            }
            else
            {
                await App.Navigation.InitializeNavigation();
            }
        }

        public async Task<ValidateSidResponse> ValidateSid(string url)
        {
            RestClient client = new RestClient(ApiConstants.PROTOCOL + url);
            RestRequest request = new RestRequest(ApiConstants.VALIDATE_SOL, Method.GET);

            try
            {
                IRestResponse iresp = await client.ExecuteAsync(request);
                if (iresp.IsSuccessful)
                {
                    return JsonConvert.DeserializeObject<ValidateSidResponse>(iresp.Content);
                }
            }
            catch (Exception e)
            {
                EbLog.Info("validate_solution api failure");
                EbLog.Error(e.Message);
            }
            return null;
        }

        private async Task CreateEmbeddedSolution(ValidateSidResponse result)
        {
            SolutionInfo sln = new SolutionInfo
            {
                SolutionName = Vendor.SolutionURL.Split(CharConstants.DOT)[0],
                RootUrl = Vendor.SolutionURL,
                IsCurrent = true,
                SolutionObject = result.SolutionObj,
                SignUpPage = result.SignUpPage
            };
            App.Settings.CurrentSolution = sln;

            try
            {
                await Store.SetJSONAsync(AppConst.MYSOLUTIONS, new List<SolutionInfo> { sln });
                await Store.SetJSONAsync(AppConst.SOLUTION_OBJ, sln);

                App.DataDB.CreateDB(sln.SolutionName);
                await HelperFunctions.CreateDirectory();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void OnDynamicContentRendering()
        {
            
        }
    }
}