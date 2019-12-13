using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Services;
using ExpressBase.Mobile.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xamarin.Forms;

namespace ExpressBase.Mobile.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {

        private string email;
        public string Email
        {
            get
            {
                return this.email;
            }
            set
            {
                if (this.email == value)
                {
                    return;
                }
                this.email = value;
                this.NotifyPropertyChanged();
            }
        }

        private string password;
        public string PassWord
        {
            get
            {
                return this.password;
            }
            set
            {
                if (this.password == value)
                {
                    return;
                }
                this.password = value;
                this.NotifyPropertyChanged();
            }
        }

        private ImageSource logourl;
        public ImageSource LogoUrl
        {
            get
            {
                return logourl;
            }
            set
            {
                logourl = value;
                this.NotifyPropertyChanged();
            }
        }


        public LoginViewModel()
        {
            this.LoginCommand = new Command(LoginAction);
            SetLogo();
        }

        public Command LoginCommand { set; get; }

        private void LoginAction(object obj)
        {
            string _username = this.Email.Trim();
            string _password = this.PassWord.Trim();

            if (CanLogin())
            {
                ApiAuthResponse response = Auth.TryAuthenticate(_username, _password);
                if (response.IsValid)
                {
                    Auth.UpdateStore(response, _username, password);
                    Application.Current.MainPage.Navigation.PushAsync(new AppSelect());
                }
            }
        }

        private bool CanLogin()
        {
            if (string.IsNullOrWhiteSpace(this.Email) || string.IsNullOrWhiteSpace(this.PassWord))
                return false;

            return true;
        }

        private void SetLogo()
        {
            INativeHelper helper = DependencyService.Get<INativeHelper>();
            string sid = Settings.SolutionId;
            try
            {
                if (!helper.DirectoryOrFileExist($"ExpressBase/{sid}/logo.png", SysContentType.File))
                {
                    byte[] imageByte = Api.GetSolutionLogo($"images/logo/{sid}.png");
                    if (imageByte != null)
                    {
                        File.WriteAllBytes(helper.NativeRoot + $"/ExpressBase/{sid}/logo.png", imageByte);
                        LogoUrl = ImageSource.FromStream(() => new MemoryStream(imageByte));
                    }
                    else
                    {
                        LogoUrl = ImageSource.FromResource("eblogo.png");
                    }
                }
                else
                {
                    var bytes = helper.GetPhoto($"ExpressBase/{sid}/logo.png");
                    LogoUrl = (bytes == null) ? ImageSource.FromResource("eblogo.png"): ImageSource.FromStream(() => new MemoryStream(bytes));                    
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
