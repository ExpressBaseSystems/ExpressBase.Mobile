using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Services;
using System;
using System.Collections.Generic;
using System.Timers;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ExpressBase.Mobile.CustomControls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AudioRecorder : Frame
    {
        public bool MultiSelect { set; get; }

        private const string AUDIO_TYPE = ".mp3";

        const string ACTION_START = "start";

        const string ACTION_STOP = "stop";

        readonly IEbAudioHelper audioHelper;

        Timer recordingTimer;

        int elapsedMinutes = 0;

        int elapsedSeconds = 0;

        readonly double maxDuration = 120000;//2 minutes

        private readonly Dictionary<string, byte[]> audioFiles = new Dictionary<string, byte[]>();

        private readonly Dictionary<string, View> audioTemplates = new Dictionary<string, View>();

        private readonly List<string> uploadedFiles = new List<string>();

        public AudioRecorder(EbMobileAudioInput audio)
        {
            InitializeComponent();

            MultiSelect = audio.MultiSelect;
            audioHelper = DependencyService.Get<IEbAudioHelper>();
            double max = audio.MaximumDuration <= maxDuration && audio.MaximumDuration > 0 ? audio.MaximumDuration : maxDuration;
            audioHelper.MaximumDuration = max * 60000;
            audioHelper.OnRecordingCompleted += OnRecordingCompleted;
        }

        private void OnRecordingCompleted(object sender, EventArgs e)
        {
            byte[] note = (byte[])sender;
            if (note != null)
            {
                if (!MultiSelect)
                {
                    audioFiles.Clear();
                    RecordList.Children.Clear();
                }
                AddAudioFrame(note, true);
            }
            RecordingCompleted();
        }

        private async void RecordButton_Clicked(object sender, EventArgs e)
        {
            XAudioButton btn = (XAudioButton)sender;

            if (btn.ActionType == ACTION_START)
            {
                if (await AppPermission.Audio())
                {
                    RecordingTimerLabel.IsVisible = true;
                    await audioHelper.StartRecording();
                    StartRecordingTimer();
                    SetStopRecordButtonSyle();
                }
                else
                    Utils.Toast("Microphone permission revoked");
            }
            else
            {
                audioHelper.StopRecording();
                SetRecordButtonSyle();
            }
        }

        private void SetRecordButtonSyle()
        {
            RecordButton.ActionType = ACTION_START;
            RecordButton.Text = "\uf130";
            RecordButton.TextColor = Color.FromHex("#3c903c");
            RecordButton.BorderColor = Color.FromHex("#3c903c");
            RecordingPlaceHolder.IsVisible = false;
        }

        private void SetStopRecordButtonSyle()
        {
            RecordButton.ActionType = ACTION_STOP;
            RecordButton.Text = "\uf04d";
            RecordButton.TextColor = Color.FromHex("#d85a5a");
            RecordButton.BorderColor = Color.FromHex("#d85a5a");
            RecordingPlaceHolder.IsVisible = true;
        }

        private void RecordingCompleted()
        {
            RecordingTimerLabel.Text = "0:00";
            recordingTimer?.Stop();
            elapsedMinutes = elapsedSeconds = 0;
        }

        private void AddAudioFrame(byte[] note, bool deleteButton, string name = null)
        {
            EbAudioTemplate template = new EbAudioTemplate(note, name) { AllowDelete = deleteButton };
            template.OnDelete += TemplateOnDelete;

            View view = template.CreateView();
            audioTemplates.Add(template.Name, view);
            audioFiles.Add(template.Name, note);

            RecordList.Children.Add(view);
        }

        private void TemplateOnDelete(object sender, EventArgs e)
        {
            string templateName = sender.ToString();

            if (audioTemplates.TryGetValue(templateName, out var view))
            {
                audioTemplates.Remove(templateName);
                RecordList.Children.Remove(view);

                if (audioFiles.ContainsKey(templateName))
                {
                    audioFiles.Remove(templateName);
                }
            }
        }

        private void StartRecordingTimer()
        {
            recordingTimer = new Timer(1000);
            recordingTimer.Elapsed += (sender, e) =>
            {
                if (elapsedSeconds == 60)
                {
                    elapsedMinutes += 1;
                    elapsedSeconds = 0;
                }
                else
                    elapsedSeconds += 1;

                string seconsNotaion = elapsedSeconds < 10 ? $"0{elapsedSeconds}" : $"{elapsedSeconds}";
                Device.BeginInvokeOnMainThread(() => RecordingTimerLabel.Text = $"{elapsedMinutes}:{seconsNotaion}");
            };
            recordingTimer.Start();
        }

        public List<FileWrapper> GetFiles(string ctrlName)
        {
            List<FileWrapper> files = new List<FileWrapper>();

            foreach (var pair in audioFiles)
            {
                if (uploadedFiles.Contains(pair.Key)) continue;

                files.Add(new FileWrapper
                {
                    Name = pair.Key,
                    FileName = pair.Key + AUDIO_TYPE,
                    Bytea = pair.Value,
                    ControlName = ctrlName
                });
            }
            return files;
        }

        public async void SetValue(NetworkMode network, FUPSetValueMeta meta, string ctrlname)
        {
            if (network == NetworkMode.Offline)
            {
                string pattern = $"{meta.TableName}-{meta.RowId}-{ctrlname}*";

                List<FileWrapper> Files = HelperFunctions.GetFilesByPattern(pattern);

                foreach (FileWrapper file in Files)
                {
                    string name = Guid.NewGuid().ToString("N");
                    uploadedFiles.Add(name);
                    this.AddAudioFrame(file.Bytea, false, name);
                }
            }
            else if (network == NetworkMode.Online)
            {
                foreach (FileMetaInfo info in meta.Files)
                {
                    try
                    {
                        ApiFileResponse resp = await FormDataServices.Instance.GetFile(info.FileCategory, $"{info.FileRefId + AUDIO_TYPE}");

                        if (resp != null && resp.HasContent)
                        {
                            string name = Guid.NewGuid().ToString("N");
                            uploadedFiles.Add(name);
                            this.AddAudioFrame(resp.Bytea, false, name);
                        }
                    }
                    catch (Exception ex)
                    {
                        EbLog.Error("GetFile api error ::" + ex.Message);
                    }
                }
            }
        }
    }
}