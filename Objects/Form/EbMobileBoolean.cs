﻿using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Structures;
using System;
using Xamarin.Forms;

namespace ExpressBase.Mobile
{
    public class EbMobileBoolean : EbMobileControl
    {
        public override EbDbTypes EbDbType { get { return EbDbTypes.BooleanOriginal; } set { } }

        public override object SQLiteToActual(object value)
        {
            if (int.TryParse(value?.ToString(), out int t) && t == 1)
                return true;
            else
                return false;
        }

        public override object ActualToSQLite(object value)
        {
            if (bool.TryParse(value?.ToString(), out bool t) && t)
                return 1;
            else
                return 0;
        }

        public override View Draw(FormMode Mode, NetworkMode Network)
        {
            CheckBox check = new CheckBox
            {
                Color = (Color)HelperFunctions.GetResourceValue("Primary_Color")
            };

            check.CheckedChanged += (sender, arg) => this.ValueChanged();
            XControl = check;

            return base.Draw(Mode, Network);
        }

        public override object GetValue()
        {
            return (this.XControl as CheckBox).IsChecked;
        }

        public override void SetValue(object value)
        {
            try
            {
                if (value != null)
                {
                    bool isChecked = false;

                    if (value is int)
                        isChecked = Convert.ToInt32(value) != 0;
                    else if (value is bool boolean)
                        isChecked = boolean;
                    else if (value is string s)
                        isChecked = bool.Parse(s);

                    (this.XControl as CheckBox).IsChecked = isChecked;
                }
            }
            catch (Exception ex)
            {
                EbLog.Error("Boolean setvalue error");
                EbLog.Error(ex.Message);
            }
        }

        public override MobileTableColumn GetMobileTableColumn()
        {
            bool value = this.GetValue<bool>();

            return new MobileTableColumn
            {
                Name = this.Name,
                Type = this.EbDbType,
                Value = value,
                Control = this
            };
        }

        public override void Reset()
        {
            (this.XControl as CheckBox).ClearValue(CheckBox.IsCheckedProperty);
        }

        public override bool Validate()
        {
            if (this.Required && !(this.XControl as CheckBox).IsChecked)
                return false;

            return true;
        }
    }
}
