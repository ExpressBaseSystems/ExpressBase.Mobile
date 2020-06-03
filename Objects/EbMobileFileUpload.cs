using ExpressBase.Mobile.CustomControls;
using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Services;
using ExpressBase.Mobile.Views.Dynamic;
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
        private Grid container;

        private int currentRow = 1;

        private int currentColumn = 0;

        private int counter = 0;

        public Dictionary<string, byte[]> Gallery { set; get; }

        private readonly TapGestureRecognizer recognizer;

        public EbMobileFileUpload()
        {
            this.Gallery = new Dictionary<string, byte[]>();
            recognizer = new TapGestureRecognizer();
            recognizer.Tapped += Recognizer_Tapped;
        }

        private void Recognizer_Tapped(object sender, EventArgs e)
        {
            try
            {
                Page navigator = (Application.Current.MainPage as MasterDetailPage).Detail;
                FormRender current = (navigator as NavigationPage).CurrentPage as FormRender;
                current.ShowFullScreenImage(sender);
            }
            catch (Exception ex)
            {
                Log.Write(ex.Message);
            }
        }

        public override void InitXControl(FormMode Mode, NetworkMode Network)
        {
            base.InitXControl(Mode, Network);

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
            this.container = new Grid()
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

            this.XControl = this.container;
        }

        public void AppendButtons()
        {
            if (this.EnableCameraSelect)
            {
                var CameraBtn = new Button()
                {
                    Style = (Style)HelperFunctions.GetResourceValue("FupCameraButton")
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
                    Style = (Style)HelperFunctions.GetResourceValue("FupFileButton")
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
            try
            {
                string filename = "file" + Guid.NewGuid().ToString("N") + counter++;
                CustomImageWraper Wraper = new CustomImageWraper(filename);
                var image = new Image
                {
                    Aspect = Aspect.AspectFill,
                    HeightRequest = 160,
                    Source = ImageSource.FromStream(() => { return Media.GetStream(); }),
                    GestureRecognizers = { recognizer }
                };
                AbsoluteLayout.SetLayoutBounds(image, new Rectangle(0, 0, 1, 1));
                AbsoluteLayout.SetLayoutFlags(image, AbsoluteLayoutFlags.All);
                Wraper.Children.Add(image);

                var closeBtn = new Button
                {
                    ClassId = filename,
                    Style = (Style)HelperFunctions.GetResourceValue("FupThumbCloseButton")
                };
                closeBtn.Clicked += CloseBtn_Clicked;
                AbsoluteLayout.SetLayoutBounds(closeBtn, new Rectangle(0.95, 0.05, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize));
                AbsoluteLayout.SetLayoutFlags(closeBtn, AbsoluteLayoutFlags.PositionProportional);
                Wraper.Children.Add(closeBtn);

                AddImageToGallery(Wraper);
                Gallery.Add(filename, HelperFunctions.StreamToBytea(Media.GetStream()));
            }
            catch (Exception ex)
            {
                Log.Write(ex.Message);
            }
        }

        private void CloseBtn_Clicked(object sender, EventArgs e)
        {
            string classid = (sender as Button).ClassId;
            foreach (var item in this.container.Children)
            {
                if (item.ClassId == classid && item is CustomImageWraper)
                {
                    (item as CustomImageWraper).Children.Clear();
                    Gallery.Remove(classid);
                    break;
                }
            }
        }

        public void AddImageToGallery(CustomImageWraper Wrapper)
        {
            this.container.Children.Add(Wrapper, currentColumn, currentRow);
            if (currentColumn == 1)
            {
                this.container.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                currentRow++;
                currentColumn = 0;
            }
            else
                currentColumn = 1;
        }

        public void PushFilesToDir(string TableName, int RowId)
        {
            INativeHelper helper = DependencyService.Get<INativeHelper>();

            foreach (KeyValuePair<string, byte[]> kp in this.Gallery)
            {
                string filename = $"{TableName}-{RowId}-{this.Name}-{Guid.NewGuid().ToString("n").Substring(0, 10)}.jpg";
                File.WriteAllBytes(helper.NativeRoot + $"/ExpressBase/{ App.Settings.Sid.ToUpper()}/FILES/{filename}", kp.Value);
            }
        }

        public override bool SetValue(object value)
        {
            FUPSetValueMeta meta = value as FUPSetValueMeta;
            try
            {
                if (this.NetworkType == NetworkMode.Offline)
                {
                    string pattern = $"{meta.TableName}-{meta.RowId}-{this.Name}*";
                    List<FileWrapper> Files = HelperFunctions.GetFilesByPattern(pattern);

                    foreach (FileWrapper file in Files)
                    {
                        this.AddImage(file.FileName, file.Bytea);
                    }
                }
                else if (this.NetworkType == NetworkMode.Online)
                {
                    foreach (FileMetaInfo info in meta.Files)
                    {
                        ApiFileResponse resp = FormDataServices.Instance.GetFile(info.FileCategory, $"{info.FileRefId}.jpg");
                        if (resp != null && resp.HasContent)
                            this.AddImage(info.FileName, resp.Bytea);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex.Message);
            }
            return true;
        }


        private void AddImage(string filename, byte[] bytea)
        {
            CustomImageWraper Wraper = new CustomImageWraper(filename)
            {
                Children =
                {
                    new Image
                    {
                        Aspect = Aspect.AspectFill,
                        HeightRequest = 160,
                        Source = ImageSource.FromStream(() => new MemoryStream(bytea)),
                        GestureRecognizers = { recognizer }
                    }
                }
            };
            AddImageToGallery(Wraper);
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
