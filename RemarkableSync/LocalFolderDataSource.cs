using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RemarkableSync.document;


namespace RemarkableSync
{
    public partial class LocalFolderDataSource : IRmDataSource
    {
        private string ContentFolderPath;

        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public LocalFolderDataSource(string root_path)
        {
            ContentFolderPath = root_path;
        }

        public async Task<List<RmItem>> GetItemHierarchy(CancellationToken cancellationToken, IProgress<string> progress)
        {
            List<RmItem> collection = GetAllItems();
            return getChildItemsRecursive("", ref collection);
        }

        public async Task<RmDocument> DownloadDocument(string ID, CancellationToken cancellationToken, IProgress<string> progress)
        {
            return await Task.Run(() =>
            {
                // get the .content file for the notebook first
                string contentFileFullPath = $"{ContentFolderPath}/{ID}.content";
                return new RmLocalDoc(ID, ContentFolderPath);
            });
        }

        public void Dispose()
        {
            
        }

        private List<RmItem> GetAllItems()
        {
            List<RmItem> items = new List<RmItem>();

            var files = Directory.GetFiles(ContentFolderPath, "*.metadata");
            foreach (var filename in files)
            {
                using(FileStream file = File.OpenRead(filename))
                {
                    RmSftpDataSource.NotebookMetadata notebookMetadata = RmSftpDataSource.NotebookMetadata.FromStream(file);
                    if (notebookMetadata.deleted)
                        continue;
                    items.Add(notebookMetadata.ToRmItem(Path.GetFileNameWithoutExtension(file.Name)));
                }                
            }

            return items;
        }

        private List<RmItem> getChildItemsRecursive(string parentId, ref List<RmItem> items)
        {
            var children = (from item in items where item.Parent == parentId select item).ToList();
            foreach (var child in children)
            {
                child.Children = getChildItemsRecursive(child.ID, ref items);
            }
            return children;
        }
    }
}

