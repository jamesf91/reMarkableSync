using RemarkableSync.RmLine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RemarkableSync
{
    public class RmDownloadedDoc: IDisposable
    {
        protected string _folderPath;
        protected string _id;
        protected int _pageCount;

        internal class RmPageMetadata
        {
            public List<RmPageMetadataLayer> layers { get; set; }
        }
        internal class RmPageMetadataLayer
        {
            public string name { get; set; }
        }

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
                List<string> layerNames = GetPageLayerNames(pageNumber);

                using (FileStream contentFileStream = File.OpenRead(GetPageContentFilePath(pageNumber)))
                using (MemoryStream contentStream = new MemoryStream())
                {
                    contentFileStream.CopyTo(contentStream);
                    RmPage page = RmPage.ParseStream(contentStream, layerNames);
                    return page;
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

        protected string GetPageMetadataFilePath(int pageNumber)
        {
            return Path.Combine(_folderPath, _id, $"{pageNumber}-metadata.json");
        }

        protected string GetPageContentFolderPath()
        {
            return Path.Combine(_folderPath, _id);
        }

        protected List<string> GetPageLayerNames(int pageNumber)
        {
            var layerNames = new List<string>();
            var metadataJsonString = File.ReadAllText(GetPageMetadataFilePath(pageNumber));
            try
            {
                RmPageMetadata metadata = JsonSerializer.Deserialize<RmPageMetadata>(metadataJsonString);
                layerNames = (from layer in metadata.layers select layer.name).ToList();
            }
            catch (Exception err)
            {
                Console.WriteLine($"RmDownloadedDoc::GetPageLayerNames() - RmPageMetadata json deseralizing failed with: {err.Message}.\n Content:\n{metadataJsonString}");
            }
            return layerNames;
        }
    }
}
