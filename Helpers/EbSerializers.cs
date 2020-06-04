using ExpressBase.Mobile.Constants;
using Newtonsoft.Json;
using System;

namespace ExpressBase.Mobile.Helpers
{
    public class EbSerializers
    {
        public static string Json_Serialize(object obj)
        {
            return JsonConvert.SerializeObject(obj, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            });
        }

        public static T Json_Deserialize<T>(string json)
        {
            return (T)(JsonConvert.DeserializeObject(json, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                ObjectCreationHandling = ObjectCreationHandling.Replace,
                MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead
            }));
        }

        public static dynamic Json_Deserialize(string json)
        {
            try
            {
                return JsonConvert.DeserializeObject(json,
                    new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.All,
                        ObjectCreationHandling = ObjectCreationHandling.Replace,
                        MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead
                    });
            }
            catch (Exception e)
            {
                Console.WriteLine("============Json_Deserialize Exception : " + e.Message);

                return null;
            }
        }

        public static string JsonToNETSTD(string json_core)
        {
            return json_core
                .Replace(RegexConstants.COR_LIB, RegexConstants.MS_LIB)
                .Replace(RegexConstants.EB_COM_OBJ, RegexConstants.EB_MOB)
                .Replace(RegexConstants.EB_OBJ, RegexConstants.EB_MOB)
                .Replace(RegexConstants.EB_PARAM, RegexConstants.EB_MOB)
                .Replace(RegexConstants.EB_COM, RegexConstants.EB_MOB);
        }
    }
}

