using Plugin.Media;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace ExpressBase.Mobile.CustomControls
{

    public class XFileSelect : XCustomControl
    {
        private Grid _Grid { set; get; }

        public XFileSelect(EbMobileFileUpload Control)
        {
            this.EbControl = Control;

            this.BuildHtml();
            this.AppendButtons();
        }

        public void BuildHtml()
        {
            this._Grid = new Grid();

            this._Grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            this._Grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            this._Grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(5, GridUnitType.Star) });
            this._Grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(5, GridUnitType.Star) });

            this.XControl = this._Grid;
        }

        public void AppendButtons()
        {
            var FilesBtn = new Button() { Text = "Choose File" };
            (this.XControl as Grid).Children.Add(FilesBtn, 0, 0);

            var CameraBtn = new Button() { Text = "Take pic" };
            (this.XControl as Grid).Children.Add(CameraBtn, 1, 0);

            //bind events
            CameraBtn.Clicked += OnCameraClick;
            FilesBtn.Clicked += OnFileClick;
        }

        public async void OnCameraClick(object o, object e)
        {
            await CrossMedia.Current.Initialize();

            if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
            {
                await Application.Current.MainPage.DisplayAlert("No Camera", ":( No camera available.", "OK");
                return;
            }

            var photo = await CrossMedia.Current.TakePhotoAsync(new Plugin.Media.Abstractions.StoreCameraMediaOptions()
            {
                AllowCropping = true
            });

            if (photo != null)
            {
                Image _img = new Image
                {
                    Source = ImageSource.FromStream(() => { return photo.GetStream(); })
                };
                this._Grid.Children.Add(_img, 0, 1);
                Grid.SetColumnSpan(_img, 2);
            }
        }

        public async void OnFileClick(object o, object e)
        {
            var photo = await CrossMedia.Current.PickPhotoAsync(new Plugin.Media.Abstractions.PickMediaOptions() { });

            if (photo != null)
            {
                Image _img = new Image
                {
                    Source = ImageSource.FromStream(() => { return photo.GetStream(); })
                };
                this._Grid.Children.Add(_img);
                this._Grid.Children.Add(_img, 0, 1);
                Grid.SetColumnSpan(_img, 2);
            }
        }
    }
}
