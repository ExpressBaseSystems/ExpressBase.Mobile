using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Structures;
using ExpressBase.Mobile.CustomControls;
using ExpressBase.Mobile.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;
using System.Linq;

namespace ExpressBase.Mobile.Services
{
    public class FormService
    {
        protected IList<Element> Controls { set; get; }

        public EbMobileForm Form { set; get; }

        public bool Status { set; get; }

        public FormService(IList<Element> Elements, EbMobileForm form)
        {
            this.Controls = Elements;
            this.Form = form;
        }

        public bool Save()
        {
            MobileFormData data = this.GetFormData();
            string query = string.Empty;
            try
            {
                if (data.Tables.Count > 0)
                {
                    List<DbParameter> _params = new List<DbParameter>();
                    foreach (MobileTable _table in data.Tables)
                        query += this.GetTQndDbP(_table, _params);

                    int rowAffected = App.DataDB.DoNonQuery(query, _params.ToArray());
                    return (rowAffected > 0);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return false;
        }

        private MobileFormData GetFormData()
        {
            MobileFormData FormData = new MobileFormData
            {
                MasterTable = this.Form.TableName
            };

            MobileTable Table = new MobileTable { TableName = this.Form.TableName };
            MobileTableRow row = new MobileTableRow
            {
                RowId = 0,
                IsUpdate = false
            };

            Table.Add(row);
            foreach (Element el in this.Controls)
            {
                if (el is FileInput)
                {

                }
                else
                {
                    ICustomElement xctrl = el as ICustomElement;

                    row.Columns.Add(new MobileTableColumn
                    {
                        Name = xctrl.Name,
                        Type = xctrl.DbType,
                        Value = xctrl.GetValue()
                    });
                }
            }

            FormData.Tables.Add(Table);
            return FormData;
        }

        private string GetTQndDbP(MobileTable Table, List<DbParameter> Parameters)
        {
            StringBuilder sb = new StringBuilder();
            List<string> _vals = new List<string>();
            string[] _cols = (Table.Count > 0) ? Table[0].Columns.Select(en => en.Name).ToArray() : new string[0];

            for (int i = 0; i < Table.Count; i++)
            {
                foreach (MobileTableColumn col in Table[i].Columns)
                {
                    string _prm = string.Format("@{0}_{1}", col.Name, i);

                    _vals.Add(_prm);

                    Parameters.Add(new DbParameter
                    {
                        ParameterName = _prm,
                        DbType = (int)col.Type,
                        Value = col.Value
                    });
                }
                sb.AppendFormat("INSERT INTO {0}({1}) VALUES ({2});", Table.TableName, string.Join(",", _cols), string.Join(",", _vals.ToArray()));
            }
            return sb.ToString();
        }
    }
}
