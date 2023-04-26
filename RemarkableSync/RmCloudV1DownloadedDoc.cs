using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace RemarkableSync
{
    public class RmCloudV1DownloadedDoc: RmDocument
    {

        public RmCloudV1DownloadedDoc(ZipArchive archive, string id) : base(id)
        {
            try
            {
                archive.ExtractToDirectory(_root_path);
            }
            catch (Exception err)
            {
                Logger.Error($"failed to extract archive. Error: {err.Message}");
                _root_path = "";
                throw err;
            }

            try
            {
                string pageFolder = Path.Combine(_root_path, id);
                var rmFiles = new List<string>(Directory.EnumerateFiles(pageFolder, "*.rm", SearchOption.TopDirectoryOnly));
                _content.pageCount = rmFiles.Count;
            }
            catch (Exception err)
            {
                Logger.Error($"failed to get page count. Error: {err.Message}");
                _content.pageCount = 0;
            }

            LoadDocumentContent();
        }
    }
}
