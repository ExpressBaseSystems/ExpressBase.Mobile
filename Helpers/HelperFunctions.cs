using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Xamarin.Forms;

namespace ExpressBase.Mobile.Helpers
{
    public class HelperFunctions
    {
        public static EbMobilePage GetPage(string Refid)
        {
            string _objlist = Store.GetValue(AppConst.OBJ_COLLECTION);
            List<MobilePagesWraper> _list = JsonConvert.DeserializeObject<List<MobilePagesWraper>>(_objlist);
            MobilePagesWraper Wrpr = _list.Find(item => item.RefId == Refid);

            string regexed = EbSerializers.JsonToNETSTD(Wrpr.Json);
            return EbSerializers.Json_Deserialize<EbMobilePage>(regexed);
        }

        public static string WrapSelectQuery(string sql)
        {
            return string.Format("SELECT * FROM ({0}) AS WRAPER LIMIT 100;", sql.TrimEnd(';'));
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
                {
                    Params.Add(match.Value);
                }
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

                        if(FolderName != null)
                        {
                            return helper.CreateDirectoryOrFile($"ExpressBase/{sid}/{FolderName}", SysContentType.Directory);
                        }
                    }
                    else
                    {
                        if (FolderName != null)
                        {
                            return helper.CreateDirectoryOrFile($"ExpressBase/{sid}/{FolderName}", SysContentType.Directory);
                        }
                    }
                }
                else
                {
                    helper.CreateDirectoryOrFile("ExpressBase", SysContentType.Directory);
                    helper.CreateDirectoryOrFile($"ExpressBase/{sid}", SysContentType.Directory);

                    if(FolderName != null)
                    {
                        return helper.CreateDirectoryOrFile($"ExpressBase/{sid}/{FolderName}", SysContentType.Directory);
                    }
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
    }
}
