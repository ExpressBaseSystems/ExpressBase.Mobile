using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace ExpressBase.Mobile.Services
{
    public class Store{

        public static string GetValue(string key)
        {
            try
            {
                return SecureStorage.GetAsync(key).Result;
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        public static void SetValue(string key,string val)
        {
            try
            {
                SecureStorage.SetAsync(key,val);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public static void Remove(string key)
        {
            try
            {
                SecureStorage.Remove(key);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public static void RemoveAll()
        {
            try
            {
                SecureStorage.RemoveAll();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
