﻿using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Extensions;
using ExpressBase.Mobile.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ExpressBase.Mobile.Helpers
{
    public class HelperFunctions
    {
        public static string WrapSelectQuery(string sql, List<DbParameter> Parameters = null)
        {
            string query = string.Empty;
            sql = sql.TrimEnd(';');
            try
            {
                List<string> sqlParam = GetSqlParams(sql);

                query = string.Format("SELECT COUNT(*) AS count FROM ({0}); SELECT * FROM ({0}) AS WRAPER", sql);

                if (!Parameters.IsNullOrEmpty())
                {
                    List<string> conditions = new List<string>();
                    foreach (DbParameter param in Parameters)
                    {
                        if (!sqlParam.Contains(param.ParameterName))
                            conditions.Add($"WRAPER.{param.ParameterName} = @{param.ParameterName}");
                    }

                    if (conditions.Count > 0)
                    {
                        query += " WHERE ";
                        query += string.Join(" AND ", conditions.ToArray());
                    }
                }
                query += " LIMIT @limit OFFSET @offset;";
            }
            catch (Exception ex)
            {
                EbLog.Error(ex.Message);
            }
            return query;
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
            if (Application.Current.Resources.TryGetValue(keyName, out var retVal)) { }
            return retVal;
        }

        public static void SetResourceValue(string keyName, object value)
        {
            Application.Current.Resources[keyName] = value;
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

        public static async Task CreateDirectory(string FolderName = null)
        {
            //var read = await AppPermission.ReadStorage();
            //var write = await AppPermission.WriteStorage();

            //if (read && write)
            //{
            CreatePlatFormDir(FolderName);
            //}
        }

        private static string CreatePlatFormDir(string FolderName)
        {
            string sid = App.Settings.Sid.ToUpper();

            try
            {
                INativeHelper helper = DependencyService.Get<INativeHelper>();

                if (!helper.Exist($"{sid}", SysContentType.Directory))
                {
                    helper.Create($"{sid}", SysContentType.Directory);

                    if (FolderName != null)
                        helper.Create($"{sid}/{FolderName}", SysContentType.Directory);
                }
                else
                {
                    if (FolderName != null)
                        helper.Create($"{sid}/{FolderName}", SysContentType.Directory);
                }

                if (!helper.Exist($"{AppConst.SHARED_MEDIA}", SysContentType.Directory))
                {
                    helper.Create($"{AppConst.SHARED_MEDIA}", SysContentType.Directory);
                    helper.Create($"{AppConst.SHARED_MEDIA}/{sid}", SysContentType.Directory);
                }
                else if (!helper.Exist($"{AppConst.SHARED_MEDIA}/{sid}", SysContentType.Directory))
                {
                    helper.Create($"{AppConst.SHARED_MEDIA}/{sid}", SysContentType.Directory);
                }
                else
                {
                    helper.Delete($"{AppConst.SHARED_MEDIA}/{sid}", SysContentType.Directory);
                    helper.Create($"{AppConst.SHARED_MEDIA}/{sid}", SysContentType.Directory);
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

        public static List<FileWrapper> GetFilesByPattern(string Patten, string ControlName = null)
        {
            List<FileWrapper> Files = new List<FileWrapper>();
            try
            {
                INativeHelper helper = DependencyService.Get<INativeHelper>();

                string sid = App.Settings.Sid.ToUpper();

                string[] filenames = helper.GetFiles($"{sid}/FILES", Patten);

                foreach (string filepath in filenames)
                {
                    string filename = Path.GetFileName(filepath);

                    var bytes = helper.GetFile($"{sid}/FILES/{filename}");

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

        public static void WriteFilesLocal(string filename, byte[] fileBytea)
        {
            try
            {
                INativeHelper helper = DependencyService.Get<INativeHelper>();

                string path = $"{App.Settings.Sid.ToUpper()}/FILES/{filename}";

                if (!helper.Exist(path, SysContentType.File))
                {
                    File.WriteAllBytes(helper.NativeRoot + $"/{path}", fileBytea);
                }
            }
            catch (Exception ex)
            {
                EbLog.Error(ex.Message);
                EbLog.Error(ex.StackTrace);
            }
        }
    }
}
