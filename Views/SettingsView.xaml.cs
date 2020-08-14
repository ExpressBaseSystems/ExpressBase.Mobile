using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ExpressBase.Mobile.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SettingsView : ContentPage
    {
        public SettingsView()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            string dir = App.Settings.AppDirectory;
            string sid = App.Settings.Sid;

            LocalPath.Text = $"../{dir}/{sid}/log.txt";
        }

        private async void ShareLog_Clicked(object sender, EventArgs e)
        {
            string sid = App.Settings.Sid;
            try
            {
                var message = new EmailMessage()
                {
                    Subject = "Application Log File",
                    To = new List<string> { "support@expressbase.com" }
                };

                INativeHelper helper = DependencyService.Get<INativeHelper>();

                string path = $"{App.Settings.AppDirectory}/{sid}/logs.txt";

                if (helper.DirectoryOrFileExist(path, SysContentType.File))
                {
                    message.Attachments.Add(new EmailAttachment($"{helper.NativeRoot}/{path}"));
                    await Email.ComposeAsync(message);
                }
                else
                {
                    DependencyService.Get<IToast>().Show("logs.txt not found!");
                }
            }
            catch (FeatureNotSupportedException)
            {
                EbLog.Write("Email is not supported on this device");
            }
            catch (Exception ex)
            {
                EbLog.Write("Unknown exception while sharing log.txt");
                EbLog.Write(ex.Message);
            }
        }

        private void ClearLog_Clicked(object sender, EventArgs e)
        {
            string sid = App.Settings.Sid;
            try
            {
                INativeHelper helper = DependencyService.Get<INativeHelper>();
                var path = helper.NativeRoot + $"/{App.Settings.AppDirectory}/{sid}/logs.txt";
                File.WriteAllText(path, string.Empty);

                DependencyService.Get<IToast>().Show("log file cleared :)");
            }
            catch (Exception ex)
            {
                EbLog.Write("Failed to clear logs");
                EbLog.Write(ex.Message);
            }
        }
    }
}