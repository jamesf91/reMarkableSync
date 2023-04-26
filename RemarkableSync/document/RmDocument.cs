using RemarkableSync.document;
using RemarkableSync.MyScript;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace RemarkableSync
{
    /****
     * Rm document is split in multiple files containing info about the file
     * All starts in the root folder
     * Files have a UUID as filename
     * root\{FILE_UUID}.content
     * root\{FILE_UUID}.metadata
     * root\{FILE_UUID}.pagedata
     * root\{FILE_UUID}.thumbnails\{page_1_UUID}.png
     * root\{FILE_UUID}.thumbnails\{page_2_UUID}.png
     * root\{FILE_UUID}\{page_1_UUID}.rm
     * root\{FILE_UUID}\{page_1_UUID}-metadata.json
     * root\{FILE_UUID}\{page_2_UUID}.rm
     * root\{FILE_UUID}\{page_2_UUID}-metadata.json
     * 
     * root\{FILE_UUID}.textconversion\{page_1_UUID}.json
     * root\{FILE_UUID}.highlights\
     * */
    public class RmDocument : IDisposable
    {
        protected string _root_path;
        protected string _id;
        protected DocumentContent _content;
        private Dictionary<int,PageBinary> _pages = new Dictionary<int, PageBinary>();

        protected static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        internal class RmPageMetadata
        {
            public List<RmPageMetadataLayer> layers { get; set; }
        }
        internal class RmPageMetadataLayer
        {
            public string name { get; set; }
        }

        public RmDocument(string id) : this(id, Path.Combine(Path.GetTempPath(), id)) { }

        public RmDocument(string id, string root_path)
        {
            _id = id;
            _root_path = root_path;
        }

        protected void LoadDocumentContent(string docContentJsonString = null)
        {
            try
            {
                if (docContentJsonString == null)
                {
                    string filepath = GetDocumentContentFilePath();
                    Logger.Debug($"Reading content file {filepath} for page mapping");
                    if (!File.Exists(filepath))
                    {
                        throw new FileNotFoundException("DocumentContent file not found", filepath);
                    }
                    docContentJsonString = File.ReadAllText(filepath);
                }
                _content = DocumentContent.GetDocumentContentFromJson(docContentJsonString);
            }
            catch (Exception)
            {
                throw new Exception($"Unsupported format");
            }
        }

        public Bitmap GetPageAsImage(int pageNumber)
        {
            return GetPageAsBinary(pageNumber).GetBitmap();
        }

        public List<Bitmap> GetPagesAsImage()
        {
            List<Bitmap> images = new List<Bitmap>();
            for (int i = 0; i < PageCount; i++)
            {
                images.Add(GetPageAsImage(i));
            }

            return images;
        }

        public HwrRequestBundle GetPageAsMyScriptHwrRequestBundle(int pageNumber, string language)
        {
            HwrRequest request = new HwrRequest();

            request.xDPI = request.yDPI = 226;
            request.contentType = "Text";
            request.configuration = new Configuration() { lang = language };
            request.strokeGroups = new StrokeGroup[1];

            var strokeGroupBundle = GetPageAsBinary(pageNumber).GetMyScriptFormat();
            request.strokeGroups[0] = strokeGroupBundle.Item1;

            return new HwrRequestBundle()
            {
                Request = request,
                Bounds = new List<BoundingBox>() { strokeGroupBundle.Item2 }
            };
        }

        private PageBinary GetPageAsBinary(int pageNumber)
        {
            if(_pages.ContainsKey(pageNumber)) { return _pages[pageNumber]; }

            if (_root_path == "" || _content == null)
            {
                string error = $"Document content not available.";
                Logger.Error(error);
                throw new Exception(error);
            }

            if (pageNumber >= _content.pageCount)
            {
                string error = $"unexpected page number {pageNumber} for pageCount {_content.pageCount}";
                Logger.Error(error);
                throw new Exception(error);
            }

            try
            {
                string binPath = GetPageBinaryFilePath(pageNumber);
                _pages.Add(pageNumber, new PageBinary(binPath));
                return _pages[pageNumber];
            }
            catch (Exception err)
            {
                string error = $"Parsing page content to Page object failed with err: {err.Message}";
                Logger.Error(error);
                throw new Exception(error);
            }
        }

        public void Dispose()
        {
            if (_root_path.Length > 0)
            {
                try
                {
                    Directory.Delete(_root_path, true);
                }
                catch (Exception err)
                {
                    Logger.Error($"failed to remove folder: {_root_path}. Error: {err.Message}");
                }

            }
        }

        public List<string> Pages
        {
            get { return _content.getPages(); }
        }

        public int PageCount
        {
            get { return _content.pageCount; }
        }

        protected string GetDocumentContentFilePath()
        {
            return Path.Combine(_root_path, $"{_id}.content");
        }

        protected string GetPageBinaryFilePath(int pageNumber)
        {
            string pageFilename = GetPageUUID(pageNumber);
            return Path.Combine(_root_path, _id, $"{pageFilename}.rm");
        }

        protected string GetPageMetadataFilePath(int pageNumber)
        {
            string pageFilename = GetPageUUID(pageNumber);
            return Path.Combine(_root_path, _id, $"{pageFilename}-metadata.json");
        }

        protected string GetDocumentFolderPath()
        {
            return Path.Combine(_root_path, _id);
        }

        protected string GetPageUUID(int pageNumber)
        {
            return _content.getPages().ElementAt(pageNumber);
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
                Logger.Error($"RmPageMetadata json deseralizing failed with: {err.Message}.\n Content:\n{metadataJsonString}");
            }
            return layerNames;
        }
    }
}
