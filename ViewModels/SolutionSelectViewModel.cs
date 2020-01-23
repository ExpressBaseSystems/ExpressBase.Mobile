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
            get
            {
                return this.solutionurtl;
            }
            set
            {
                if (this.solutionurtl == value)
                {
                    return;
                }
                this.solutionurtl = value;
                this.NotifyPropertyChanged();
            }
        }

        private bool isEnabled;

        public bool IsEnabled
        {
            get
            {
                return this.isEnabled;
            }
            set
            {
                if (this.isEnabled == value)
                {
                    return;
                }
                this.isEnabled = value;
                this.NotifyPropertyChanged();
            }
        }

        private bool isSaveEnabled;

        public bool IsSaveEnabled
        {
            get
            {
                return this.isSaveEnabled;
            }
            set
            {
                if (this.isSaveEnabled == value)
                {
                    return;
                }
                this.isSaveEnabled = value;
                this.NotifyPropertyChanged();
            }
        }

        public Command StoreSolutionUrl { get; set; }

        public Command EditButtonCommand { get; set; }

        public SolutionSelectViewModel()
        {
            SolutionUrl = (Settings.RootUrl != null) ? Settings.RootUrl.Replace("https://", string.Empty) : string.Empty;

            if (string.IsNullOrEmpty(SolutionUrl))
                IsEnabled = true;

            StoreSolutionUrl = new Command(async () => await SolutionUrlSet());

            EditButtonCommand = new Command(EditButtonClicked);
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
                Device.BeginInvokeOnMainThread(() => IsBusy = true);
                ValidateSidResponse Response = await RestServices.ValidateSid(url);
                if (Response.IsValid)
                {
                    string _sid = url.Split('.')[0];
                    Store.SetValue(AppConst.SID, _sid);
                    Store.SetValue(AppConst.ROOT_URL, url);
                    this.CreateDB(_sid);
                    this.CreateDir();
                    if (Response.Logo != null)
                    {
                        this.SaveLogo(Response.Logo);
                    }
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        IsBusy = false;
                        Application.Current.MainPage.Navigation.PushAsync(new Login());
                    });
                }
                else
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        IsBusy = false;
                        Application.Current.MainPage.DisplayAlert("Alert!", "Invalid solution URL", "Ok");
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Device.BeginInvokeOnMainThread(() =>
                {
                    IsBusy = false;
                    Application.Current.MainPage.DisplayAlert("Alert!", "Something went wrong", "Ok");
                });
            }
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
