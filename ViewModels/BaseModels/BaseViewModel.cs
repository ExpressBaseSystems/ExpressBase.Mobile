using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Views;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ExpressBase.Mobile.ViewModels
{
    public class BaseViewModel : INotifyPropertyChanged, IDisposable
    {
        private string _pageTitle;
        public string PageTitle
        {
            get { return this._pageTitle; }
            set
            {
                this._pageTitle = value;
                this.NotifyPropertyChanged();
            }
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get { return this._isBusy; }
            set
            {
                this._isBusy = value;
                this.NotifyPropertyChanged();
            }
        }

        public Command ResetConfig => new Command(ResetClicked);

        public Command LogoutCommand => new Command(LogoutClicked);

        public event PropertyChangedEventHandler PropertyChanged;

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void ResetClicked()
        {
            Store.Remove(AppConst.SID);
            Store.Remove(AppConst.ROOT_URL);
            Store.Remove(AppConst.APPID);
            Store.Remove(AppConst.USERNAME);
            Store.Remove(AppConst.PASSWORD);
            Store.Remove(AppConst.BTOKEN);
            Store.Remove(AppConst.RTOKEN);
            Store.RemoveJSON(AppConst.OBJ_COLLECTION);
            Store.RemoveJSON(AppConst.APP_COLLECTION);
            App.RootMaster = null;
            Application.Current.MainPage = new NavigationPage(new MySolutions())
            {
                BarBackgroundColor = Color.FromHex("0046bb"),
                BarTextColor = Color.White
            };
        }

        public void LogoutClicked()
        {
            try
            {
                Store.Remove(AppConst.BTOKEN);
                Store.Remove(AppConst.RTOKEN);
                Store.Remove(AppConst.USER_ID);
                Store.Remove(AppConst.PASSWORD);
                Store.RemoveJSON(AppConst.USER_OBJECT);
                Store.RemoveJSON(AppConst.USER_LOCATIONS);
                Store.Remove(AppConst.CURRENT_LOCATION);

                Store.Remove(AppConst.APPID);
                Store.Remove(AppConst.APPNAME);
                Store.RemoveJSON(AppConst.OBJ_COLLECTION);
                Store.RemoveJSON(AppConst.APP_COLLECTION);
                Application.Current.MainPage = new NavigationPage(new Login())
                {
                    BarBackgroundColor = Color.FromHex("0046bb"),
                    BarTextColor = Color.White
                };
            }
            catch(Exception ex)
            {
                Log.Write(ex.Message);
            }
        }

        public virtual void RefreshPage() { }

        public virtual Task InitializeAsync()
        {
            return Task.FromResult(false);
        }
    }
}
