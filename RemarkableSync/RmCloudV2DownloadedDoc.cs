using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace RemarkableSync
{
    class RmCloudV2DownloadedDoc : RmDocument
    {
        private V2HttpHelper _httpHelper;
        private object _taskProgress;

        public RmCloudV2DownloadedDoc(string id, List<DocFile> fileList, HttpClient client, CancellationToken cancellationToken, IProgress<string> progress) : base(id)
        {
            Logger.Debug($"Creating downloaded doc for id: {id}");
            _httpHelper = new V2HttpHelper(client);
            _taskProgress = 0;

            try
            {
                // find and download <id>.content file first to get page mapping
                var contentDocFile = fileList.FirstOrDefault(docfile => docfile.DocumentID == (id + ".content"));
                if (contentDocFile == null)
                {
                    throw new Exception("Content file not found, unable to parse");
                }

                progress.Report($"Getting page info for document...");
                if (!DownloadFile(contentDocFile, 0, progress).Result)
                {
                    throw new Exception("Could not load file contents");

                }

                RmDocument doc = new RmDocument(id, _root_path);

                lock (_taskProgress)
                {
                    _taskProgress = 0;
                }

                ParallelOptions po = new ParallelOptions
                {
                    CancellationToken = cancellationToken
                };

                //todo: only need to download the binary pages?

                Parallel.ForEach(fileList, po,
                    file =>
                    {
                        var result = DownloadFile(file, fileList.Count, progress).Result;
                    });
            }
            catch (Exception err)
            {
                Logger.Error($"failed to download document content for id: {id}. Error: {err.Message}");
                _root_path = "";
                throw err;
            }

            try
            {
                string pageFolder = Path.Combine(_root_path, id);
                var rmFiles = new List<string>(Directory.EnumerateFiles(pageFolder, "*.rm", SearchOption.TopDirectoryOnly));
                Logger.Debug($"Downloaded {rmFiles.Count} of {fileList.Count} files");

            }
            catch (Exception err)
            {
                Logger.Error($"failed to get page count. Error: {err.Message}");

            }

            Logger.Debug($"Finished creating downloaded doc for id: {id}");
            LoadDocumentContent();
        }

        private async Task<bool> DownloadFile(DocFile file, int totalFileCount, IProgress<string> progress)
        {
            string dir = _root_path;
            string filename = file.DocumentID;
            Logger.Debug($"Downloading file: {file.DocumentID}, hash: {file.Hash} - Start");

            if (filename.Contains("/"))
            {
                // change to windows path notation
                filename = filename.Replace('/', '\\');
                dir = Path.Combine(dir, Path.GetDirectoryName(filename));
                filename = Path.GetFileName(filename);
            }

            Directory.CreateDirectory(dir);
            string fullPathFilename = Path.Combine(dir, filename);

            Stream stream = await _httpHelper.GetStreamFromHashAsync(file.Hash);
            if (stream == null)
            {
                Logger.Debug($"Downloading file: {file.DocumentID}, hash: {file.Hash} - returned null stream");
                return false;
            }

            var fileStream = File.Create(fullPathFilename);
            stream.Seek(0, SeekOrigin.Begin);
            stream.CopyTo(fileStream);
            fileStream.Close();

            if (totalFileCount > 0)
            {
                int taskProgress = 0;
                lock (_taskProgress)
                {
                    taskProgress = (int)_taskProgress + 1;
                    _taskProgress = taskProgress;
                }
                progress.Report($"Downloading document content... {taskProgress} out of {totalFileCount} files complete.");
            }
            Logger.Debug($"Downloading file: {file.DocumentID}, hash: {file.Hash} - Finished");
            return true;
        }
    }
}