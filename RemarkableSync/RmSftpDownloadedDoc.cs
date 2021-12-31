using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Renci.SshNet;

namespace RemarkableSync
{
    public class RmSftpDownloadedDoc: RmDownloadedDoc
    {
        public RmSftpDownloadedDoc(string basePath, string id, List<string> pageIDs, SftpClient client) : base (id)
        {
            try
            {
                _pageCount = pageIDs.Count;

                //create the temp content folder
                Directory.CreateDirectory(GetPageContentFolderPath());

                string sourceContentFolder = $"{basePath}/{id}/";

                for (int i = 0; i < _pageCount; ++i)
                {
                    var file = File.OpenWrite(GetPageContentFilePath(i));
                    client.DownloadFile(sourceContentFolder + pageIDs[i]+ ".rm", file);
                    file.Flush();
                    file.Close();
                    var metadataFile = File.OpenWrite(GetPageMetadataFilePath(i));
                    client.DownloadFile(sourceContentFolder + pageIDs[i] + "-metadata.json", metadataFile);
                    metadataFile.Flush();
                    metadataFile.Close();
                }
            }
            catch (Exception err)
            {
                Console.WriteLine($"RmDownloadedDoc::RmDownloadedDoc() - failed to download content to temp folder. Error: {err.Message}");
                throw err;
            }
        }
    }
}
