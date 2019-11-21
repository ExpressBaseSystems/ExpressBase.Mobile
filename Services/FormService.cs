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

        public MobileFormData FormData { set; get; }

        public EbMobileForm Form { set; get; }

        public bool Status { set; get; }

        public FormService(IList<Element> Elements, EbMobileForm form)
        {
            this.Controls = Elements;
            this.Form = form;
            this.FormData = new MobileFormData();
            this.BuildFormData();
        }

        private void BuildFormData()
        {
            this.FormData.MasterTable = this.Form.TableName;
            MobileTable Table = new MobileTable();
            MobileTableRow row = new MobileTableRow();
            row.RowId = 0;
            row.IsUpdate = false;

            Table.Add(row);
            foreach (Element el in this.Controls)
            {
                if (el is TextBox)
                {
                    row.Columns.Add(new MobileTableColumn
                    {
                        Name = el.ClassId,
                        Type = (el as TextBox).DbType,
                        Value = (el as TextBox).Text
                    });
                }
                else if (el is CustomDatePicker)
                {
                    row.Columns.Add(new MobileTableColumn
                    {
                        Name = el.ClassId,
                        Type = (el as CustomDatePicker).DbType,
                        Value = (el as CustomDatePicker).Date.ToString("yyyy-MM-dd")
                    });
                }
                else if (el is CustomSelect)
                {
                    EbMobileSSOption opt = (el as CustomSelect).SelectedItem as EbMobileSSOption;

                    row.Columns.Add(new MobileTableColumn
                    {
                        Name = el.ClassId,
                        Type = (el as CustomSelect).DbType,
                        Value = (opt == null) ? null : opt.Value
                    });
                }
                else if (el is FileInput)
                {

                }
            }

            this.FormData.Tables.Add(Table);
            string json = JsonConvert.SerializeObject(this.FormData);

            this.PushToCloud(json);
        }

        private void PushToCloud(string json)
        {
            //var resp = CommonServices.PushWebFormData(json, this.Page.RefId, Settings.LocationId, 0);

            //if (resp.RowAffected > 0)
            //{
            //    this.Status = true;
            //}
            //else
            //{
            //    this.Status = false;
            //}
        }
    }
}
