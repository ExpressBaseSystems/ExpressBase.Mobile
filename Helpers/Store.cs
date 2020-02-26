using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace ExpressBase.Mobile.Helpers
{
    public class Store
    {
        public static string GetValue(string key)
        {
            try
            {
                string temp = SecureStorage.GetAsync(key).Result;
                if (temp == null || temp == "null")
                    return null;
                else
                    return temp;
            }
            catch (Exception ex)
            {
                Log.Write("Store.GetValue::" + ex.Message);
                return null;
            }
        }

        public static async Task<string> GetValueAsync(string key)
        {
            try
            {
                string temp = await SecureStorage.GetAsync(key);
                if (temp == null || temp == "null")
                    return null;
                else
                    return temp;
            }
            catch (Exception ex)
            {
                Log.Write("Store.GetValueAsync::" + ex.Message);
                return null;
            }
        }

        public static void SetValue(string key, string val)
        {
            try
            {
                SecureStorage.SetAsync(key, val);
            }
            catch (Exception ex)
            {
                Log.Write("Store.SetValue::" + ex.Message);
            }
        }

        public static async Task SetValueAsync(string key, string val)
        {
            try
            {
                await SecureStorage.SetAsync(key, val);
            }
            catch (Exception ex)
            {
                Log.Write("Store.SetValueAsync::" + ex.Message);
            }
        }

        public static void Remove(string key)
        {
            try
            {
                SecureStorage.Remove(key);
            }
            catch (Exception ex)
            {
                Log.Write("Store.Remove::" + ex.Message);
            }
        }

        public static void RemoveAll()
        {
            try
            {
                SecureStorage.RemoveAll();
            }
            catch (Exception ex)
            {
                Log.Write("Store.RemoveAll::" + ex.Message);
            }
        }

        public static void SetJSON(string key, object value)
        {
            try
            {
                Application.Current.Properties[key] = JsonConvert.SerializeObject(value);
                Application.Current.SavePropertiesAsync();
            }
            catch (Exception ex)
            {
                Log.Write("Store.SetJSON::" + ex.Message);
            }
        }

        public static T GetJSON<T>(string key)
        {
            try
            {
                if (Application.Current.Properties.TryGetValue(key, out var value))
                    return JsonConvert.DeserializeObject<T>(value.ToString());
            }
            catch (Exception ex)
            {
                Log.Write("Store.GetJSON::" + ex.Message);
            }
            return default;
        }

        public static void RemoveJSON(string key)
        {
            try
            {
                Application.Current.Properties.Remove(key);
            }
            catch (Exception ex)
            {
                Log.Write("Store.RemoveJSON::" + ex.Message);
            }
        }
    }
}
