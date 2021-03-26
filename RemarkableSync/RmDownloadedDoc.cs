using RemarkableSync.RmLine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemarkableSync
{
    public class RmDownloadedDoc: IDisposable
    {
        protected string _folderPath;
        protected string _id;
        protected int _pageCount;


        public RmDownloadedDoc(string id)
        {
            _id = id;
            _folderPath = Path.Combine(Path.GetTempPath(), id);
        }

        public RmPage GetPageContent(int pageNumber)
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
                        RmPage page = RmPage.ParseStream(stream);
                        return page;
                    }
                }
            }
            catch (Exception err)
            {
                Console.WriteLine($"RmDownloadedDoc::GetPageContent() - Parsing page content to Page object failed with err: {err.Message}");
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
                catch (Exception err)
                {
                    Console.WriteLine($"RmDownloadedDoc::Dispose() - failed to remove folder: {_folderPath}. Error: {err.Message}");
                }

            }
        }

        public int PageCount
        {
            get { return _pageCount; }
        }

        protected string GetPageContentFilePath(int pageNumber)
        {
            return Path.Combine(_folderPath, _id, $"{pageNumber}.rm");
        }

        protected string GetPageContentFolderPath()
        {
            return Path.Combine(_folderPath, _id);
        }
    }
}
