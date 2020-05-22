using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace ExpressBase.Mobile.Extensions
{
    public static class StringExtension
    {
        public static string ToMD5(this string str)
        {
            var md5 = MD5.Create();

            byte[] result = md5.ComputeHash(ASCIIEncoding.ASCII.GetBytes(str));

            StringBuilder strBuilder = new StringBuilder();
            for (int i = 0; i < result.Length; i++)
            {
                strBuilder.Append(result[i].ToString("x2"));
            }
            return strBuilder.ToString();
        }
    }
}
