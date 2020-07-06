using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using Xamarin.Forms;

namespace ExpressBase.Mobile.Models
{
    public class ApiAuthResponse
    {
        public string BToken { set; get; }

        public string RToken { set; get; }

        public bool IsValid { set; get; }

        public int UserId { set; get; }

        public string DisplayName { set; get; }

        public User User { set; get; }

        public byte[] DisplayPicture { set; get; }

        public bool Is2FEnabled { get; set; }

        public string TwoFAToken { set; get; }

        public bool TwoFAStatus { set; get; }

        public string TwoFAToAddress { set; get; }
    }

    public class ApiTwoFactorResponse : IEbApiStatusCode
    {
        public bool IsValid { set; get; }

        public bool IsVerification { set; get; }

        public HttpStatusCode StatusCode { get; set; }
    }

    public enum EbSystemRoles
    {
        EbAdmin,
        EbReadOnlyUser,
        EbUser
    }

    public enum SystemRoles
    {
        SolutionOwner = 1,
        SolutionAdmin = 2,
        SolutionDeveloper = 3,
        SolutionTester = 4,
        SolutionPM = 5,
        SolutionUser = 6
    }

    public class User
    {
        public int UserId { get; set; }

        public string CId { get; set; }

        public List<string> Roles { get; set; }

        public List<string> Permissions { get; set; }

        public string AuthId { get; set; }

        public Preferences Preference { get; set; }

        public string Email { get; set; }

        public string FullName { get; set; }

        public int SignInLogId { get; set; }

        public List<string> EbObjectIds { set; get; }

        public List<int> LocationIds { get; }

        public bool IsAdmin
        {
            get
            {
                return Roles.Contains("SolutionOwner") || Roles.Contains("SolutionAdmin");
            }
        }

        public bool HasEbSystemRole()
        {
            foreach (var role in Enum.GetValues(typeof(EbSystemRoles)))
            {
                if (this.Roles.Contains(role.ToString()))
                    return true;
            }
            return false;
        }

        public bool HasSystemRole()
        {
            foreach (var role in Enum.GetValues(typeof(SystemRoles)))
            {
                if (this.Roles.Contains(role.ToString()))
                    return true;
            }
            return false;
        }

        public bool HasRole(string role)
        {
            return this.Roles.Contains(role);
        }

        public bool HasPermission(string permission)
        {
            return this.Permissions.Contains(permission);
        }

        public List<MobilePagesWraper> FilterByLocation()
        {
            var pages = App.Settings.MobilePages;

            if (IsAdmin)
            {
                return new List<MobilePagesWraper>(pages);
            }

            List<int> objids = new List<int>();

            foreach (string perm in Permissions)
            {
                int id = Convert.ToInt32(perm.Split(CharConstants.DASH)[2]);
                int locid = Convert.ToInt32(perm.Split(CharConstants.COLON)[1]);

                if (locid == App.Settings.CurrentLocId || locid == -1)
                    objids.Add(id);
            }

            List<MobilePagesWraper> filtered = pages.Where(item => objids.Contains(item.RefId.ToObjId())).ToList();

            return filtered ?? new List<MobilePagesWraper>();
        }
    }

    public class Preferences
    {
        public string Locale { get; set; }

        public string TimeZone { get; set; }

        public int DefaultLocation { get; set; }

        public string DefaultDashBoard { get; set; }

        public string ShortDatePattern { get; set; }

        public string ShortDate { set; get; }

        public string ShortTimePattern { set; get; }

        public string ShortTime { set; get; }
    }

    public class EbLocation : INotifyPropertyChanged
    {
        public int LocId { get; set; }

        public string LongName { get; set; }

        public string ShortName { get; set; }

        public Dictionary<string, string> Meta { get; set; }

        public string Logo { get; set; }

        public bool Selected { set; get; }

        public Color SelectionColor
        {
            get
            {
                return Selected ? App.Settings.Vendor.GetPrimaryColor() : Color.White;
            }
        }

        public Color Bordercolor
        {
            get
            {
                return Selected ? App.Settings.Vendor.GetPrimaryColor() : Color.FromHex("cccccc");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Select()
        {
            Selected = true;
            RaisePropertyChanged("SelectionColor");
            RaisePropertyChanged("Bordercolor");
        }

        public void UnSelect()
        {
            Selected = false;
            RaisePropertyChanged("SelectionColor");
            RaisePropertyChanged("Bordercolor");
        }
    }
}


