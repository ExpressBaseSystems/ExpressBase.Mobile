using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Services;
using ExpressBase.Mobile.Views;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace ExpressBase.Mobile.ViewModels
{
    public class SolutionSelectViewModel : BaseViewModel
    {
        private string solutionurtl;

        public SolutionSelectViewModel()
        {
            this.StoreSolutionUrl = new Command(SolutionUrlSet);
        }

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

        public Command StoreSolutionUrl { get; set; }

        private void SolutionUrlSet(object obj)
        {
            string url = this.SolutionUrl.Trim();
            Task.Run(() =>
            {
                try
                {
                    if (!string.IsNullOrEmpty(url))
                    {
                        Device.BeginInvokeOnMainThread(() => IsBusy = true);
                        if (Api.ValidateSid(url))
                        {
                            string _sid = url.Split('.')[0];
                            Store.SetValue(AppConst.SID, _sid);
                            Store.SetValue(AppConst.ROOT_URL, url);
                            this.CreateDB(_sid);
                            this.CreateDir(_sid);
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
            });
        }

        private void CreateDB(string sid)
        {
            App.DataDB.CreateDB(sid);
        }

        private void CreateDir(string sid)
        {
            INativeHelper helper = DependencyService.Get<INativeHelper>();
            try
            {
                if (helper.DirectoryOrFileExist("ExpressBase", SysContentType.Directory))
                {
                    if (!helper.DirectoryOrFileExist($"ExpressBase/{sid.ToUpper()}", SysContentType.Directory))
                    {
                        string SolDirPath = helper.CreateDirectoryOrFile($"ExpressBase/{sid.ToUpper()}", SysContentType.Directory);
                    }
                }
                else
                {
                    string path = helper.CreateDirectoryOrFile("ExpressBase", SysContentType.Directory);

                    if (!helper.DirectoryOrFileExist($"ExpressBase/{sid.ToUpper()}", SysContentType.Directory))
                    {
                        string SolDirPath = helper.CreateDirectoryOrFile($"ExpressBase/{sid.ToUpper()}", SysContentType.Directory);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
