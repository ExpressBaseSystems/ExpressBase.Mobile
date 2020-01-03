using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Xamarin.Forms;

namespace ExpressBase.Mobile.ViewModels
{
    public class BaseViewModel : INotifyPropertyChanged, IDisposable
    {
        public string PageTitle { set; get; }

        protected bool _isBusy;
        public bool IsBusy
        {
            get
            {
                return this._isBusy;
            }
            set
            {
                if (this._isBusy == value)
                {
                    return;
                }
                this._isBusy = value;
                this.NotifyPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public Command ResetConfig { set; get; }

        public void ResetClicked(object sender)
        {
            Store.Remove(AppConst.SID);
            Store.Remove(AppConst.ROOT_URL);
            Store.Remove(AppConst.APPID);
            Store.Remove(AppConst.USERNAME);
            Store.Remove(AppConst.PASSWORD);
            Store.Remove(AppConst.BTOKEN);
            Store.Remove(AppConst.RTOKEN);
            Store.Remove(AppConst.OBJ_COLLECTION);
            Store.Remove(AppConst.APP_COLLECTION);
            Application.Current.MainPage = new NavigationPage(new SolutionSelect());
        }

        public virtual void RefreshPage()
        {

        }
    }
}
