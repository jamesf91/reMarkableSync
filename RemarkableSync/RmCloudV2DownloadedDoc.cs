using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace RemarkableSync
{
    class RmCloudV2DownloadedDoc : RmDownloadedDoc
    {
        private V2HttpHelper _httpHelper;
        private object _taskProgress;

        public RmCloudV2DownloadedDoc(string id, List<DocFile> fileList, HttpClient client , CancellationToken cancellationToken, IProgress<string> progress) : base(id)
        {
            Logger.LogMessage($"Creating downloaded doc for id: {id}");
            _httpHelper = new V2HttpHelper(client);
            _taskProgress = 0;

            try
            {
                Dictionary<string, string> pageNameReplacements = new Dictionary<string, string>();

                // find and download <id>.content file first to get page mapping
                var contentDocFile = fileList.FirstOrDefault(docfile => docfile.DocumentID == (id + ".content"));
                if (contentDocFile != null)
                {
                    progress.Report($"Getting page info for document...");

                    fileList.Remove(contentDocFile);

                    var result = DownloadFile(contentDocFile, 0, pageNameReplacements, progress).Result;
                    if (result)
                    {
                        string filepath = Path.Combine(_folderPath, contentDocFile.DocumentID);
                        Logger.LogMessage($"Reading content file {filepath} for page mapping");
                        string contentJsonString = File.ReadAllText(filepath);
                        ContentJson content = JsonSerializer.Deserialize<ContentJson>(contentJsonString);
                        var pageMapping = content.pages.ToList();
                        for (var i = 0; i < pageMapping.Count; ++i)
                        {
                            if (pageMapping[i].Length > 0)
                            {
                                pageNameReplacements.Add(pageMapping[i], i.ToString());
                            }
                        }
                    }
                }

                lock(_taskProgress)
                {
                    _taskProgress = 0;
                }

                ParallelOptions po = new ParallelOptions
                {
                    CancellationToken = cancellationToken
                };
                Parallel.ForEach(fileList, po,
                    file =>
                    {
                        var result = DownloadFile(file, fileList.Count, pageNameReplacements, progress).Result;
                    });
            }
            catch (Exception err)
            {
                Logger.LogMessage($"failed to download document content for id: {id}. Error: {err.Message}");
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

            Logger.LogMessage($"Finished creating downloaded doc for id: {id}");
        }

        private async Task<bool> DownloadFile(DocFile file, int totalFileCount, Dictionary<string, string> pageNameReplacements, IProgress<string> progress)
        {
            string dir = _folderPath;
            string filename = file.DocumentID;
            Logger.LogMessage($"Downloading file: {file.DocumentID}, hash: {file.Hash} - Start");

            if (filename.Contains("/"))
            {
                // change to windows path notation
                filename = filename.Replace('/', '\\');
                int pos = filename.LastIndexOf('\\');
                if (pos != -1)
                {
                    dir = Path.Combine(dir, filename.Substring(0, pos));
                    filename = filename.Substring(pos + 1);
                }
            }

            // check filename replacement
            lock(pageNameReplacements)
            {
                foreach(var pair in pageNameReplacements)
                {
                    if (filename.Contains(pair.Key))
                    {
                        filename = filename.Replace(pair.Key, pair.Value);
                        break;
                    }
                }
            }

            Directory.CreateDirectory(dir);
            string fullPathFilename = Path.Combine(dir, filename);

            Stream stream = await _httpHelper.GetStreamFromHashAsync(file.Hash);
            if (stream == null)
            {
                Logger.LogMessage($"Downloading file: {file.DocumentID}, hash: {file.Hash} - returned null stream");
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
            Logger.LogMessage($"Downloading file: {file.DocumentID}, hash: {file.Hash} - Finished");
            return true;
        }


        class ContentJson
        {
            public int coverPageNumber { get; set; }
            public Documentmetadata documentMetadata { get; set; }
            public bool dummyDocument { get; set; }
            public Extrametadata extraMetadata { get; set; }
            public string fileType { get; set; }
            public string fontName { get; set; }
            public int formatVersion { get; set; }
            public int lineHeight { get; set; }
            public int margins { get; set; }
            public string orientation { get; set; }
            public int originalPageCount { get; set; }
            public int pageCount { get; set; }
            public object[] pageTags { get; set; }
            public string[] pages { get; set; }
            public string sizeInBytes { get; set; }
            public object[] tags { get; set; }
            public string textAlignment { get; set; }
            public int textScale { get; set; }
        }

        class Documentmetadata
        {
        }

        class Extrametadata
        {
            public string LastBallpointColor { get; set; }
            public string LastBallpointSize { get; set; }
            public string LastBallpointv2Color { get; set; }
            public string LastBallpointv2Size { get; set; }
            public string LastCalligraphyColor { get; set; }
            public string LastCalligraphySize { get; set; }
            public string LastClearPageColor { get; set; }
            public string LastClearPageSize { get; set; }
            public string LastEraseSectionColor { get; set; }
            public string LastEraseSectionSize { get; set; }
            public string LastEraserColor { get; set; }
            public string LastEraserSize { get; set; }
            public string LastEraserTool { get; set; }
            public string LastFinelinerColor { get; set; }
            public string LastFinelinerSize { get; set; }
            public string LastFinelinerv2Color { get; set; }
            public string LastFinelinerv2Size { get; set; }
            public string LastHighlighterColor { get; set; }
            public string LastHighlighterSize { get; set; }
            public string LastHighlighterv2Color { get; set; }
            public string LastHighlighterv2Size { get; set; }
            public string LastMarkerColor { get; set; }
            public string LastMarkerSize { get; set; }
            public string LastMarkerv2Color { get; set; }
            public string LastMarkerv2Size { get; set; }
            public string LastPaintbrushColor { get; set; }
            public string LastPaintbrushSize { get; set; }
            public string LastPaintbrushv2Color { get; set; }
            public string LastPaintbrushv2Size { get; set; }
            public string LastPen { get; set; }
            public string LastPencilColor { get; set; }
            public string LastPencilSize { get; set; }
            public string LastPencilv2Color { get; set; }
            public string LastPencilv2Size { get; set; }
            public string LastReservedPenColor { get; set; }
            public string LastReservedPenSize { get; set; }
            public string LastSelectionToolColor { get; set; }
            public string LastSelectionToolSize { get; set; }
            public string LastSharpPencilColor { get; set; }
            public string LastSharpPencilSize { get; set; }
            public string LastSharpPencilv2Color { get; set; }
            public string LastSharpPencilv2Size { get; set; }
            public string LastSolidPenColor { get; set; }
            public string LastSolidPenSize { get; set; }
            public string LastTool { get; set; }
            public string LastZoomToolColor { get; set; }
            public string LastZoomToolSize { get; set; }
        }
    }



}