using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Services;
using ExpressBase.Mobile.Views.Base;
using ExpressBase.Mobile.Views.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ExpressBase.Mobile.CustomControls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class EbSignaturePad : ContentView
    {

        private int counter = 0;

        private byte[] ImageBytea;

        private TapGestureRecognizer ClickHereRecognizer;

        private TapGestureRecognizer ImageRecognizer;

        private EbMobileSignaturePad signPadCtrl;

        public bool IsSignFormVisible;

        public EbSignaturePad(EbMobileSignaturePad control)
        {
            this.signPadCtrl = control;
            InitializeComponent();
            Initialize(control);
        }

        private void Initialize(EbMobileSignaturePad control)
        {
            ClickHereLayout.IsVisible = true;
            ImageLayout.IsVisible = false;

            ClickHereRecognizer = new TapGestureRecognizer();
            ClickHereRecognizer.Tapped += ClickHere_Tapped;
            ClickHereLayout.GestureRecognizers.Add(ClickHereRecognizer);

            ImageRecognizer = new TapGestureRecognizer();
            ImageRecognizer.Tapped += ThumbNail_Tapped;
            ImageCont.GestureRecognizers.Add(ImageRecognizer);
        }

        private void ThumbNail_Tapped(object sender, EventArgs e)
        {
            if (App.Navigation.GetCurrentPage() is IFormRenderer rendrer)
            {
                rendrer.ShowFullScreenImage((sender as Image).Source);
            }
        }

        private async void ClickHere_Tapped(object sender, EventArgs e)
        {
            try
            {
                if (!IsSignFormVisible)
                {
                    IsSignFormVisible = true;
                    SignaturePadForm signPadView = new SignaturePadForm(this);
                    signPadView.OnSigningDone += (imgStream) =>
                    {
                        this.OnSigningDone(imgStream);
                    };
                    await App.Navigation.NavigateMasterModalAsync(signPadView);
                }
            }
            catch (Exception ex)
            {
                EbLog.Error(ex.Message);
            }
        }

        private void OnSigningDone(Stream imgStream)
        {
            ImageBytea = HelperFunctions.StreamToBytea(imgStream);
            ImageCont.Source = ImageSource.FromStream(() => { return new MemoryStream(ImageBytea); });

            ClickHereLayout.IsVisible = false;
            ImageLayout.IsVisible = true;
        }

        private void ImageCloseBtn_Clicked(object sender, EventArgs e)
        {
            ImageBytea = null;
            ClickHereLayout.IsVisible = true;
            ImageLayout.IsVisible = false;
        }

        public List<FileWrapper> GetFiles(string ctrlName)
        {
            List<FileWrapper> files = new List<FileWrapper>();

            if (ImageBytea != null)
            {
                string filename = "file" + Guid.NewGuid().ToString("N") + counter++;
                files.Add(new FileWrapper
                {
                    Name = filename,
                    FileName = filename + ".png",
                    Bytea = ImageBytea,
                    ControlName = ctrlName
                });
            }
            return files;
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
                        ApiFileResponse resp = await FormService.Instance.GetFile(info.FileCategory, $"{info.FileRefId}.png");
                        if (resp != null && resp.HasContent)
                        {
                            this.AppendToGallery(info.FileName, resp.Bytea);
                        }
                    }
                    catch (Exception ex)
                    {
                        EbLog.Error("GetFile api error ::" + ex.Message);
                    }
                }
            }
        }

        public void AppendToGallery(string filename, byte[] bytea)
        {
            ImageCont.Source = ImageSource.FromStream(() => { return new MemoryStream(bytea); });

            ClickHereLayout.IsVisible = false;
            ImageLayout.IsVisible = true;
        }
    }
}