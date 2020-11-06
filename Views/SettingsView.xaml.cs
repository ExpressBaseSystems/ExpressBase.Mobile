using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
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

                if (helper.Exist(path, SysContentType.File))
                {
                    message.Attachments.Add(new EmailAttachment($"{helper.NativeRoot}/{path}"));
                    await Email.ComposeAsync(message);
                }
                else
                {
                    Utils.Toast("logs.txt not found!");
                }
            }
            catch (FeatureNotSupportedException)
            {
                EbLog.Error("Email is not supported on this device");
            }
            catch (Exception ex)
            {
                EbLog.Error("Unknown exception while sharing log.txt");
                EbLog.Error(ex.Message);
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

                Utils.Toast("log file cleared :)");
            }
            catch (Exception ex)
            {
                EbLog.Error("Failed to clear logs");
                EbLog.Error(ex.Message);
            }
        }

        private async void OpenLog_Clicked(object sender, EventArgs e)
        {
            try
            {
                INativeHelper helper = DependencyService.Get<INativeHelper>();
                var path = helper.NativeRoot + $"/{App.Settings.AppDirectory}/{App.Settings.Sid}/logs.txt";

                await Launcher.OpenAsync(new OpenFileRequest
                {
                    File = new ReadOnlyFile(path)
                });
            }
            catch(Exception ex)
            {
                EbLog.Error("Failed to open logs file");
                EbLog.Error(ex.Message);
            }
        }
    }
}