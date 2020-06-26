using ExpressBase.Mobile.CustomControls;
using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Views.Dynamic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ExpressBase.Mobile
{
    public class EbMobileFileUpload : EbMobileControl, INonPersistControl
    {
        public bool EnableCameraSelect { set; get; }

        public bool EnableFileSelect { set; get; }

        public bool MultiSelect { set; get; }

        public bool EnableEdit { set; get; }

        private FileUploader control;

        private List<FileMetaInfo> uploadedFileRef;

        public override void InitXControl(FormMode Mode, NetworkMode Network)
        {
            base.InitXControl(Mode, Network);

            control = new FileUploader();
            control.Initialize(this);
            control.BindFullScreenCallback(ShowFullScreen);
            control.BindDeleteCallback(DeleteFile);
            this.XControl = control;

            //this will create a folder FILES in platform dir 
            Task.Run(async () => { await HelperFunctions.CreateDirectory("FILES"); }); ;
        }

        public void ShowFullScreen(Image image)
        {
            Page navigator = (Application.Current.MainPage as MasterDetailPage).Detail;
            Page current = (navigator as NavigationPage).CurrentPage;
            if (current is FormRender)
            {
                (current as FormRender).ShowFullScreenImage(image);
            }
        }

        public void DeleteFile()
        {

        }

        public override MobileTableColumn GetMobileTableColumn()
        {
            return null;
        }

        public void PushFilesToDir(string TableName, int RowId)
        {
            INativeHelper helper = DependencyService.Get<INativeHelper>();

            List<FileWrapper> files = control.GetFiles(this.Name);

            foreach (FileWrapper wrapr in files)
            {
                wrapr.Name = $"{TableName}-{RowId}-{this.Name}-{Guid.NewGuid().ToString("n").Substring(0, 10)}.jpg";
                File.WriteAllBytes(helper.NativeRoot + $"/ExpressBase/{ App.Settings.Sid.ToUpper()}/FILES/{wrapr.Name}", wrapr.Bytea);
            }
        }

        public override object GetValue()
        {
            List<FileWrapper> files = control.GetFiles(this.Name);

            if (uploadedFileRef != null && files.Any())
            {
                foreach (var meta in uploadedFileRef)
                {
                    files.Add(new FileWrapper
                    {
                        FileRefId = meta.FileRefId,
                        FileName = meta.FileName,
                        IsUploaded = true,
                        ControlName = Name,
                    });
                }
            }
            return files;
        }

        public override bool SetValue(object value)
        {
            if (value != null)
            {
                uploadedFileRef = (value as FUPSetValueMeta).Files;

                control.SetValue(this.NetworkType, value as FUPSetValueMeta, this.Name);
            }
            return true;
        }
    }
}
