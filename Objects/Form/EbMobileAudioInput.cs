using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.CustomControls;
using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;

namespace ExpressBase.Mobile
{
    public class EbMobileAudioInput : EbMobileControl, IFileUploadControl
    {
        public double MaximumDuration { set; get; }

        public bool MultiSelect { set; get; }

        public override EbDbTypes EbDbType { get { return EbDbTypes.String; } set { } }

        private AudioRecorder recorder;

        public override View Draw(FormMode mode, NetworkMode network)
        {
            recorder = new AudioRecorder(this);
            this.XControl = recorder;

            return base.Draw(mode, network);
        }

        public override MobileTableColumn GetMobileTableColumn()
        {
            return new MobileTableColumn
            {
                Name = this.Name,
                Type = this.EbDbType,
                Control = this
            };
        }

        public override object GetValue()
        {
            return recorder.GetFiles(this.Name);
        }

        public override void SetValue(object value)
        {
            if (value != null && value is FUPSetValueMeta fupMeta)
            {
                try
                {
                    if (!string.IsNullOrEmpty(fupMeta.FileRefIds))
                    {
                        this.OldValue = fupMeta.FileRefIds;

                        string[] refids = fupMeta.FileRefIds.Split(CharConstants.COMMA);

                        foreach (string id in refids)
                        {
                            fupMeta.Files.Add(new FileMetaInfo
                            {
                                FileCategory = EbFileCategory.Audio,
                                FileRefId = Convert.ToInt32(id)
                            });
                        }
                        recorder.SetValue(this.NetworkType, fupMeta, this.Name);
                    }
                }
                catch (Exception ex)
                {
                    EbLog.Error("[AudioInput] setvalue error, " + ex.Message);
                }
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
