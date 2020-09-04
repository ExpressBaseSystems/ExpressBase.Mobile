using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Services;
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

        private bool isrefreshing;

        public bool IsRefreshing
        {
            get { return isrefreshing; }
            set
            {
                isrefreshing = value;
                NotifyPropertyChanged();
            }
        }

        public Command ResetConfig => new Command(async () => await Reset());

        public Command LogoutCommand => new Command(async () => await Logout());

        public event PropertyChangedEventHandler PropertyChanged;

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public async Task Reset()
        {
            Store.ResetCashedSolutionData();
            Store.RemoveJSON(AppConst.SOLUTION_OBJ);
            App.Settings.Reset();

            App.RootMaster = null;
            Application.Current.MainPage = new NavigationPage()
            {
                BarBackgroundColor = App.Settings.Vendor.GetPrimaryColor(),
                BarTextColor = Color.White
            };
            await Application.Current.MainPage.Navigation.PushAsync(new MySolutions());
        }

        public async Task Logout()
        {
            Store.ResetCashedSolutionData();
            await NavigationService.LoginWithNS();
        }

        public virtual void RefreshPage() { }

        public virtual Task InitializeAsync()
        {
            return Task.FromResult(false);
        }

        public virtual Task UpdateAsync()
        {
            return Task.FromResult(false);
        }
    }
}
