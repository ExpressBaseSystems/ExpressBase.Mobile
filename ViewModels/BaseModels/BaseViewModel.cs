using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Views;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Xamarin.Forms;

namespace ExpressBase.Mobile.ViewModels
{
    public class BaseViewModel : INotifyPropertyChanged, IDisposable
    {
        public string PageTitle { set; get; }

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

        private bool _show_message;
        public bool ShowMessage
        {
            get { return this._show_message; }
            set
            {
                this._show_message = value;
                this.NotifyPropertyChanged();
            }
        }

        private string _message;
        public string Message
        {
            get { return this._message; }
            set
            {
                this._message = value;
                this.NotifyPropertyChanged();
            }
        }

        private Color _message_color;
        public Color MessageColor
        {
            get { return this._message_color; }
            set
            {
                this._message_color = value;
                this.NotifyPropertyChanged();
            }
        }

        public Command ResetConfig => new Command(ResetClicked);

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
            Store.Remove(AppConst.OBJ_COLLECTION);
            Store.Remove(AppConst.APP_COLLECTION);
            Application.Current.MainPage = new NavigationPage(new SolutionSelect())
            {
                BarBackgroundColor = Color.FromHex("0046bb"),
                BarTextColor = Color.White
            };
        }

        public virtual void RefreshPage() { }

        public void ShowMessageBox(string message, Color background)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                this.MessageColor = background;
                this.Message = message;
                this.ShowMessage = true;
            });
        }

        public void HideMessageBox()
        {
            Device.BeginInvokeOnMainThread(() => this.ShowMessage = false );
        }
    }
}
