using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using Plugin.Media;
using Plugin.Media.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using Xamarin.Forms;

namespace ExpressBase.Mobile.CustomControls
{
    public class CustomImageWraper : StackLayout
    {
        public string Name { set; get; }

        public CustomImageWraper()
        {

        }

        public CustomImageWraper(string name)
        {
            Name = name;
        }
    }

    public class XFileSelect : XCustomControl
    {
        private Grid _Grid { set; get; }

        private int CurrentRow { set; get; } = 1;

        private int CurrentColumn { set; get; } = 0;

        private int Counter = 0;

        public Dictionary<string, byte[]> Gallery { set; get; }

        public XFileSelect(EbMobileFileUpload Control)
        {
            this.EbControl = Control;
            this.Gallery = new Dictionary<string, byte[]>();

            this.BuildHtml();
            this.AppendButtons();

            //this will create a folder FILES in platform dir 
            HelperFunctions.CreatePlatFormDir("FILES");
        }

        public void BuildHtml()
        {
            this._Grid = new Grid()
            {
                RowSpacing = 5,
                RowDefinitions =
                {
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Star }
                },
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = new GridLength(5, GridUnitType.Star) },
                    new ColumnDefinition { Width = new GridLength(5, GridUnitType.Star) }
                }
            };

            this.XControl = this._Grid;
        }

        public void AppendButtons()
        {
            var ctrl = this.EbControl as EbMobileFileUpload;

            if (ctrl.EnableCameraSelect)
            {
                var CameraBtn = new Button()
                {
                    FontSize = 18,
                    BackgroundColor = Color.FromHex("eeeeee"),
                    Margin = 0,
                    Text = "\uf030",
                    FontFamily = (OnPlatform<string>)HelperFunctions.GetResourceValue("FontAwesome")
                };
                (this.XControl as Grid).Children.Add(CameraBtn, 1, 0);
                CameraBtn.Clicked += OnCameraClick;
                if (!ctrl.EnableFileSelect)
                    Grid.SetColumnSpan(CameraBtn, 2);
            }

            if (ctrl.EnableFileSelect)
            {
                var FilesBtn = new Button()
                {
                    FontSize = 18,
                    TextColor = Color.White,
                    BackgroundColor = Color.FromHex("ffc059"),
                    Margin = 0,
                    Text = "\uf115",
                    FontFamily = (OnPlatform<string>)HelperFunctions.GetResourceValue("FontAwesome")
                };
                (this.XControl as Grid).Children.Add(FilesBtn, 0, 0);
                FilesBtn.Clicked += OnFileClick;
                if (!ctrl.EnableFileSelect)
                    Grid.SetColumnSpan(FilesBtn, 2);
            }

            if (!ctrl.EnableCameraSelect && !ctrl.EnableFileSelect)
            {
                ctrl.Hidden = true;
            }
        }

        public async void OnCameraClick(object o, object e)
        {
            await CrossMedia.Current.Initialize();

            if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
            {
                await Application.Current.MainPage.DisplayAlert("No Camera", ":( No camera available.", "OK");
                return;
            }

            var photo = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions()
            {
                AllowCropping = true
            });

            if (photo != null)
            {
                RenderImage(photo);
            }
        }

        public async void OnFileClick(object o, object e)
        {
            var photo = await CrossMedia.Current.PickPhotoAsync(new PickMediaOptions() { });

            if (photo != null)
            {
                RenderImage(photo);
            }
        }

        private void RenderImage(MediaFile Media)
        {
            string filename = "File" + Counter++;
            CustomImageWraper Wraper = new CustomImageWraper(filename)
            {
                BackgroundColor = Color.FromHex("eeeeee"),
                Children =
                    {
                        new Image
                        {
                            Aspect = Aspect.AspectFill,
                            HeightRequest = 160,
                            Source = ImageSource.FromStream(() => { return Media.GetStream(); })
                        }
                    }
            };

            AddImageToGallery(Wraper);
            Gallery.Add(Wraper.Name, HelperFunctions.StreamToBytea(Media.GetStream()));
        }

        public void AddImageToGallery(CustomImageWraper Wrapper)
        {
            this._Grid.Children.Add(Wrapper, CurrentColumn, CurrentRow);
            if (CurrentColumn == 1)
            {
                this._Grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                CurrentRow++;
                CurrentColumn = 0;
            }
            else
                CurrentColumn = 1;
        }

        public void PushFilesToDir(string TableName, int RowId)
        {
            INativeHelper helper = DependencyService.Get<INativeHelper>();

            foreach (KeyValuePair<string, byte[]> kp in this.Gallery)
            {
                string filename = $"{TableName}-{RowId}-{Guid.NewGuid().ToString("n").Substring(0, 10)}.jpg";
                File.WriteAllBytes(helper.NativeRoot + $"/ExpressBase/{Settings.SolutionId.ToUpper()}/FILES/{filename}", kp.Value);
            }
        }

        public void RenderOnEdit(string TableName, int RowId)
        {
            string sid = Settings.SolutionId.ToUpper();
            INativeHelper helper = DependencyService.Get<INativeHelper>();

            string pattern = $"{TableName}-{RowId}*";
            string[] filenames = helper.GetFiles($"ExpressBase/{sid}/FILES", pattern);

            foreach (string filepath in filenames)
            {
                string filename = Path.GetFileName(filepath);

                var bytes = helper.GetPhoto($"ExpressBase/{sid}/FILES/{filename}");

                CustomImageWraper Wraper = new CustomImageWraper(filename)
                {
                    BackgroundColor = Color.FromHex("eeeeee"),
                    Children =
                    {
                        new Image
                        {
                            Aspect = Aspect.AspectFill,
                            HeightRequest = 160,
                            Source = ImageSource.FromStream(() => new MemoryStream(bytes))
                        }
                    }
                };

                AddImageToGallery(Wraper);
            }
        }
    }
}
