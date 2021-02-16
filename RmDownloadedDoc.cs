using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using RemarkableSync.RmLine;

namespace RemarkableSync
{
    class RmDownloadedDoc: IDisposable
    {
        private string _folderPath;
        private string _id;
        private int _pageCount;


        public RmDownloadedDoc(ZipArchive archive, string id)
        {
            _id = id;
            try
            {
                _folderPath = Path.Combine(Path.GetTempPath(), id);
                archive.ExtractToDirectory(_folderPath);
            }
            catch (Exception err)
            {
                Console.WriteLine($"RmDownloadedDoc::RmDownloadedDoc() - failed to extract archive. Error: {err.Message}");
                _folderPath = "";
                return;
            }

            try
            {
                string pageFolder = Path.Combine(_folderPath, id);
                var rmFiles = new List<string>(Directory.EnumerateFiles(pageFolder, "*.rm", SearchOption.TopDirectoryOnly));
                _pageCount = rmFiles.Count;
            }
            catch (Exception err)
            {
                Console.WriteLine($"RmDownloadedDoc::RmDownloadedDoc() - failed to get page count. Error: {err.Message}");
                _pageCount = 0;
            }
        }

        public Page GetPageContent(int pageNumber)
        {
            if (_folderPath == "")
            {
                Console.WriteLine($"RmDownloadedDoc::GetPageContent() - Document content not available.");
                return null;
            }

            if (pageNumber >= _pageCount)
            {
                Console.WriteLine($"RmDownloadedDoc::GetPageContent() - unexpected page number {pageNumber} for pageCount {_pageCount}");
                return null;
            }

            try
            {
                using (FileStream fileStream = File.OpenRead(GetPageContentFilePath(pageNumber)))
                {
                    using (MemoryStream stream = new MemoryStream())
                    {
                        fileStream.CopyTo(stream);
                        Page page = Page.ParseStream(stream);
                        return page;
                    }
                }
            }
            catch(Exception err)
            {
                Console.WriteLine($"RmDownloadedDoc::GetPageContent() - Parsing page content to Page object failedwith err: {err.Message}");
            }

            return null;
        }

        public void Dispose()
        {
            if (_folderPath.Length > 0)
            {
                try
                {
                    Directory.Delete(_folderPath, true);
                }
                catch(Exception err)
                {
                    Console.WriteLine($"RmDownloadedDoc::Dispose() - failed to remove folder: {_folderPath}. Error: {err.Message}");
                }

            }
        }

        public int PageCount
        {
            get { return _pageCount; }
        }

        private string GetPageContentFilePath(int pageNumber)
        {
            return Path.Combine(_folderPath, _id, $"{pageNumber}.rm");
        }
    }
}
