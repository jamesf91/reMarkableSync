using RemarkableSync.document;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace RemarkableSync
{
    class V2DocTreeProcessor
    {
        private static string AppFolder = "remarkableSync";
        private static string TreeFile = "doctree.json";

        private static string SchemaVersion = "3";
        private static char Delimiter = ':';

        private string _docTreeFilePath;
        private Dictionary<string, Doc> _docs;
        private long _generation;
        private string _rootHash;
        private object _taskProgress;
        private V2HttpHelper _httpHelper;

        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public V2DocTreeProcessor(HttpClient client)
        {
            _docTreeFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), AppFolder, TreeFile);
            _docs = new Dictionary<string, Doc>();
            _generation = 0;
            _rootHash = "";
            _taskProgress = 0;
            _httpHelper = new V2HttpHelper(client);

            Initialize();
        }

        public async Task SyncTreeAsync(CancellationToken cancellationToken, IProgress<string> progress)
        {
            // TODO: progress reporting
            Logger.Debug("Entering ... ");
            BlobStream rootHashBlob = await _httpHelper.GetBlobStreamFromHashAsync("root");
            if (rootHashBlob == null)
            {
                Logger.Error("Unable to get root blob for syncing");
                throw new Exception("Unable to get document tree from cloud.");
            }

            if (rootHashBlob.Blob == _rootHash)
            {
                Logger.Error("Remote hash has not changed. Local cache is up to date");
                return;
            }

            cancellationToken.ThrowIfCancellationRequested();

            BlobStream rootBlob = await _httpHelper.GetBlobStreamFromHashAsync(rootHashBlob.Blob);
            var latestDocFiles = ParseIndex(rootBlob.Blob);
            if (latestDocFiles == null)
            {
                Logger.Error("Error parsing root index");
                throw new Exception("Unable to get document tree from cloud.");
            }

            List<Doc> docsToRead = ReconcileDocs(latestDocFiles);

            cancellationToken.ThrowIfCancellationRequested();
            progress.Report($"Retrieving document metadata... 0 out of {docsToRead.Count} complete.");
            lock (_taskProgress)
            {
                _taskProgress = 0;
            }
            ParallelOptions po = new ParallelOptions
            {
                CancellationToken = cancellationToken
            };
            Parallel.ForEach(docsToRead, po,
                doc =>
                {
                    var result = GetMetadataForDocAsync(doc, docsToRead.Count, cancellationToken, progress).Result;
                });

            // this needs to be done last to ensure data is valid before accepting new root hash
            _rootHash = rootHashBlob.Blob;
            _generation = rootHashBlob.Generation;
            WriteTree();
            Logger.Debug("Completed ");
        }

        public List<RmItem> GetAllItems()
        {
            var items = new List<RmItem>();

            lock (_docs)
            {
                foreach (var entry in _docs)
                {
                    var doc = entry.Value;
                    items.Add(new RmItem
                    {
                        ID = doc.DocumentID,
                        Version = doc.version,
                        Type = doc.type,
                        VissibleName = doc.visibleName,
                        Parent = doc.parent
                    });
                }
            }

            return items;
        }

        public List<DocFile> GetFileListForDocument(string docId)
        {
            List<DocFile> fileList = new List<DocFile>();
            lock (_docs)
            {
                if (!_docs.ContainsKey(docId))
                {
                    Logger.Debug($"Unknown docId: {docId} ");
                    return fileList;
                }

                fileList = _docs[docId].Files.ToList();
            }
            return fileList;
        }

        private void Initialize()
        {
            var tree = ReadTree();
            if (tree != null)
            {
                _generation = tree.Generation;
                _rootHash = tree.Hash;
                lock (_docs)
                {
                    _docs = tree.Docs.ToDictionary(Doc => Doc.DocumentID);
                }
            }
        }

        private bool WriteTree()
        {
            try
            {
                DocTree tree = null;
                lock (_docs)
                {
                    tree = new DocTree
                    {
                        Hash = _rootHash,
                        Generation = _generation,
                        Docs = _docs.Values.ToArray()
                    };
                }
                JsonSerializerOptions options = new JsonSerializerOptions { WriteIndented = true };
                string treeFileContent = JsonSerializer.Serialize(tree, options);

                Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), AppFolder));
                File.WriteAllText(_docTreeFilePath, treeFileContent);
                return true;
            }
            catch (Exception err)
            {
                Logger.Error($"Failed to write tree to disk. err: {err.ToString()} ");
                return false;
            }
        }

        private DocTree ReadTree()
        {
            try
            {
                string treeFileContent = File.ReadAllText(_docTreeFilePath);
                return JsonSerializer.Deserialize<DocTree>(treeFileContent);
            }
            catch (Exception err)
            {
                Logger.Error($"Failed to read tree from disk. err: {err.ToString()} ");
                return null;
            }
        }

        private Dictionary<string, DocFile> ParseIndex(string rootIndex)
        {
            var lines = Array.FindAll(rootIndex.Split('\n'), line => line.Length > 0).ToList();
            if (lines[0] != SchemaVersion)
            {
                Logger.Debug($"Unexpected schema version: {lines[0]}");
                return null;
            }
            lines.RemoveAt(0);
            try
            {
                Dictionary<string, DocFile> docs = (from line in lines
                                                    select ParseDocEntry(line)).ToDictionary(doc => doc.DocumentID);
                return docs;
            }
            catch (Exception err)
            {
                Logger.Error($"Error while parsing doc entries: err: {err.Message}");
                return null;
            }
        }

        private DocFile ParseDocEntry(string line)
        {
            var fields = line.Split(Delimiter).ToList();
            if (fields.Count != 5)
            {
                Logger.Error($"Expected 5 fields, got {fields.Count} fields for line {line}");
                throw new Exception("Incorrect number of fields in doc entry line");
            }

            return new DocFile
            {
                Hash = fields[0],
                Type = fields[1],
                DocumentID = fields[2],
                Subfiles = int.Parse(fields[3]),
                Size = int.Parse(fields[4])
            };
        }

        private List<Doc> ReconcileDocs(Dictionary<string, DocFile> newDocfiles)
        {
            List<Doc> documentsToRead = new List<Doc>();
            int docsAdded = 0;
            int docsModified = 0;
            int docsRemoved = 0;

            lock (_docs)
            {
                var oldDocs = new Dictionary<string, Doc>(_docs);

                foreach (string newDocId in newDocfiles.Keys)
                {
                    if (!oldDocs.ContainsKey(newDocId))
                    {
                        // new document
                        Doc newDoc = new Doc
                        {
                            Hash = newDocfiles[newDocId].Hash,
                            Type = newDocfiles[newDocId].Type,
                            DocumentID = newDocfiles[newDocId].DocumentID,
                            Subfiles = newDocfiles[newDocId].Subfiles,
                            Size = newDocfiles[newDocId].Size
                        };
                        documentsToRead.Add(newDoc);
                        _docs.Add(newDocId, newDoc);
                        docsAdded++;
                        continue;
                    }

                    if (oldDocs[newDocId].Hash != newDocfiles[newDocId].Hash)
                    {
                        // modified document
                        Doc modifiedDoc = new Doc
                        {
                            Hash = newDocfiles[newDocId].Hash,
                            Type = newDocfiles[newDocId].Type,
                            DocumentID = newDocfiles[newDocId].DocumentID,
                            Subfiles = newDocfiles[newDocId].Subfiles,
                            Size = newDocfiles[newDocId].Size
                        };
                        documentsToRead.Add(modifiedDoc);
                        docsModified++;
                    }

                    oldDocs.Remove(newDocId);
                }

                // any remaining unmatched docs are to be removed.
                docsRemoved = oldDocs.Count;
                foreach (string oldDocId in oldDocs.Keys)
                {
                    _docs.Remove(oldDocId);
                }
            }

            Logger.Debug($"Checking existing docs against new docs, adding {docsAdded}, updating {docsModified}, removing {docsRemoved}");
            return documentsToRead;
        }

        private async Task<bool> GetMetadataForDocAsync(Doc doc, int totalCount, CancellationToken cancellationToken, IProgress<string> progress)
        {
            BlobStream docBlob = await _httpHelper.GetBlobStreamFromHashAsync(doc.Hash);
            var docfiles = ParseIndex(docBlob.Blob);
            if (docfiles == null)
            {
                Logger.Error($"Error parsing doc index for documentID {doc.DocumentID}, hash {doc.Hash}");
                // handle error
                return false;
            }

            doc.Files = docfiles.Values.OrderBy(docfile => docfile.DocumentID).ToArray();

            string metadataDocId = "";
            foreach (string docId in docfiles.Keys)
            {
                if (docId.EndsWith("metadata"))
                {
                    metadataDocId = docId;
                    break;
                }
            }

            if (metadataDocId.Length == 0)
            {
                Logger.Error($"Unable to find .metadata for documentID {doc.DocumentID}, hash {doc.Hash}");
                // handle error
                return false;
            }

            cancellationToken.ThrowIfCancellationRequested();

            BlobStream metadataBlobStream = await _httpHelper.GetBlobStreamFromHashAsync(docfiles[metadataDocId].Hash);
            try
            {
                MetadataFile metadata = JsonSerializer.Deserialize<MetadataFile>(metadataBlobStream.Blob);
                doc.visibleName = metadata.visibleName;
                doc.type = metadata.type;
                doc.parent = metadata.parent;
                doc.lastModified = metadata.lastModified;
                doc.lastOpened = metadata.lastOpened;
                doc.lastOpenedPage = metadata.lastOpenedPage;
                doc.version = metadata.version;
                doc.pinned = metadata.pinned;
                doc.synced = metadata.synced;
                doc.modified = metadata.modified;
                doc.deleted = metadata.deleted;
                doc.metadatamodified = metadata.metadatamodified;
            }
            catch (Exception err)
            {
                Logger.Error($"Error deseralizing metadata file. Err: {err.ToString()}");
                // handle error
                return false;
            }

            lock (_docs)
            {
                _docs[doc.DocumentID] = doc;
            }

            int taskProgress = 0;
            lock (_taskProgress)
            {
                taskProgress = (int)_taskProgress + 1;
                _taskProgress = taskProgress;
            }
            progress.Report($"Retrieving document metadata... {taskProgress} out of {totalCount} complete.");
            return true;
        }
    }

    class DocTree
    {
        public string Hash { get; set; }
        public long Generation { get; set; }
        public Doc[] Docs { get; set; }
    }

    class Doc
    {
        public DocFile[] Files { get; set; }
        public string Hash { get; set; }
        public string Type { get; set; }
        public string DocumentID { get; set; }
        public int Subfiles { get; set; }
        public int Size { get; set; }
        public string visibleName { get; set; }
        public string type { get; set; }
        public string parent { get; set; }
        public string lastModified { get; set; }
        public string lastOpened { get; set; }
        public int lastOpenedPage { get; set; }
        public int version { get; set; }
        public bool pinned { get; set; }
        public bool synced { get; set; }
        public bool modified { get; set; }
        public bool deleted { get; set; }
        public bool metadatamodified { get; set; }
    }

    class DocFile
    {
        public string Hash { get; set; }
        public string Type { get; set; }
        public string DocumentID { get; set; }
        public int Subfiles { get; set; }
        public int Size { get; set; }
    }

    class MetadataFile
    {
        public string visibleName { get; set; }
        public string type { get; set; }
        public string parent { get; set; }
        public string lastModified { get; set; }
        public string lastOpened { get; set; }
        public int lastOpenedPage { get; set; }
        public int version { get; set; }
        public bool pinned { get; set; }
        public bool synced { get; set; }
        public bool modified { get; set; }
        public bool deleted { get; set; }
        public bool metadatamodified { get; set; }
    }



}
