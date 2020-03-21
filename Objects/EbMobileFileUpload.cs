using ExpressBase.Mobile.CustomControls;
using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using Plugin.Media;
using Plugin.Media.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xamarin.Forms;

namespace ExpressBase.Mobile
{
    public class EbMobileFileUpload : EbMobileControl, INonPersistControl
    {
        public bool EnableCameraSelect { set; get; }

        public bool EnableFileSelect { set; get; }

        public bool MultiSelect { set; get; }

        public bool EnableEdit { set; get; }

        //mobile prop
        private Grid Container { set; get; }

        private int CurrentRow { set; get; } = 1;

        private int CurrentColumn { set; get; } = 0;

        private int Counter = 0;

        public Dictionary<string, byte[]> Gallery { set; get; }

        public EbMobileFileUpload()
        {
            this.Gallery = new Dictionary<string, byte[]>();
        }

        public override void InitXControl(FormMode Mode)
        {
            this.BuildXControl();
            this.AppendButtons();

            //this will create a folder FILES in platform dir 
            HelperFunctions.CreatePlatFormDir("FILES");
        }

        public override MobileTableColumn GetMobileTableColumn()
        {
            return null;
        }

        public void BuildXControl()
        {
            this.Container = new Grid()
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

            this.XControl = this.Container;
        }

        public void AppendButtons()
        {
            if (this.EnableCameraSelect)
            {
                var CameraBtn = new Button()
                {
                    FontSize = 18,
                    CornerRadius = 4,
                    BorderColor = Color.FromHex("cccccc"),
                    BorderWidth = 1,
                    BackgroundColor = Color.FromHex("eeeeee"),
                    Margin = 0,
                    Text = "\uf030",
                    FontFamily = (OnPlatform<string>)HelperFunctions.GetResourceValue("FontAwesome")
                };
                (this.XControl as Grid).Children.Add(CameraBtn, 1, 0);
                CameraBtn.Clicked += OnCameraClick;
                if (!this.EnableFileSelect)
                    Grid.SetColumn(CameraBtn, 0);
            }

            if (this.EnableFileSelect)
            {
                var FilesBtn = new Button()
                {
                    FontSize = 18,
                    CornerRadius = 4,
                    BorderColor = Color.FromHex("cccccc"),
                    BorderWidth = 1,
                    TextColor = Color.White,
                    BackgroundColor = Color.FromHex("ffc059"),
                    Margin = 0,
                    Text = "\uf115",
                    FontFamily = (OnPlatform<string>)HelperFunctions.GetResourceValue("FontAwesome")
                };
                (this.XControl as Grid).Children.Add(FilesBtn, 0, 0);
                FilesBtn.Clicked += OnFileClick;
            }

            if (!this.EnableCameraSelect && !this.EnableFileSelect)
                this.Hidden = true;
        }

        public async void OnCameraClick(object o, object e)
        {
            try
            {
                await CrossMedia.Current.Initialize();

                if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
                    return;

                var photo = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions()
                {
                    PhotoSize = PhotoSize.Small,
                    AllowCropping = true
                });

                if (photo != null)
                    RenderImage(photo);
            }
            catch (Exception ex)
            {
                Log.Write("EbMobileFileUpload.OnCameraClick---" + ex.Message);
            }
        }

        public async void OnFileClick(object o, object e)
        {
            try
            {
                var photo = await CrossMedia.Current.PickPhotoAsync(new PickMediaOptions() { PhotoSize = PhotoSize.Small });

                if (photo != null)
                    RenderImage(photo);
            }
            catch (Exception ex)
            {
                Log.Write("EbMobileFileUpload.OnFileClick---" + ex.Message);
            }
        }

        private void RenderImage(MediaFile Media)
        {
            string filename = "file" + Guid.NewGuid().ToString("N") + Counter++;
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
            this.Container.Children.Add(Wrapper, CurrentColumn, CurrentRow);
            if (CurrentColumn == 1)
            {
                this.Container.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
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
                string filename = $"{TableName}-{RowId}-{this.Name}-{Guid.NewGuid().ToString("n").Substring(0, 10)}.jpg";
                File.WriteAllBytes(helper.NativeRoot + $"/ExpressBase/{Settings.SolutionId.ToUpper()}/FILES/{filename}", kp.Value);
            }
        }

        public void RenderOnEdit(string TableName, int RowId)
        {
            string pattern = $"{TableName}-{RowId}-{this.Name}*";
            List<FileWrapper> Files = HelperFunctions.GetFilesByPattern(pattern);

            foreach (FileWrapper file in Files)
            {
                CustomImageWraper Wraper = new CustomImageWraper(file.FileName)
                {
                    BackgroundColor = Color.FromHex("eeeeee"),
                    Children =
                    {
                        new Image
                        {
                            Aspect = Aspect.AspectFill,
                            HeightRequest = 160,
                            Source = ImageSource.FromStream(() => new MemoryStream(file.Bytea))
                        }
                    }
                };
                AddImageToGallery(Wraper);
            }
        }

        public List<FileWrapper> GetFiles()
        {
            List<FileWrapper> files = new List<FileWrapper>();

            foreach (var pair in this.Gallery)
            {
                files.Add(new FileWrapper
                {
                    Name = pair.Key,
                    FileName = pair.Key + ".jpg",
                    Bytea = pair.Value,
                    ControlName = this.Name
                });
            }
            return files;
        }
    }
}
