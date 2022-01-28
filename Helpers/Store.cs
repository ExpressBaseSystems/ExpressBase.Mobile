using ExpressBase.Mobile.Constants;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
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
                if (temp != null || temp != "null")
                    return temp;
            }
            catch (Exception ex)
            {
                EbLog.Error("Store.GetValue::" + ex.Message);
            }
            return null;
        }

        public static T GetValue<T>(string key)
        {
            try
            {
                string temp = SecureStorage.GetAsync(key).Result;

                if (temp != null || temp != "null")
                {
                    var converter = TypeDescriptor.GetConverter(typeof(T));
                    if (converter != null)
                    {
                        return (T)converter.ConvertFromString(temp);
                    }
                }
            }
            catch (Exception ex)
            {
                EbLog.Error("Store.GetValue::" + ex.Message);
                return default;
            }
            return default;
        }

        public static async Task<string> GetValueAsync(string key)
        {
            try
            {
                string temp = await SecureStorage.GetAsync(key);
                if (temp != null || temp != "null")
                    return temp;
            }
            catch (Exception ex)
            {
                EbLog.Error("Store.GetValueAsync::" + ex.Message);
            }
            return null;
        }

        public static async Task<T> GetValueAsync<T>(string key)
        {
            try
            {
                string temp = await SecureStorage.GetAsync(key);

                if (temp != null || temp != "null")
                {
                    var converter = TypeDescriptor.GetConverter(typeof(T));
                    if (converter != null)
                    {
                        return (T)converter.ConvertFromString(temp);
                    }
                }
            }
            catch (Exception ex)
            {
                EbLog.Error("Store.GetValue::" + ex.Message);
            }
            return default;
        }

        public static bool TryGetValue<T>(string key, out T value)
        {
            value = default;
            try
            {
                string temp = SecureStorage.GetAsync(key).Result;

                if (temp != null || temp != "null")
                {
                    var converter = TypeDescriptor.GetConverter(typeof(T));
                    if (converter != null)
                    {
                        value = (T)converter.ConvertFromString(temp);
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                EbLog.Error("Store.GetValue::" + ex.Message);
                return false;
            }
            return false;
        }

        public static void SetValue(string key, string val)
        {
            try
            {
                SecureStorage.SetAsync(key, val);
            }
            catch (Exception ex)
            {
                EbLog.Error("Store.SetValue::" + ex.Message);
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
                EbLog.Error("Store.SetValueAsync::" + ex.Message);
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
                EbLog.Error("Store.Remove::" + ex.Message);
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
                EbLog.Error("Store.RemoveAll::" + ex.Message);
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
                EbLog.Error("Store.SetJSON::" + ex.Message);
            }
        }

        public static async Task SetJSONAsync(string key, object value)
        {
            try
            {
                Application.Current.Properties[key] = JsonConvert.SerializeObject(value);
                await Application.Current.SavePropertiesAsync();
            }
            catch (Exception ex)
            {
                EbLog.Error("Store.SetJSON::" + ex.Message);
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
                EbLog.Error("Store.GetJSON::" + ex.Message);
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
                EbLog.Error("Store.RemoveJSON::" + ex.Message);
            }
        }

        public static void ResetCashedSolutionData()
        {
            try
            {
                //remove auth info
                Store.RemoveJSON(AppConst.USER_OBJECT);
                Store.Remove(AppConst.BTOKEN);
                Store.Remove(AppConst.RTOKEN);
                App.Settings.CurrentUser = null;

                //remove application info
                Store.RemoveJSON(AppConst.APP_COLLECTION);
                Store.RemoveJSON(AppConst.CURRENT_APP);
                Store.RemoveJSON(AppConst.IMAGES_IN_PDF);
                App.Settings.CurrentApplication = null;

                //remove objects info
                Store.RemoveJSON(AppConst.WEBOBJ_COLLECTION);

                //location info
                Store.RemoveJSON(AppConst.USER_LOCATIONS);
                Store.RemoveJSON(AppConst.CURRENT_LOCOBJ);
                App.Settings.CurrentLocation = null;
            }
            catch (Exception ex)
            {
                EbLog.Error("Store.RemoveJSON::" + ex.Message);
            }
        }
    }
}
