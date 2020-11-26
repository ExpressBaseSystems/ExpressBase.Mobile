using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Views.Base;
using Plugin.SimpleAudioPlayer;
using System;
using System.IO;
using Xamarin.Forms;

namespace ExpressBase.Mobile.CustomControls
{
    public class EbAudioTemplate
    {
        public string Name { set; get; }

        public bool AllowDelete { set; get; } = true;

        public event EbEventHandler OnDelete;

        const string ACTION_START = "start";

        const string ACTION_STOP = "stop";

        XAudioButton playButton;

        Slider slider;

        Label lengthLabel;

        readonly ISimpleAudioPlayer player;

        bool paused;

        public EbAudioTemplate()
        {
            player = CrossSimpleAudioPlayer.CreateSimpleAudioPlayer();
        }

        public EbAudioTemplate(byte[] audio, string name = null)
        {
            player = CrossSimpleAudioPlayer.CreateSimpleAudioPlayer();
            player.Load(ByteToStream(audio));
            Name = name ?? Guid.NewGuid().ToString("N");
        }

        public View CreateView()
        {
            Frame container = new Frame
            {
                Style = (Style)HelperFunctions.GetResourceValue("AudioRecordFrame")
            };
            var containerInner = new StackLayout { Orientation = Xamarin.Forms.StackOrientation.Horizontal };

            slider = new Slider()
            {
                Style = (Style)HelperFunctions.GetResourceValue("AudioPlaySlider")
            };
            slider.ValueChanged += SliderPostionValueChanged;

            playButton = new XAudioButton
            {
                Style = (Style)HelperFunctions.GetResourceValue("AudioPlayButton"),
                Name = Name,
                Text = "\uf04b",
                ActionType = ACTION_START,
            };
            playButton.Clicked += PlayButtonClicked;

            lengthLabel = new Label
            {
                VerticalOptions = LayoutOptions.Center,
                Text = $"0/{(int)player.Duration}",
                FontSize = 13
            };

            containerInner.Children.Add(playButton);
            containerInner.Children.Add(slider);
            containerInner.Children.Add(lengthLabel);

            if (AllowDelete)
            {
                Button delete = new Button
                {
                    VerticalOptions = LayoutOptions.Center,
                    Style = (Style)HelperFunctions.GetResourceValue("AudioPlayButton"),
                    HorizontalOptions = LayoutOptions.End,
                    Text = "\uf1f8",
                    TextColor = Color.Red
                };

                delete.Clicked += Delete_Clicked;
                containerInner.Children.Add(delete);
            }

            container.Content = containerInner;

            return container;
        }

        private void Delete_Clicked(object sender, EventArgs e)
        {
            OnDelete?.Invoke(Name, null);
        }

        private void PlayButtonClicked(object sender, EventArgs e)
        {
            if (playButton.ActionType == ACTION_START)
            {
                paused = false;
                player.Play();
                slider.Maximum = player.Duration;
                slider.IsEnabled = player.CanSeek;
                SetPauseButtonStyle();
                Device.StartTimer(TimeSpan.FromSeconds(0.5), UpdateSliderPosition);
            }
            else if (playButton.ActionType == ACTION_STOP)
            {
                paused = true;
                player.Pause();
                SetPlayButtonStyle();
            }
        }

        bool UpdateSliderPosition()
        {
            lengthLabel.Text = $"{(int)player.CurrentPosition}/{(int)player.Duration}";

            slider.ValueChanged -= SliderPostionValueChanged;
            slider.Value = player.CurrentPosition;
            slider.ValueChanged += SliderPostionValueChanged;

            if (!player.IsPlaying && !paused)
            {
                OnPlayerCompletes();
            }
            return player.IsPlaying;
        }

        void OnPlayerCompletes()
        {
            slider.ClearValue(Slider.ValueProperty);
            lengthLabel.Text = $"0/{(int)player.Duration}";
            SetPlayButtonStyle();
        }

        private void SetPlayButtonStyle()
        {
            playButton.Text = "\uf04b";
            playButton.ActionType = ACTION_START;
        }

        private void SetPauseButtonStyle()
        {
            playButton.Text = "\uf04c";
            playButton.ActionType = ACTION_STOP;
        }

        private Stream ByteToStream(byte[] audio)
        {
            return new MemoryStream(audio);
        }

        private void SliderPostionValueChanged(object sender, ValueChangedEventArgs e)
        {
            if (slider.Value != player.Duration)
                player.Seek(slider.Value);
        }
    }
}