using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Extensions;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Structures;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Xamarin.Forms;

namespace ExpressBase.Mobile.Helpers
{
    public class HelperFunctions
    {
        public static EbMobilePage GetPage(string Refid)
        {
            EbMobilePage page = null;
            try
            {
                var wraper = Store.GetJSON<List<MobilePagesWraper>>(AppConst.OBJ_COLLECTION);
                MobilePagesWraper wrpr = wraper?.Find(item => item.RefId == Refid);
                page = wrpr?.ToPage();
            }
            catch(Exception ex)
            {
                Log.Write("HelperFunctions.GetPage---" + ex.Message);
            }
            return page;
        }

        public static string WrapSelectQuery(string sql, List<DbParameter> Parameters = null)
        {
            sql = string.Format("SELECT COUNT(*) AS row_count FROM ({0}); SELECT * FROM ({0}) AS WRAPER", sql.TrimEnd(';'));

            if (!Parameters.IsNullOrEmpty())
            {
                List<string> conditions = new List<string>();
                sql += " WHERE ";
                foreach (DbParameter param in Parameters)
                    conditions.Add($"WRAPER.{param.ParameterName} = @{param.ParameterName}");

                sql += string.Join(" AND ", conditions.ToArray());
            }

            return sql + " LIMIT @limit OFFSET @offset;";
        }

        public static string WrapSelectQueryUnPaged(string sql, List<DbParameter> Parameters = null)
        {
            sql = string.Format("SELECT * FROM ({0}) AS WRAPER", sql.TrimEnd(';'));

            if (!Parameters.IsNullOrEmpty())
            {
                List<string> conditions = new List<string>();
                sql += " WHERE ";
                foreach (DbParameter param in Parameters)
                    conditions.Add($"WRAPER.{param.ParameterName} = @{param.ParameterName}");

                sql += string.Join(" AND ", conditions.ToArray());
            }

            return sql + ";";
        }

        public static object GetResourceValue(string keyName)
        {
            // Search all dictionaries
            if (Application.Current.Resources.TryGetValue(keyName, out var retVal)) { }
            return retVal;
        }

        public static List<string> GetSqlParams(string sql)
        {
            List<string> Params = new List<string>();
            sql = Regex.Replace(sql, @"'([^']*)'", string.Empty);
            Regex r = new Regex(@"((?<=:(?<!::))\w+|(?<=@(?<!::))\w+)");

            foreach (Match match in r.Matches(sql))
            {
                if (!Params.Contains(match.Value))
                    Params.Add(match.Value);
            }
            return Params;
        }

        public static string CreatePlatFormDir(string FolderName = null)
        {
            string sid = Settings.SolutionId.ToUpper();
            try
            {
                INativeHelper helper = DependencyService.Get<INativeHelper>();

                if (helper.DirectoryOrFileExist("ExpressBase", SysContentType.Directory))
                {
                    if (!helper.DirectoryOrFileExist($"ExpressBase/{sid}", SysContentType.Directory))
                    {
                        helper.CreateDirectoryOrFile($"ExpressBase/{sid}", SysContentType.Directory);

                        if (FolderName != null)
                            return helper.CreateDirectoryOrFile($"ExpressBase/{sid}/{FolderName}", SysContentType.Directory);
                    }
                    else
                    {
                        if (FolderName != null)
                            return helper.CreateDirectoryOrFile($"ExpressBase/{sid}/{FolderName}", SysContentType.Directory);
                    }
                }
                else
                {
                    helper.CreateDirectoryOrFile("ExpressBase", SysContentType.Directory);
                    helper.CreateDirectoryOrFile($"ExpressBase/{sid}", SysContentType.Directory);

                    if (FolderName != null)
                        return helper.CreateDirectoryOrFile($"ExpressBase/{sid}/{FolderName}", SysContentType.Directory);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return null;
        }

        public static byte[] StreamToBytea(Stream input)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                input.CopyTo(ms);
                return ms.ToArray();
            }
        }

        public static string GetQuery(MobileTable Rows, List<DbParameter> Parameters)
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

        public static List<FileWrapper> GetFilesByPattern(string Patten, string ControlName = null)
        {
            List<FileWrapper> Files = new List<FileWrapper>();
            try
            {
                INativeHelper helper = DependencyService.Get<INativeHelper>();
                string sid = Settings.SolutionId.ToUpper();

                string[] filenames = helper.GetFiles($"ExpressBase/{sid}/FILES", Patten);

                foreach (string filepath in filenames)
                {
                    string filename = Path.GetFileName(filepath);

                    var bytes = helper.GetPhoto($"ExpressBase/{sid}/FILES/{filename}");

                    Files.Add(new FileWrapper
                    {
                        Name = Path.GetFileNameWithoutExtension(filename),
                        Bytea = bytes,
                        FileName = filename,
                        ControlName = ControlName
                    });
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return Files;
        }

        public static string B64ToString(string b64)
        {
            byte[] b = Convert.FromBase64String(b64);
            return System.Text.Encoding.UTF8.GetString(b);
        }
    }
}
