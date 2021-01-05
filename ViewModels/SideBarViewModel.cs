using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Services;
using ExpressBase.Mobile.ViewModels.BaseModels;
using ExpressBase.Mobile.Views.Dynamic;
using System;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ExpressBase.Mobile.ViewModels
{
    public class SideBarViewModel : StaticBaseViewModel
    {
        private static SideBarViewModel _instance;

        public static SideBarViewModel Instance => _instance ??= new SideBarViewModel();

        public bool HasAppSwitcher => Utils.Applications.Count > 1;

        public bool HasSolutionSwitcher => App.Settings.Vendor.HasSolutionSwitcher;

        public bool HasLocationSwitcher => App.Settings.Vendor.HasLocationSwitcher && Utils.Locations.Count > 1;

        public bool HasMyActions => App.Settings.Vendor.HasActions;

        public ImageSource DisplayPicture { set; get; }

        private bool _isProfileEditable;

        public bool IsProfileEditable
        {
            get => _isProfileEditable;
            set
            {
                _isProfileEditable = value;
                NotifyPropertyChanged();
            }
        }

        public string UserName => currentUser?.FullName;

        public string Email => currentUser?.Email;

        private readonly User currentUser = App.Settings.CurrentUser;

        private MobilePagesWraper profilePage;

        public Command EditProfileCommand => new Command(async () => await EditProfile());

        private SideBarViewModel()
        {
            SetDisplayPicture();
        }

        public override void Initialize()
        {
            AllowProfileEditIfEnabled();
        }

        private void SetDisplayPicture()
        {
            try
            {
                INativeHelper helper = DependencyService.Get<INativeHelper>();

                byte[] bytes = helper.GetFile($"{App.Settings.AppDirectory}/{App.Settings.Sid}/user.png");

                if (bytes != null)
                {
                    DisplayPicture = ImageSource.FromStream(() => new MemoryStream(bytes));
                }
                else
                {
                    DisplayPicture = ImageSource.FromFile("user_avatar.jpg");
                }
            }
            catch (Exception ex)
            {
                EbLog.Error("[SetDisplayPicture] error in sidebar vm, " + ex.Message);
            }
        }

        private void AllowProfileEditIfEnabled()
        {
            if (currentUser.UserType == 0) return;

            EbProfileUserType userType = App.Settings.CurrentSolution.GetUserTypeById(currentUser.UserType);

            if (userType != null && !string.IsNullOrEmpty(userType.RefId) && App.Settings.ExternalMobilePages != null)
            {
                MobilePagesWraper profilePageWrapper = App.Settings.ExternalMobilePages.Find(item => item.RefId == userType.RefId);

                if (profilePageWrapper != null)
                {
                    IsProfileEditable = true;
                    profilePage = profilePageWrapper;
                }
            }
        }

        private async Task EditProfile()
        {
            if (profilePage == null || !Utils.HasInternet) return;

            EbMobilePage page = profilePage.GetPage();

            if (page != null && page.Container is EbMobileForm)
            {
                App.RootMaster.IsPresented = false;

                try
                {
                    MobileProfileData profileData = await DataService.Instance.GetProfileDataAsync(page.RefId, App.Settings.CurrentLocId);

                    if (profileData != null && profileData.RowId > 0)
                    {
                        EbLog.Info($"profile [rowid] value = '{profileData.RowId}', rendering form in Edit");
                        await App.Navigation.NavigateByRenderer(new FormRender(page, profileData.RowId, profileData.Data));
                    }
                    else
                    {
                        EbLog.Info($"profile data null or [rowid] value = '0', rendering form in new mode");
                        await App.Navigation.NavigateByRenderer(new FormRender(page));
                    }
                }
                catch (Exception ex)
                {
                    EbLog.Error(ex.Message);
                }
            }
        }
    }
}
