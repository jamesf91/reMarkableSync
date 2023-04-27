using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RemarkableSync.document;
using Renci.SshNet;
using Renci.SshNet.Common;


namespace RemarkableSync
{
    public partial class RmSftpDataSource : IRmDataSource
    {
        public static readonly string SshHostConfig = "SshHost";

        public static readonly string SshPasswordConfig = "SshPassword";

        private static readonly string ContentFolderPath = "/home/root/.local/share/remarkable/xochitl";

        private SftpClient _client;

        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public RmSftpDataSource(IConfigStore configStore)
        {
            string host = configStore.GetConfig(SshHostConfig);
            string password = configStore.GetConfig(SshPasswordConfig);

            if (password == null)
            {
                string errMsg = "Unable to get SSH password from config store";
                Logger.Error($"{errMsg}");
                throw new Exception(errMsg);
            }

            ConnectToRm(host, password);
        }

        public RmSftpDataSource(string host, string password)
        {
            ConnectToRm(host, password);
        }

        private void ConnectToRm(string host, string password)
        {
            const string username = "root";
            const int port = 22;

            try
            {
                _client = new SftpClient(host, port, username, password);
                _client.Connect();
            }
            catch (SshAuthenticationException err)
            {
                Logger.Error($"authentication error on SFTP connection: err: {err.Message}");
                throw new Exception("reMarkable device SSH login failed");
            }
            catch (Exception err)
            {
                Logger.Error($"error on SFTP connection: err: {err.Message}");
                throw new Exception("Failed to connect to reMarkable device via SSH");
            }
        }

        public async Task<List<RmItem>> GetItemHierarchy(CancellationToken cancellationToken, IProgress<string> progress)
        {
            List<RmItem> collection = await Task.Run(GetAllItems);
            return getChildItemsRecursive("", ref collection);
        }

        public async Task<RmDocument> DownloadDocument(string ID, CancellationToken cancellationToken, IProgress<string> progress)
        {
            return await Task.Run(() =>
            {
                // get the .content file for the notebook first
                string contentFileFullPath = $"{ContentFolderPath}/{ID}.content";
                string content = _client.ReadAllText(contentFileFullPath);
                RmSftpDownloadedDoc downloadedDoc = new RmSftpDownloadedDoc(ContentFolderPath, ID, _client, content);            

                return downloadedDoc;
            });
        }

        public void Dispose()
        {
            _client?.Dispose();
        }

        private List<RmItem> GetAllItems()
        {
            List<RmItem> items = new List<RmItem>();

            foreach (var file in _client.ListDirectory(ContentFolderPath))
            {
                if (file.IsDirectory)
                    continue;

                if (!file.Name.EndsWith(".metadata"))
                    continue;

                MemoryStream stream = new MemoryStream();
                _client.DownloadFile(file.FullName, stream);
                NotebookMetadata notebookMetadata = NotebookMetadata.FromStream(stream);

                if (notebookMetadata.deleted)
                    continue;

                items.Add(notebookMetadata.ToRmItem(file.Name));
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

