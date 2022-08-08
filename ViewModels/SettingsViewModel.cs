using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Services;
using ExpressBase.Mobile.ViewModels.BaseModels;
using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ExpressBase.Mobile.ViewModels
{
    public class SettingsViewModel : StaticBaseViewModel
    {
        public string LogFilePath { set; get; }

        public SettingsViewModel()
        {
            Initialize();
        }

        public override void Initialize()
        {
            //LogFilePath = $"../{App.Settings.AppDirectory}/{App.Settings.Sid.ToUpper()}/log.txt";
        }
    }
}
