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

        void CloseApp();

        string NativeRoot { get; }

        bool DirectoryOrFileExist(string Path, SysContentType Type);

        string CreateDirectoryOrFile(string DirectoryPath, SysContentType Type);

        byte[] GetPhoto(string url);

        string[] GetFiles(string Url, string Pattern);

        string GetBaseURl();

        void WriteLogs(string message, LogTypes logType);
    }

    public interface IToast
    {
        void Show(string message);
    }


    public class EbLog
    {
        public static void Write(string message, LogTypes logType = LogTypes.EXCEPTION)
        {
            try
            {
                DependencyService.Get<INativeHelper>().WriteLogs(message, logType);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}