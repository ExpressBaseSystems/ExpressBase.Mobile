using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Services;
using Plugin.Media;
using Plugin.Media.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ExpressBase.Mobile.CustomControls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class FileUploader : ContentView
    {
        private int counter = 0;

        private double thumbnailWidth;

        private Dictionary<string, byte[]> Files { set; get; }

        private readonly TapGestureRecognizer recognizer;

        private Action<Image> bindable;

        public FileUploader()
        {
            InitializeComponent();

            Files = new Dictionary<string, byte[]>();
            recognizer = new TapGestureRecognizer();
            recognizer.Tapped += ThumbNail_Tapped;
        }

        public void Initialize(bool hasCamera, bool hasFile)
        {
            if (!hasCamera)
            {
                CameraButton.IsVisible = false;
            }
            if (!hasFile)
            {
                FilesButton.IsVisible = false;
                if (!hasCamera)
                {
                    Grid.SetColumn(FilesButton, 0);
                }
            }
        }

        private async void CameraButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                await CrossMedia.Current.Initialize();

                if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
                    return;

                MediaFile photo = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions()
                {
                    PhotoSize = PhotoSize.Small,
                    AllowCropping = true
                });

                if (photo != null)
                    AppendToGallery(photo);
            }
            catch (Exception ex)
            {
                EbLog.Write(ex.Message);
            }
        }

        private void Container_SizeChanged(object sender, EventArgs e)
        {
            thumbnailWidth = (sender as FlexLayout).Width / 3 - 10;
        }

        private async void FilesButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                var photo = await CrossMedia.Current.PickPhotoAsync(new PickMediaOptions() { PhotoSize = PhotoSize.Small });

                if (photo != null)
                    AppendToGallery(photo);
            }
            catch (Exception ex)
            {
                EbLog.Write(ex.Message);
            }
        }

        private void AppendToGallery(MediaFile media)
        {
            var bytea = HelperFunctions.StreamToBytea(media.GetStream());

            CustomImageWraper thumbnail = GetTemplate(bytea);
            Container.Children.Add(thumbnail);
            Files.Add(thumbnail.Name, bytea);

            ToggleGalleryBG();
        }

        public void AppendToGallery(string filename, byte[] bytea)
        {
            CustomImageWraper thumbnail = GetTemplate(bytea, filename, false);
            Container.Children.Add(thumbnail);

            ToggleGalleryBG();
        }

        private CustomImageWraper GetTemplate(byte[] bytea, string name = null, bool hasClose = true)
        {
            string filename = name ?? "file" + Guid.NewGuid().ToString("N") + counter++;

            CustomImageWraper wraper = new CustomImageWraper(filename, thumbnailWidth);

            Image image = new Image
            {
                Aspect = Aspect.AspectFill,
                Source = ImageSource.FromStream(() => { return new MemoryStream(bytea); }),
                GestureRecognizers = { recognizer }
            };
            AbsoluteLayout.SetLayoutBounds(image, new Rectangle(0, 0, 1, 1));
            AbsoluteLayout.SetLayoutFlags(image, AbsoluteLayoutFlags.All);
            wraper.Children.Add(image);

            if (hasClose)
            {
                Button closeBtn = new Button
                {
                    ClassId = filename,
                    Style = (Style)HelperFunctions.GetResourceValue("FupThumbCloseButton")
                };
                closeBtn.Clicked += CloseBtn_Clicked;
                AbsoluteLayout.SetLayoutBounds(closeBtn, new Rectangle(0.95, 0.05, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize));
                AbsoluteLayout.SetLayoutFlags(closeBtn, AbsoluteLayoutFlags.PositionProportional);
                wraper.Children.Add(closeBtn);
            }

            return wraper;
        }

        private void CloseBtn_Clicked(object sender, EventArgs e)
        {
            string classid = (sender as Button).ClassId;
            foreach (var item in Container.Children)
            {
                if (item.ClassId == classid && item is CustomImageWraper)
                {
                    Container.Children.Remove(item);
                    Files.Remove(classid);
                    break;
                }
            }
            ToggleGalleryBG();
        }

        private void ThumbNail_Tapped(object sender, EventArgs e)
        {
            bindable?.Invoke((sender as Image));
        }

        public List<FileWrapper> GetFiles(string ctrlName)
        {
            List<FileWrapper> files = new List<FileWrapper>();

            foreach (var pair in Files)
            {
                files.Add(new FileWrapper
                {
                    Name = pair.Key,
                    FileName = pair.Key + ".jpg",
                    Bytea = pair.Value,
                    ControlName = ctrlName
                });
            }
            return files;
        }

        public void BindFullScreenCallback(Action<Image> method)
        {
            bindable = method;
        }

        private void ToggleGalleryBG()
        {
            Gallery.BackgroundColor = Container.Children.Any() ? Color.FromHex("eeeeee") : Color.Transparent;
        }

        public async void SetValue(NetworkMode nw, FUPSetValueMeta meta, string ctrlname)
        {
            if (nw == NetworkMode.Offline)
            {
                string pattern = $"{meta.TableName}-{meta.RowId}-{ctrlname}*";
                List<FileWrapper> Files = HelperFunctions.GetFilesByPattern(pattern);

                foreach (FileWrapper file in Files)
                {
                    this.AppendToGallery(file.FileName, file.Bytea);
                }
            }
            else if (nw == NetworkMode.Online)
            {
                foreach (FileMetaInfo info in meta.Files)
                {
                    try
                    {
                        ApiFileResponse resp = await FormDataServices.Instance.GetFile(info.FileCategory, $"{info.FileRefId}.jpg");
                        if (resp != null && resp.HasContent)
                        {
                            this.AppendToGallery(info.FileName, resp.Bytea);
                        }
                    }
                    catch(Exception ex)
                    {
                        EbLog.Write("GetFile api error ::" + ex.Message);
                    }
                }
            }
        }
    }
}