using ExpressBase.Mobile.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ExpressBase.Mobile.Models
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
    }

    public interface IToast
    {
        void Show(string message);
    }
}