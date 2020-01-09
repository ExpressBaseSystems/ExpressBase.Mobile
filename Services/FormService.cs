using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.CustomControls;
using ExpressBase.Mobile.Models;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;
using System.Linq;
using ExpressBase.Mobile.Structures;
using ExpressBase.Mobile.Enums;
using System.Threading.Tasks;

namespace ExpressBase.Mobile.Services
{
    public class FormService
    {
        private FormMode Mode { set; get; }

        protected IList<XCustomControl> XControls { set; get; }

        public EbMobileForm Form { set; get; }

        private EbDataRow ParentRow { set; get; }

        private string ParentTable { set; get; }

        private bool HasFileSelect { set; get; } = false;

        public FormService() { }

        public FormService(IList<XCustomControl> XControls, EbMobileForm form, FormMode Mode)
        {
            this.Mode = Mode;
            this.XControls = XControls;
            this.Form = form;
        }

        public bool Save(int RowId)
        {
            MobileFormData data = this.GetFormData(RowId);
            string query = string.Empty;
            try
            {
                if (data.Tables.Count > 0)
                {
                    List<DbParameter> _params = new List<DbParameter>();
                    foreach (MobileTable _table in data.Tables)
                    {
                        query += this.GetQuery(_table, _params);
                    }
                    int rowAffected = App.DataDB.DoNonQuery(query, _params.ToArray());

                    if (this.HasFileSelect)
                    {
                        object lastRowId = (RowId != 0) ? RowId : App.DataDB.DoScalar(string.Format(StaticQueries.CURRVAL, this.Form.TableName));
                        this.PushFiles(lastRowId);
                    }

                    return (rowAffected > 0);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return false;
        }

        public bool Save(EbDataRow _ParentRow, string ParentTableName)
        {
            ParentRow = _ParentRow;
            ParentTable = ParentTableName;

            MobileFormData data = this.GetFormData(0);
            string query = string.Empty;
            try
            {
                if (data.Tables.Count > 0)
                {
                    List<DbParameter> _params = new List<DbParameter>();
                    foreach (MobileTable _table in data.Tables)
                    {
                        query += this.GetQuery(_table, _params);
                    }
                    int rowAffected = App.DataDB.DoNonQuery(query, _params.ToArray());

                    if (this.HasFileSelect)
                    {
                        object lastRowId = App.DataDB.DoScalar(string.Format(StaticQueries.CURRVAL, this.Form.TableName));
                        this.PushFiles(lastRowId);
                    }

                    return (rowAffected > 0);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return false;
        }

        private MobileFormData GetFormData(int RowId)
        {
            MobileFormData FormData = new MobileFormData
            {
                MasterTable = this.Form.TableName
            };
            MobileTable Table = new MobileTable { TableName = this.Form.TableName };
            MobileTableRow row = new MobileTableRow
            {
                RowId = RowId,
                IsUpdate = (RowId > 0)
            };

            Table.Add(row);
            foreach (XCustomControl XCtrl in this.XControls)
            {
                if (XCtrl is XFileSelect)
                {
                    this.HasFileSelect = true;
                    continue;
                }
                else
                {
                    var value = XCtrl.GetValue();
                    if (value == null)
                        continue;

                    row.Columns.Add(new MobileTableColumn
                    {
                        Name = XCtrl.Name,
                        Type = XCtrl.DbType,
                        Value = value
                    });
                }
            }
            if (RowId <= 0)
                row.AppendEbColValues();//append ebcol values

            if (this.Mode == FormMode.REF)
            {
                row.Columns.Add(new MobileTableColumn
                {
                    Name = this.ParentTable + "_id",
                    Type = EbDbTypes.Int32,
                    Value = this.ParentRow["id"]
                });
            }

            FormData.Tables.Add(Table);
            return FormData;
        }

        private string GetQuery(MobileTable Rows, List<DbParameter> Parameters)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < Rows.Count; i++)
            {
                if (Rows[i].IsUpdate)//update
                {
                    List<string> _colstrings = new List<string>();
                    foreach (MobileTableColumn col in Rows[i].Columns)
                    {
                        _colstrings.Add(string.Format("{0} = @{1}_{2}", col.Name, col.Name, i));

                        Parameters.Add(new DbParameter
                        {
                            ParameterName = string.Format("@{0}_{1}", col.Name, i),
                            DbType = (int)col.Type,
                            Value = col.Value
                        });
                    }
                    sb.AppendFormat("UPDATE {0} SET {1} WHERE id = {2};", Rows.TableName, string.Join(",", _colstrings), ("@rowid" + i));

                    Parameters.Add(new DbParameter
                    {
                        ParameterName = ("@rowid" + i),
                        DbType = (int)EbDbTypes.Int32,
                        Value = Rows[i].RowId
                    });
                }
                else//insert
                {
                    string[] _cols = (Rows.Count > 0) ? Rows[i].Columns.Select(en => en.Name).ToArray() : new string[0];
                    List<string> _vals = new List<string>();
                    foreach (MobileTableColumn col in Rows[i].Columns)
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
                    sb.AppendFormat("INSERT INTO {0}({1}) VALUES ({2});", Rows.TableName, string.Join(",", _cols), string.Join(",", _vals.ToArray()));
                }
            }
            return sb.ToString();
        }

        private void PushFiles(object LastRowId)
        {
            int rowid = Convert.ToInt32(LastRowId);

            Task.Run(() =>
            {
                foreach (XCustomControl ctrl in this.XControls)
                {
                    if (ctrl is XFileSelect)
                    {
                        (ctrl as XFileSelect).PushFilesToDir(this.Form.TableName, rowid);
                    }
                }
            });
        }
    }
}
