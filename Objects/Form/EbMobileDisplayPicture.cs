﻿using ExpressBase.Mobile.CustomControls;
using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Structures;
using System;
using Xamarin.Forms;

namespace ExpressBase.Mobile
{
    public class EbMobileDisplayPicture : EbMobileControl, IFileUploadControl
    {
        public bool EnableCameraSelect { set; get; }

        public bool EnableFileSelect { set; get; }

        public override EbDbTypes EbDbType { get { return EbDbTypes.String; } set { } }

        private FileUploader XamControl;

        public override View Draw(FormMode Mode, NetworkMode Network)
        {
            XamControl = new FileUploader(this);
            XControl = XamControl;

            return base.Draw(Mode, Network);
        }

        public override object GetValue()
        {
            return XamControl.GetFiles(this.Name);
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

        public override void SetValue(object value)
        {
            if (value != null && value is FUPSetValueMeta fupMeta)
            {
                try
                {
                    if (!string.IsNullOrEmpty(fupMeta.FileRefIds))
                    {
                        fupMeta.Files.Add(new FileMetaInfo
                        {
                            FileCategory = EbFileCategory.Images,
                            FileRefId = Convert.ToInt32(fupMeta.FileRefIds)
                        });

                        XamControl.SetValue(this.NetworkType, fupMeta, this.Name);
                    }
                }
                catch (Exception ex)
                {
                    EbLog.Error("[DisplayPicture] setvalue error, " + ex.Message);
                }
            }
        }

        public override bool Validate()
        {
            return base.Validate();
        }
    }
}
