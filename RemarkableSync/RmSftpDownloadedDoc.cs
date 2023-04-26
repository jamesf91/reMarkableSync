using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Renci.SshNet;

namespace RemarkableSync
{
    public class RmSftpDownloadedDoc: RmDocument
    {
        public RmSftpDownloadedDoc(string basePath, string id, SftpClient client, String docContentJsonString) : base (id)
        {
            try
            {
                LoadDocumentContent(docContentJsonString);

                //create the temp content folder
                Directory.CreateDirectory(GetDocumentFolderPath());

                string sourceContentFolder = $"{basePath}/{id}/";

                for (int i = 0; i < _content.pageCount; i++)
                {
                    string pageUuid = GetPageUUID(i);
                    if(client.Exists(sourceContentFolder + pageUuid + ".rm"))
                    {
                        var file = File.OpenWrite(GetPageBinaryFilePath(i));
                        client.DownloadFile(sourceContentFolder + pageUuid + ".rm", file);
                        file.Flush();
                        file.Close();
                    }
                    else
                    {
                        Logger.Debug($"Page binary {sourceContentFolder}{pageUuid}.rm not found, probably a blank page");
                    }
                    if (client.Exists(sourceContentFolder + pageUuid + "-metadata.json"))
                    {
                        var metadataFile = File.OpenWrite(GetPageMetadataFilePath(i));
                        client.DownloadFile(sourceContentFolder + pageUuid + "-metadata.json", metadataFile);
                        metadataFile.Flush();
                        metadataFile.Close();
                    }
                    else
                    {
                        Logger.Debug($"Page metadata {sourceContentFolder}{pageUuid}-metadata.json not found");
                    }
                }
            }
            catch (Exception err)
            {
                Logger.Error($"failed to download content to temp folder. Error: {err.Message}");
                throw err;
            }
        }
    }
}
