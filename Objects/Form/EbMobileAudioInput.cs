using ExpressBase.Mobile.CustomControls.Views;
using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Structures;
using System.Collections.Generic;
using System.Linq;

namespace ExpressBase.Mobile
{
    public class EbMobileAudioInput : EbMobileControl, IFileUploadControl
    {
        public double MaximumDuration { set; get; }

        public bool MultiSelect { set; get; }

        public override EbDbTypes EbDbType { get { return EbDbTypes.String; } set { } }

        private AudioRecorder recorder;

        private List<FileMetaInfo> uploadedFileRef;

        public override void InitXControl(FormMode mode, NetworkMode network)
        {
            base.InitXControl(mode, network);

            recorder = new AudioRecorder(this);
            this.XControl = recorder;
        }

        public override MobileTableColumn GetMobileTableColumn()
        {
            return new MobileTableColumn
            {
                Name = this.Name,
                Type = this.EbDbType
            };
        }

        public override object GetValue()
        {
            List<FileWrapper> files = recorder.GetFiles(this.Name);

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

        public override void SetValue(object value)
        {
            if (value != null)
            {
                uploadedFileRef = (value as FUPSetValueMeta).Files;
                recorder.SetValue(this.NetworkType, value as FUPSetValueMeta, this.Name);
            }
        }

        public override bool Validate()
        {
            List<FileWrapper> files = this.GetValue<List<FileWrapper>>();

            if (this.Required && !files.Any())
                return false;

            return true;
        }
    }
}
