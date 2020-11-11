using ExpressBase.Mobile.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
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
        private const string ACTION_PLAY = "play";
        private const string ACTION_STOP = "stop";

        private readonly EbMobileAudioInput audioControl;

        IEbAudioHelper audioHelper;

        private Dictionary<string, byte[]> audioFiles = new Dictionary<string, byte[]>();

        bool progress = false;

        public AudioRecorder(EbMobileAudioInput audio)
        {
            InitializeComponent();

            audioControl = audio;
            audioHelper = DependencyService.Get<IEbAudioHelper>();
        }

        private async void RecordButton_Clicked(object sender, EventArgs e)
        {
            if (await AppPermission.Audio())
            {
                StopRecording.IsVisible = true;
                RecordingTimer.IsVisible = true;
                string path = await audioHelper.StartRecording();
                UpdateRecordingTimer();
            }
            else
            {
                Utils.Toast("Microphone permission revoked");
            }
        }

        private async void StopRecording_Clicked(object sender, EventArgs e)
        {
            progress = false;
            byte[] note = await audioHelper.StopRecording();

            StopRecording.IsVisible = false;
            if (note != null)
            {
                AddAudioFrame(note);
            }
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
            audioSlider.DragCompleted += AudioSlider_DragCompleted;

            XAudioButton audioBtn = new XAudioButton
            {
                Style = (Style)HelperFunctions.GetResourceValue("AudioPlayButton"),
                AudioSlider = audioSlider,
                Name = name,
                Text = "\uf04b",
                ActionType = ACTION_PLAY,
            };
            audioBtn.Clicked += AudioBtn_Clicked;

            containerInner.Children.Add(audioBtn);
            containerInner.Children.Add(audioSlider);
            container.Content = containerInner;

            RecordList.Children.Add(container);
        }

        private void AudioSlider_DragCompleted(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private async void AudioBtn_Clicked(object sender, EventArgs e)
        {
            XAudioButton btn = (XAudioButton)sender;
            var audioFile = audioFiles[btn.Name];

            if (btn.ActionType == ACTION_PLAY)
            {
                await audioHelper.StartPlaying(audioFile);
            }
            else if (btn.ActionType == ACTION_STOP)
            {
                audioHelper.StopPlaying();
            }
        }

        private void UpdateRecordingTimer()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                
            });
        }
    }
}