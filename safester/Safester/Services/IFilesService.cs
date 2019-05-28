using System;
using System.IO;

namespace Safester.Services
{
    public interface IFilesService
    {
        void OpenUri(string uri);
        string GetDownloadFolder();
    }
}
