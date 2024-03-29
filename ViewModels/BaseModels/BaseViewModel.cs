﻿using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Services;
using ExpressBase.Mobile.Views;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ExpressBase.Mobile.ViewModels
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        private string pageTitle;

        public string PageTitle
        {
            get => this.pageTitle;
            set
            {
                this.pageTitle = value;
                this.NotifyPropertyChanged();
            }
        }

        private bool isBusy;

        public bool IsBusy
        {
            get => this.isBusy;
            set
            {
                this.isBusy = value;
                this.NotifyPropertyChanged();
            }
        }

        private bool isrefreshing;

        public bool IsRefreshing
        {
            get => isrefreshing;
            set
            {
                isrefreshing = value;
                NotifyPropertyChanged();
            }
        }

        private bool isEmpty;

        public bool IsEmpty
        {
            get => isEmpty;
            set
            {
                isEmpty = value;
                NotifyPropertyChanged();
            }
        }

        public Command ResetConfig => new Command(async () => await Reset());

        public Command LogoutCommand => new Command(async () => await Logout());

        public Command GoToHomeCommand => new Command(async () => await GoToHome());

        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public async Task Reset()
        {
            Store.ResetCashedSolutionData();
            Store.RemoveJSON(AppConst.SOLUTION_OBJ);
            App.Settings.ResetSettings();

            App.RootMaster = null;
            Application.Current.MainPage = new NavigationPage();
            await App.Navigation.NavigateAsync(new MySolutions());
        }

        public async Task Logout()
        {
            //Store.ResetCashedSolutionData();

            App.Settings.SyncInfo.IsLoggedOut = true;
            await Store.SetJSONAsync(AppConst.LAST_SYNC_INFO, App.Settings.SyncInfo);

            await App.Navigation.NavigateToLogin(true);

            if (Utils.HasInternet && App.Settings.Vendor.AllowNotifications)
            {
                await NotificationService.Instance.UnRegisterCurrent();
            }
        }

        public async Task GoToHome()
        {
            await App.Navigation.PopToRootAsync(true);
        }

        public virtual void RefreshPage() { }

        public virtual void Initialize() { }

        public virtual Task InitializeAsync() { return Task.FromResult(false); }

        public virtual Task UpdateAsync() { return Task.FromResult(false); }

        protected async Task<MobileProfileData> GetProfileData(string profilePageRefId)
        {
            MobileProfileData profileData = null;
            try
            {
                profileData = await DataService.Instance.GetProfileDataAsync(profilePageRefId, App.Settings.CurrentLocId);

                if (profileData == null)
                {
                    EbLog.Info($"profile data null for refid : {profilePageRefId}");
                }
            }
            catch (Exception ex)
            {
                EbLog.Error(ex.Message);
            }
            return profileData;
        }
    }
}
