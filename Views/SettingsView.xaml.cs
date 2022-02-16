using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.ViewModels;
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
        private readonly string logPath = $"{App.Settings.AppDirectory}/{App.Settings.Sid.ToUpper()}/logs.txt";

        private readonly INativeHelper nativeHelper = DependencyService.Get<INativeHelper>();

        public SettingsView()
        {
            InitializeComponent();
            BindingContext = new SettingsViewModel();

            LastSyncInfo syncInfo = App.Settings.SyncInfo;
            if (syncInfo == null || syncInfo.LastSyncTs == DateTime.MinValue)
                LastSyncAtLbl.Text = "Sync required";
            else
                LastSyncAtLbl.Text = "Last sync at: " + syncInfo.LastSyncTs.ToString();
        }

        private async void ShareLog_Clicked(object sender, EventArgs e)
        {
            try
            {
                EmailMessage message = new EmailMessage()
                {
                    Subject = "Application Log File",
                    To = new List<string> { "support@expressbase.com" }
                };

                if (nativeHelper.Exist(logPath, SysContentType.File))
                {
                    message.Attachments.Add(new EmailAttachment($"{nativeHelper.NativeRoot}/{logPath}"));
                    await Email.ComposeAsync(message);
                }
                else
                    Utils.Toast("logs.txt not found!");
            }
            catch (FeatureNotSupportedException)
            {
                EbLog.Error("Email is not supported on this device");
            }
            catch (Exception ex)
            {
                EbLog.Error("Unknown exception while sharing log.txt, " + ex.Message);
            }
        }

        private void ClearLog_Clicked(object sender, EventArgs e)
        {
            try
            {
                File.WriteAllText($"{nativeHelper.NativeRoot}/{logPath}", string.Empty);
                Utils.Toast("Log file cleared :)");
            }
            catch (Exception ex)
            {
                EbLog.Error("Failed to clear logs, " + ex.Message);
            }
        }

        private async void OpenLog_Clicked(object sender, EventArgs e)
        {
            try
            {
                await Launcher.OpenAsync(new OpenFileRequest
                {
                    File = new ReadOnlyFile($"{nativeHelper.NativeRoot}/{logPath}")
                });
            }
            catch (Exception ex)
            {
                EbLog.Error("Failed to open logs file, " + ex.Message);
            }
        }

    }
}