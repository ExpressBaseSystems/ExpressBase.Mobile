using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.ViewModels.BaseModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ExpressBase.Mobile.ViewModels
{
    public class SideBarViewModel : StaticBaseViewModel
    {
        public bool HasAppSwitcher => Utils.Applications.Count > 1;

        public bool HasSolutionSwitcher => App.Settings.Vendor.HasSolutionSwitcher;

        public bool HasLocationSwitcher => App.Settings.Vendor.HasLocationSwitcher && Utils.Locations.Count > 1;

        public bool HasMyActions => App.Settings.Vendor.HasActions;

        public ImageSource DisplayPicture { set; get; }

        public bool IsProfileEditable { set; get; }

        public string UserName => currentUser?.FullName;

        public string Email => currentUser?.Email;

        private readonly User currentUser = App.Settings.CurrentUser;

        private MobilePagesWraper profilePage;

        public Command EditProfileCommand { set; get; }

        public SideBarViewModel()
        {
            Initialize();
        }

        public override void Initialize()
        {
            SetDisplayPicture();
            SetProfileEdit();
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

        private void SetProfileEdit()
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

                    EditProfileCommand = new Command(async () => await EditProfile());
                }
            }
        }

        private async Task EditProfile()
        {
            if (profilePage != null)
            {
                EbMobilePage page = profilePage.GetPage();

                if (page != null && page.Container is EbMobileForm)
                {
                    App.RootMaster.IsPresented = false;
                    ContentPage renderer = EbPageHelper.GetPageByContainer(page);
                    await App.Navigation.NavigateAsync(renderer);
                }
            }
        }
    }
}
