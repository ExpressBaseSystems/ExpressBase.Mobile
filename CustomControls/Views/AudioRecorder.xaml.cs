using ExpressBase.Mobile.Helpers;
using System;
using System.Collections.Generic;
using System.Timers;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ExpressBase.Mobile.CustomControls.Views
{
    public class XAudioButton : Button
    {
        public string Name { set; get; }

        public string ActionType { set; get; }

        public Slider AudioSlider { set; get; }
    }

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AudioRecorder : Frame
    {
        public bool MultiSelect { set; get; }

        private const string ACTION_START = "start";
        private const string ACTION_STOP = "stop";

        readonly IEbAudioHelper audioHelper;
        Timer recordingTimer;
        Timer playerTimer;
        int elapsedMinutes = 0;
        int elapsedSeconds = 0;
        readonly double maxDuration = 120000;//2 minutes

        private readonly Dictionary<string, byte[]> audioFiles = new Dictionary<string, byte[]>();

        public AudioRecorder(EbMobileAudioInput audio)
        {
            InitializeComponent();

            audioHelper = DependencyService.Get<IEbAudioHelper>();
            double max = audio.MaximumDuration <= maxDuration && audio.MaximumDuration > 0 ? audio.MaximumDuration : maxDuration;
            audioHelper.MaximumDuration = max * 60000;
            audioHelper.OnRecordingCompleted += OnRecordingCompleted;
            audioHelper.OnPlayerCompleted += OnPlayerCompleted;
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
                AddAudioFrame(note);
            }
            RecordingCompleted();
        }

        private void OnPlayerCompleted(object sender, EventArgs e)
        {
            XAudioButton audioButton = (XAudioButton)sender;
            audioButton.AudioSlider.Value = 0;
            SetPlayButtonStyle(audioButton);
            playerTimer?.Stop();
            playerTimer = null;
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

        private void AddAudioFrame(byte[] note)
        {
            string name = Guid.NewGuid().ToString("N");
            audioFiles.Add(name, note);

            Frame container = new Frame
            {
                Style = (Style)HelperFunctions.GetResourceValue("AudioRecordFrame")
            };
            var containerInner = new StackLayout { Orientation = Xamarin.Forms.StackOrientation.Horizontal };

            Slider audioSlider = new Slider()
            {
                Style = (Style)HelperFunctions.GetResourceValue("AudioPlaySlider")
            };

            XAudioButton audioBtn = new XAudioButton
            {
                Style = (Style)HelperFunctions.GetResourceValue("AudioPlayButton"),
                AudioSlider = audioSlider,
                Name = name,
                Text = "\uf04b",
                ActionType = ACTION_START,
            };
            audioBtn.Clicked += AudioBtn_Clicked;

            Label lengthLabel = new Label
            {
                VerticalOptions = LayoutOptions.Center,
                Text = $"{elapsedMinutes}:{elapsedSeconds}",
                HorizontalOptions = LayoutOptions.End,
                FontSize = 13
            };

            containerInner.Children.Add(audioBtn);
            containerInner.Children.Add(audioSlider);
            containerInner.Children.Add(lengthLabel);

            container.Content = containerInner;
            RecordList.Children.Add(container);
        }

        private async void AudioBtn_Clicked(object sender, EventArgs e)
        {
            XAudioButton btn = (XAudioButton)sender;
            byte[] audioFile = audioFiles[btn.Name];

            if (btn.ActionType == ACTION_START)
            {
                int duration = await audioHelper.StartPlaying(audioFile, btn);
                SetPauseButtonStyle(btn);

                btn.AudioSlider.Maximum = duration;
                btn.AudioSlider.Minimum = 0;
                btn.AudioSlider.Value = audioHelper.GetPlayerPosition();
                StartSlidingTimer(btn.AudioSlider);
            }
            else if (btn.ActionType == ACTION_STOP)
            {
                audioHelper.StopPlaying();
                SetPlayButtonStyle(btn);
            }
        }

        private void SetPlayButtonStyle(XAudioButton btn)
        {
            btn.Text = "\uf04b";
            btn.ActionType = ACTION_START;
        }

        private void SetPauseButtonStyle(XAudioButton btn)
        {
            btn.Text = "\uf04c";
            btn.ActionType = ACTION_STOP;
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

        private void StartSlidingTimer(Slider slider)
        {
            playerTimer = new Timer(1);
            playerTimer.Elapsed += (sender, e) =>
            {
                Device.BeginInvokeOnMainThread(() => slider.Value = audioHelper.GetPlayerPosition());
            };
            playerTimer.Start();
        }
    }
}