using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Views;
using ExpressBase.Mobile.Views.Shared;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ExpressBase.Mobile.ViewModels.Shared
{
    public class SideBarViewModel : BaseViewModel
    {
        private string useremail;
        public string UserEmail
        {
            get { return this.useremail; }
            set
            {
                this.useremail = value;
                this.NotifyPropertyChanged();
            }
        }

        private string username;
        public string UserName
        {
            get { return this.username; }
            set
            {
                this.username = value;
                this.NotifyPropertyChanged();
            }
        }

        public Command LogOutCommand { set; get; }

        public Command ChangeSidCommand { set; get; }

        public Command SwitchAppCommand { set; get; }

        public Command SwitchLocations { set; get; }

        public SideBarViewModel()
        {
            var _user = Settings.UserObject;
            this.UserEmail = _user.Email;
            this.UserName = _user.FullName;

            LogOutCommand = new Command(LogoutClicked);
            ChangeSidCommand = new Command(ChangeSidClicked);
            SwitchAppCommand = new Command(ChangeAppClicked);
            SwitchLocations = new Command(LocationSwitherClick);
        }

        public void LogoutClicked(object sender)
        {
            Store.Remove(AppConst.BTOKEN);
            Store.Remove(AppConst.RTOKEN);
            Store.Remove(AppConst.USER_ID);
            Store.Remove(AppConst.PASSWORD);
            Store.Remove(AppConst.USER_OBJECT);
            Store.Remove(AppConst.USER_LOCATIONS);
            Store.Remove(AppConst.CURRENT_LOCATION);

            Store.Remove(AppConst.APPID);
            Store.Remove(AppConst.APPNAME);
            Store.Remove(AppConst.OBJ_COLLECTION);
            Store.Remove(AppConst.APP_COLLECTION);
            Application.Current.MainPage = new NavigationPage(new Login())
            {
                BarBackgroundColor = Color.FromHex("0046bb"),
                BarTextColor = Color.White
            };
        }

        public void ChangeSidClicked(object sender)
        {
            //Store.Remove(AppConst.SID);
            //Store.Remove(AppConst.ROOT_URL);
            //Store.Remove(AppConst.APPID);
            //Store.Remove(AppConst.USERNAME);
            //Store.Remove(AppConst.PASSWORD);
            //Store.Remove(AppConst.BTOKEN);
            //Store.Remove(AppConst.RTOKEN);
            //Store.Remove(AppConst.OBJ_COLLECTION);
            //Store.Remove(AppConst.APP_COLLECTION);
            Application.Current.MainPage = new NavigationPage(new SolutionSelect())
            {
                BarBackgroundColor = Color.FromHex("0046bb"),
                BarTextColor = Color.White
            };
            App.RootMaster.IsPresented = false;
        }

        public void ChangeAppClicked(object sender)
        {
        //    Store.Remove(AppConst.APPID);
        //    Store.Remove(AppConst.OBJ_COLLECTION);

            App.RootMaster.IsPresented = false;
            App.RootMaster.Detail.Navigation.PushAsync(new AppSelect());
        }

        public void LocationSwitherClick(object sender)
        {
            App.RootMaster.IsPresented = false;
            App.RootMaster.Detail.Navigation.PushAsync(new LocationsView());
        }
    }
}
