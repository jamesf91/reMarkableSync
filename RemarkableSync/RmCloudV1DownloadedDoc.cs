using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace RemarkableSync
{
    public class RmCloudV1DownloadedDoc: RmDownloadedDoc
    {

        public RmCloudV1DownloadedDoc(ZipArchive archive, string id) : base(id)
        {
            try
            {
                archive.ExtractToDirectory(_folderPath);
            }
            catch (Exception err)
            {
                Logger.LogMessage($"failed to extract archive. Error: {err.Message}");
                _folderPath = "";
                throw err;
            }

            try
            {
                string pageFolder = Path.Combine(_folderPath, id);
                var rmFiles = new List<string>(Directory.EnumerateFiles(pageFolder, "*.rm", SearchOption.TopDirectoryOnly));
                _pageCount = rmFiles.Count;
            }
            catch (Exception err)
            {
                Logger.LogMessage($"failed to get page count. Error: {err.Message}");
                _pageCount = 0;
            }
        }
    }
}
