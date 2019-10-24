using ExpressBase.Mobile.Common.Data;
using ExpressBase.Mobile.Common.Structures;
using ExpressBase.Mobile.CustomControls;
using ExpressBase.Mobile.Objects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace ExpressBase.Mobile.Services
{
    public class FormService
    {
        protected IList<Element> Controls { set; get; }

        public WebformData WebFormData { set; get; }

        public EbWebForm WebForm { set; get; }

        public FormService(IList<Element> Elements, EbWebForm WebForm)
        {
            this.Controls = Elements;
            this.WebForm = WebForm;
            this.WebFormData = new WebformData();
            this.BuildWebFormData();
        }

        private void BuildWebFormData()
        {
            this.WebFormData.MasterTable = this.WebForm.TableName;
            SingleTable Table = new SingleTable();
            SingleRow row = new SingleRow();
            row.RowId = "0";
            row.IsUpdate = false;

            Table.Add(row);
            foreach (Element el in this.Controls)
            {
                dynamic _value = null;
                int _type = (int)EbDbTypes.String;

                if(el is TextBox)
                {
                    _value = (el as TextBox).Text;
                    _type = (int)(el as TextBox).DbType;
                }
                else if (el is CustomDatePicker)
                {
                    _value = (el as CustomDatePicker).Date;
                    _type = (int)(el as CustomDatePicker).DbType;
                }
                else if (el is CustomSelect)
                {
                    EbSimpleSelectOption opt = (el as CustomSelect).SelectedItem as EbSimpleSelectOption;
                    _value = opt.Value;
                    _type = (int)(el as CustomSelect).DbType;
                }

                row.Columns.Add(new SingleColumn
                {
                    Name = el.ClassId,
                    Type = _type,
                    Value = _value
                });
            }
            this.WebFormData.MultipleTables.Add(this.WebForm.TableName, Table);
            string json = JsonConvert.SerializeObject(this.WebFormData);
        }
    }
}
