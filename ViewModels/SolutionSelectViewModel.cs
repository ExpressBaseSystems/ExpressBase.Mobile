using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Services;
using ExpressBase.Mobile.Views;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;

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

            if (!string.IsNullOrEmpty(url))
            {
                if (Api.ValidateSid(url))
                {
                    string _sid = url.Split('.')[0];
                    Store.SetValue(AppConst.SID, _sid);
                    Store.SetValue(AppConst.ROOT_URL, url);
                    Application.Current.MainPage.Navigation.PushAsync(new Login());
                    this.CreateDB(_sid);
                }
                else
                {
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        await Application.Current.MainPage.DisplayAlert("Alert!", "Invalid solution URL", "Ok");
                    });
                }
            }
        }

        private void CreateDB(string sid)
        {
            App.DataDB.CreateDB(sid);
        }
    }
}
