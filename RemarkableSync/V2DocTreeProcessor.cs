using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace RemarkableSync
{
    public class V2DocTreeProcessor
    {
        private static string AppFolder = "remarkableSync";
        private static string TreeFile = "doctree.json";

        private static string BlobHost = "https://rm-blob-storage-prod.appspot.com";
        private static string DownloadUrl = BlobHost + "/api/v1/signed-urls/downloads";
        private static string HeaderGeneration = "x-goog-generation";
        private static string SchemaVersion = "3";
        private static char Delimiter = ':';

        private string _docTreeFilePath;
        private Dictionary<string, Doc> _docs;
        private long _generation;
        private string _rootHash;
        private HttpClient _client;
        private object _taskProgress;

        public V2DocTreeProcessor(HttpClient client)
        {
            _docTreeFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), AppFolder, TreeFile);
            _docs = new Dictionary<string, Doc>();
            _generation = 0;
            _rootHash = "";
            _client = client;
            _taskProgress = 0;

            Initialize();
        }

        public async Task SyncTreeAsync(CancellationToken cancellationToken, IProgress<string> progress)
        {
            // TODO: progress reporting
            Logger.LogMessage("Entering ... ");
            BlobStream rootHashBlob = await GetBlobStreamFromHashAsync("root");
            if (rootHashBlob == null)
            {
                Logger.LogMessage("Unable to get root blob for syncing");
                throw new Exception("Unable to get document tree from cloud.");
            }

            if (rootHashBlob.Blob == _rootHash)
            {
                Logger.LogMessage("Remote hash has not changed. Local cache is up to date");
                return;
            }

            cancellationToken.ThrowIfCancellationRequested();

            BlobStream rootBlob = await GetBlobStreamFromHashAsync(rootHashBlob.Blob);
            var latestDocFiles = ParseIndex(rootBlob.Blob);
            if (latestDocFiles == null)
            {
                Logger.LogMessage("Error parsing root index");
                throw new Exception("Unable to get document tree from cloud.");
            }

            List<Doc> docsToRead = ReconcileDocs(latestDocFiles);

            cancellationToken.ThrowIfCancellationRequested();
            progress.Report($"Retrieving document metadata... 0 out of {docsToRead.Count} complete.");
            lock(_taskProgress)
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

            //for (int i = 0; i < 3; ++i)
            //{
            //    await GetMetadataForDocAsync(docsToRead[i], cancellationToken, progress);
            //}

            // this needs to be done last to ensure data is valid before accepting new root hash
            _rootHash = rootHashBlob.Blob;
            _generation = rootHashBlob.Generation;
            WriteTree();
            Logger.LogMessage("Completed ");
        }

        public List<RmItem> GetAllItems()
        {
            var items = new List<RmItem>();

            lock(_docs)
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
                Logger.LogMessage($"Failed to write tree to disk. err: {err.ToString()} ");
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
                Logger.LogMessage($"Failed to read tree from disk. err: {err.ToString()} ");
                return null;
            }
        }

        private async Task<string> GetUrlAsync(string hash)
        {
            try
            {
                var requestContent = new BlobStorageRequest
                {
                    http_method = "GET",
                    relative_path = hash
                };
                HttpResponseMessage response = await HttpClientJsonExtensions.PostAsJsonAsync(_client, new Uri(DownloadUrl), requestContent);
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Request failed with status code {response.StatusCode}");
                }

                BlobStorageResponse blobResponse = await HttpContentJsonExtensions.ReadFromJsonAsync<BlobStorageResponse>(response.Content);
                return blobResponse.url;
            }
            catch (Exception err)
            {
                Logger.LogMessage($"Failed to get url for hash: {hash}. err: {err.ToString()} ");
                return "";
            }
        }

        private async Task<BlobStream> GetBlobStreamFromHashAsync(string hash)
        {
            Logger.LogMessage($"Entering: ..  hash = {hash}");
            try
            {
                string url = await GetUrlAsync(hash);
                if (url == "")
                {
                    throw new Exception($"Failed to determine GET url");
                }

                var request = new HttpRequestMessage
                {
                    RequestUri = new Uri(url),
                    Method = HttpMethod.Get
                };

                HttpResponseMessage response = await _client.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Request failed with status code {response.StatusCode}");
                }

                BlobStream blobStream = new BlobStream
                {
                    Generation = long.Parse(response.Headers.GetValues(HeaderGeneration).First()),
                    Blob = await response.Content.ReadAsStringAsync()
                };

                return blobStream;
            }
            catch (Exception err)
            {
                Logger.LogMessage($"Failed to complete GET for hash: {hash}. err: {err.ToString()} ");
                return null;
            }
        }

        private Dictionary<string, DocFile> ParseIndex(string rootIndex)
        {
            var lines = Array.FindAll(rootIndex.Split('\n'), line => line.Length > 0).ToList();
            if (lines[0] != SchemaVersion)
            {
                Logger.LogMessage($"Unexpected schema version: {lines[0]}");
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
                Logger.LogMessage($"Error while parsing doc entries: err: {err.Message}");
                return null;
            }
        }

        private DocFile ParseDocEntry(string line)
        {
            var fields = line.Split(Delimiter).ToList();
            if (fields.Count != 5)
            {
                Logger.LogMessage($"Expected 5 fields, got {fields.Count} fields for line {line}");
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

            Logger.LogMessage($"Checking existing docs against new docs, adding {docsAdded}, updating {docsModified}, removing {docsRemoved}");
            return documentsToRead;
        }

        private async Task<bool> GetMetadataForDocAsync(Doc doc, int totalCount, CancellationToken cancellationToken, IProgress<string> progress)
        {
            BlobStream docBlob = await GetBlobStreamFromHashAsync(doc.Hash);
            var docfiles = ParseIndex(docBlob.Blob);
            if (docfiles == null)
            {
                Logger.LogMessage($"Error parsing doc index for documentID {doc.DocumentID}, hash {doc.Hash}");
                // handle error
                return false ;
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
                Logger.LogMessage($"Unable to find .metadata for documentID {doc.DocumentID}, hash {doc.Hash}");
                // handle error
                return false;
            }

            cancellationToken.ThrowIfCancellationRequested();

            BlobStream metadataBlobStream = await GetBlobStreamFromHashAsync(docfiles[metadataDocId].Hash);
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
                Logger.LogMessage($"Error deseralizing metadata file. Err: {err.ToString()}");
                // handle error
                return false;
            }

            lock(_docs)
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

    public class MetadataFile
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

    class BlobStorageRequest
    {
        public string http_method { get; set; }
        public string relative_path { get; set; }
    }

    class BlobStorageResponse
    {
        public string relative_path { get; set; }
        public string url { get; set; }
        public string expires { get; set; }
        public string method { get; set; }
    }

    class BlobStream
    {
        public string Blob { get; set; }
        public long Generation { get; set; }
    }


}
