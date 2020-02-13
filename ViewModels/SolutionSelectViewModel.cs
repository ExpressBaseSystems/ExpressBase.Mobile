using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.CustomControls;
using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Services;
using ExpressBase.Mobile.Views;
using System;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace ExpressBase.Mobile.ViewModels
{
    public class SolutionSelectViewModel : BaseViewModel
    {
        private string solutionurtl;
        public string SolutionUrl
        {
            get { return this.solutionurtl; }
            set
            {
                this.solutionurtl = value;
                this.NotifyPropertyChanged();
            }
        }

        private bool isEnabled;
        public bool IsEnabled
        {
            get { return this.isEnabled; }
            set
            {
                this.isEnabled = value;
                this.NotifyPropertyChanged();
            }
        }

        private bool showLoader;
        public bool ShowLoader
        {
            get { return this.showLoader; }
            set
            {
                this.showLoader = value;
                this.NotifyPropertyChanged();
            }
        }

        private bool showMessage;
        public bool ShowMessage
        {
            get { return this.showMessage; }
            set
            {
                this.showMessage = value;
                this.NotifyPropertyChanged();
            }
        }

        public string Message { set; get; }

        public Color MessageColor { set; get; }

        public ImageSource SolutionLogo { set; get; }

        public Command StoreSolutionUrl { get; set; }

        public Command ConfirmButtonCommand { get; set; }

        private ValidateSidResponse ValidateSIDResponse { set; get; }

        public SolutionSelectViewModel()
        {
            SolutionUrl = (Settings.RootUrl != null) ? Settings.RootUrl.Replace("https://", string.Empty) : string.Empty;

            if (string.IsNullOrEmpty(SolutionUrl))
                IsEnabled = true;

            StoreSolutionUrl = new Command(async () =>
            {
                if (string.IsNullOrEmpty(SolutionUrl))
                    return;

                string url = Settings.RootUrl ?? string.Empty;

                if (SolutionUrl != url.Replace("https://", string.Empty))
                    await SolutionUrlSet();
            });

            ConfirmButtonCommand = new Command(async () => await ConfirmClicked());
        }

        private async Task ConfirmClicked()
        {
            try
            {
                string _sid = this.SolutionUrl.Split('.')[0];
                await Store.SetValueAsync(AppConst.SID, _sid);
                await Store.SetValueAsync(AppConst.ROOT_URL, this.SolutionUrl);

                Store.Remove(AppConst.OBJ_COLLECTION);//remove obj collection
                Store.Remove(AppConst.APP_COLLECTION);
                Store.Remove(AppConst.APPID);

                App.DataDB.CreateDB(_sid);
                HelperFunctions.CreatePlatFormDir();

                if (ValidateSIDResponse.Logo != null)
                    this.SaveLogo(ValidateSIDResponse.Logo);

                await Application.Current.MainPage.Navigation.PushAsync(new Login());

                ShowLoader = true;
                ShowMessage = false;
                IsBusy = false;
            }
            catch (Exception ex) { Log.Write("SolutionSelect_ConfirmClicked" + ex.Message); }
        }

        private async Task SolutionUrlSet()
        {
            string url = this.SolutionUrl.Trim();
            IToast toast = DependencyService.Get<IToast>();

            if (!Settings.HasInternet)
            {
                toast.Show("Not connected to internet!");
                return;
            }

            try
            {
                IsBusy = true;
                ShowLoader = true;

                ValidateSIDResponse = await RestServices.ValidateSid(url);
                if (ValidateSIDResponse.IsValid)
                {
                    ShowLoader = false;
                    SetMessage("Success :)", Color.Green);
                    SolutionLogo = ImageSource.FromStream(() => new MemoryStream(ValidateSIDResponse.Logo));
                    NotifyPropertyChanged("SolutionLogo");
                    ShowMessage = true;
                }
                else
                {
                    IsBusy = false;
                    toast.Show("Invalid solution URL");
                }
            }
            catch (Exception ex)
            {
                Log.Write("SolutionSelect_SolutionUrlSet" + ex.Message);
                IsBusy = false;
                toast.Show("Something went wrong");
            }
        }

        private void SetMessage(string Msg, Color color)
        {
            Message = Msg;
            NotifyPropertyChanged("Message");

            MessageColor = color;
            NotifyPropertyChanged("MessageColor");
        }

        private void SaveLogo(byte[] imageByte)
        {
            INativeHelper helper = DependencyService.Get<INativeHelper>();
            string sid = Settings.SolutionId;
            try
            {
                if (!helper.DirectoryOrFileExist($"ExpressBase/{sid}/logo.png", SysContentType.File))
                    File.WriteAllBytes(helper.NativeRoot + $"/ExpressBase/{sid}/logo.png", imageByte);
            }
            catch (Exception ex)
            {
                Log.Write("SolutionSelect_SaveLogo" + ex.Message);
            }
        }
    }
}
