using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.CustomControls;
using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Services;
using ExpressBase.Mobile.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

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

        private bool isSaveEnabled;
        public bool IsSaveEnabled
        {
            get { return this.isSaveEnabled; }
            set
            {
                this.isSaveEnabled = value;
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

        public Command EditButtonCommand { get; set; }

        public Command ConfirmButtonCommand { get; set; }

        public Command CancelCommand { get; set; }

        private ValidateSidResponse ValidateSIDResponse { set; get; }

        public SolutionSelectViewModel()
        {
            SolutionUrl = (Settings.RootUrl != null) ? Settings.RootUrl.Replace("https://", string.Empty) : string.Empty;

            if (string.IsNullOrEmpty(SolutionUrl))
                IsEnabled = true;

            StoreSolutionUrl = new Command(async () => await SolutionUrlSet());

            EditButtonCommand = new Command(EditButtonClicked);

            ConfirmButtonCommand = new Command(async () => await ConfirmClicked());

            CancelCommand = new Command(CancelClicked);
        }

        private async Task ConfirmClicked()
        {
            try
            {
                string _sid = this.SolutionUrl.Split('.')[0];
                Store.SetValue(AppConst.SID, _sid);
                Store.SetValue(AppConst.ROOT_URL, this.SolutionUrl);
                this.CreateDB(_sid);
                this.CreateDir();

                if (ValidateSIDResponse.Logo != null)
                {
                    this.SaveLogo(ValidateSIDResponse.Logo);
                }
                await Application.Current.MainPage.Navigation.PushAsync(new Login());

                ShowLoader = true;
                ShowMessage = false;
                IsBusy = false;
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
        }

        private void CancelClicked()
        {
            try
            {
                ShowMessage = false;
                IsBusy = false;
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
        }

        private void EditButtonClicked(object obj)
        {
            IsEnabled = true;
        }

        private async Task SolutionUrlSet()
        {
            string url = this.SolutionUrl.Trim();

            if (Connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                DependencyService.Get<IToast>().Show("Not connected to internet!");
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
                    await Application.Current.MainPage.DisplayAlert("Alert!", "Invalid solution URL", "Ok");
                }
            }
            catch (Exception ex)
            {
                IsBusy = false;
                await Application.Current.MainPage.DisplayAlert("Alert!", "Something went wrong", "Ok");
            }
        }

        private void SetMessage(string Msg, Color color)
        {
            Message = Msg;
            NotifyPropertyChanged("Message");

            MessageColor = color;
            NotifyPropertyChanged("MessageColor");
        }

        private void CreateDB(string sid)
        {
            App.DataDB.CreateDB(sid);
        }

        private void CreateDir()
        {
            try
            {
                string path = HelperFunctions.CreatePlatFormDir();
                if (!String.IsNullOrEmpty(path))
                {
                    //folder created
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void SaveLogo(byte[] imageByte)
        {
            INativeHelper helper = DependencyService.Get<INativeHelper>();
            string sid = Settings.SolutionId;
            try
            {
                if (!helper.DirectoryOrFileExist($"ExpressBase/{sid}/logo.png", SysContentType.File))
                {
                    File.WriteAllBytes(helper.NativeRoot + $"/ExpressBase/{sid}/logo.png", imageByte);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
