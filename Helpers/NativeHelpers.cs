using ExpressBase.Mobile.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ExpressBase.Mobile.Helpers
{
    public interface INativeHelper
    {
        string DeviceId { get; }

        string AppVersion { get; }

        void Close();

        string NativeRoot { get; }

        bool Exist(string Path, SysContentType Type);

        string Create(string DirectoryPath, SysContentType Type);

        byte[] GetFile(string url);

        string[] GetFiles(string Url, string Pattern);

        string GetAssetsURl();

        void WriteLogs(string message, LogTypes logType);
    }

    public interface IToast
    {
        void Show(string message);
    }

    public class EbLog
    {
        private static INativeHelper _helper;

        public static void Error(string message)
        {
            try
            {
                _helper ??= DependencyService.Get<INativeHelper>();

                _helper.WriteLogs(message, LogTypes.EXCEPTION);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public static void Message(string message)
        {
            try
            {
                _helper ??= DependencyService.Get<INativeHelper>();

                _helper.WriteLogs(message, LogTypes.MESSAGE);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public static void StackTrace(string message)
        {
            try
            {
                _helper ??= DependencyService.Get<INativeHelper>();

                _helper.WriteLogs(message, LogTypes.STACKTRACE);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}