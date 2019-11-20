using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Structures;
using ExpressBase.Mobile.CustomControls;
using ExpressBase.Mobile.Models;
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

        //public WebformData WebFormData { set; get; }

        //public EbWebForm WebForm { set; get; }

        public bool Status { set; get; }

        //public FormService(IList<Element> Elements, EbWebForm WebForm)
        //{
        //    this.Controls = Elements;
        //    this.WebForm = WebForm;
        //    this.WebFormData = new WebformData();
        //    this.BuildWebFormData();
        //}

        //private void BuildWebFormData()
        //{
        //    this.WebFormData.MasterTable = this.WebForm.TableName;
        //    SingleTable Table = new SingleTable();
        //    SingleRow row = new SingleRow();
        //    row.RowId = "0";
        //    row.IsUpdate = false;

        //    Table.Add(row);
        //    foreach (Element el in this.Controls)
        //    {
        //        if (el is TextBox)
        //        {
        //            row.Columns.Add(new SingleColumn
        //            {
        //                Name = el.ClassId,
        //                Type = (int)(el as TextBox).DbType,
        //                Value = (el as TextBox).Text
        //            });
        //        }
        //        else if (el is CustomDatePicker)
        //        {
        //            row.Columns.Add(new SingleColumn
        //            {
        //                Name = el.ClassId,
        //                Type = (int)(el as CustomDatePicker).DbType,
        //                Value = (el as CustomDatePicker).Date.ToString("yyyy-MM-dd")
        //            });
        //        }
        //        else if (el is CustomSelect)
        //        {
        //            EbSimpleSelectOption opt = (el as CustomSelect).SelectedItem as EbSimpleSelectOption;

        //            row.Columns.Add(new SingleColumn
        //            {
        //                Name = el.ClassId,
        //                Type = (int)(el as CustomSelect).DbType,
        //                Value = (opt == null) ? null : opt.Value
        //            });
        //        }
        //        else if(el is FileInput)
        //        {

        //        }
        //    }

        //    this.WebFormData.MultipleTables.Add(this.WebForm.TableName, Table);
        //    string json = JsonConvert.SerializeObject(this.WebFormData);

        //    this.PushToCloud(json);
        //}

        //private void PushToCloud(string json)
        //{
        //    WebFormSaveResponse resp = CommonServices.PushWebFormData(json, this.WebForm.RefId, Settings.LocationId, 0);

        //    if (resp.RowAffected > 0)
        //    {
        //        this.Status = true;
        //    }
        //    else
        //    {
        //        this.Status = false;
        //    }
        //}
    }
}
