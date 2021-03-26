using RemarkableSync.RmLine;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Application = Microsoft.Office.Interop.OneNote.Application;

namespace RemarkableSync.OnenoteAddin
{
    public partial class RmDownloadForm : Form
    {
        class RmTreeNode : TreeNode
        {
            public RmTreeNode(string id, string visibleName, bool isCollection)
            {
                Text = (isCollection ? "\xD83D\xDCC1" : "\xD83D\xDCC4") + " " + visibleName;
                ID = id;
                VisibleName = visibleName;
                IsCollection = isCollection;
            }

            public string ID { get; set; }

            public string VisibleName { get; set; }

            public bool IsCollection { get; set; }

            public static List<RmTreeNode> FromRmItem(List<RmItem> rmItems)
            {
                List<RmTreeNode> nodes = new List<RmTreeNode>();
                foreach (var rmItem in rmItems)
                {
                    RmTreeNode node = new RmTreeNode(rmItem.ID, rmItem.VissibleName, rmItem.Type == RmItem.CollectionType);
                    node.Nodes.AddRange(FromRmItem(rmItem.Children).ToArray());
                    nodes.Add(node);
                }

                return nodes;
            }
        }

        private IRmDataSource _rmDataSource;
        private Application _application;
        private IConfigStore _configStore;

        public RmDownloadForm(Application application, string settingsRegPath)
        {
            _configStore = new WinRegistryConfigStore(settingsRegPath);
            _application = application;

            InitializeComponent();
            InitializeData();            
        }

        private async void InitializeData()
        {
            rmTreeView.Nodes.Clear();
            lblInfo.Text = "Loading document list from reMarkable...";

            List<RmItem> rootItems = new List<RmItem>();

            try
            {
                await Task.Run(() =>
                {
                    int connMethod = -1;
                    try
                    {
                        string connMethodString = _configStore.GetConfig(SettingsForm.RmConnectionMethodConfig);
                        connMethod = Convert.ToInt32(connMethodString);
                    }
                    catch (Exception err)
                    {
                        Console.WriteLine($"RmDownloadForm::RmDownloadForm() - Failed to get RmConnectionMethod config with err: {err.Message}");
                        // will default to cloud
                    }

                    switch (connMethod)
                    {
                        case (int)SettingsForm.RmConnectionMethod.Ssh:
                            _rmDataSource = new RmSftpDataSource(_configStore);
                            Console.WriteLine("Using SFTP data source");
                            break;
                        case (int)SettingsForm.RmConnectionMethod.RmCloud:
                        default:
                            _rmDataSource = new RmCloudDataSource(_configStore);
                            Console.WriteLine("Using rm cloud data source");
                            break;
                    }
                });
                rootItems = await _rmDataSource.GetItemHierarchy();
            }
            catch (Exception err)
            {
                Console.WriteLine($"Error getting notebook structure from reMarkable. Err: {err.Message}");
                MessageBox.Show($"Error getting notebook structure from reMarkable.\n{err.Message}", "Error");
                Close();
                return;
            }

            Console.WriteLine("Got item hierarchy from remarkable cloud");
            var treeNodeList = RmTreeNode.FromRmItem(rootItems);

            rmTreeView.Nodes.AddRange(treeNodeList.ToArray());
            Console.WriteLine("Added nodes to tree view");
            lblInfo.Text = "Select document to load into OneNote.";
            return;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private async void btnOk_Click(object sender, EventArgs e)
        {
            if (rmTreeView.SelectedNode == null)
            {
                MessageBox.Show(this, "No document selected.");
                return;
            }

            RmTreeNode rmTreeNode = (RmTreeNode) rmTreeView.SelectedNode;
            Console.WriteLine($"Selected: {rmTreeNode.VisibleName} | {rmTreeNode.ID}");

            try
            {
                bool success = await ImportDocument(rmTreeNode);
                Console.WriteLine("Import " + (success ? "successful" : "failed"));
            }
            catch (Exception err)
            {
                Console.WriteLine($"Error importing document from reMarkable. Err: {err.Message}");
                MessageBox.Show($"Error importing document from reMarkable.\n{err.Message}", "Error");
                Close();
                return;
            }
        }


        private async Task<bool> ImportDocument(RmTreeNode rmTreeNode)
        {
            if (rmTreeNode.IsCollection)
            {
                MessageBox.Show(this, "Only document can be imported");
                return false;
            }

            RmItem item = new RmItem();
            item.Type = RmItem.DocumentType;
            item.ID = rmTreeNode.ID;
            item.VissibleName = rmTreeNode.VisibleName;

            List<RmPage> pages = new List<RmPage>();

            lblInfo.Text = $"Downloading {rmTreeNode.VisibleName}...";

            using (RmDownloadedDoc doc = await _rmDataSource.DownloadDocument(item))
            {
                Console.WriteLine("ImportDocument() - document downloaded");
                for (int i = 0; i < doc.PageCount; ++i)
                {
                    pages.Add(doc.GetPageContent(i));
                }
            }

            lblInfo.Text = $"Digitising {rmTreeNode.VisibleName}...";
            MyScriptClient hwrClient = new MyScriptClient(_configStore);
            Console.WriteLine("ImportDocument() - requesting hand writing recognition");
            MyScriptResult result = await hwrClient.RequestHwr(pages);

            if (result != null)
            {
                UpdateOneNoteWithHwrResult(rmTreeNode.VisibleName, result);
                lblInfo.Text = $"Imported {rmTreeNode.VisibleName} successfully.";
                Task.Run(() =>
                {
                    Thread.Sleep(500);
                }).Wait();
                Close();
            }
            else
            {
                lblInfo.Text = "Digitising failed";
            }
            return true;
        }

        private void UpdateOneNoteWithHwrResult(string name, MyScriptResult result)
        {
            OneNoteHelper oneNoteHelper = new OneNoteHelper(_application);
            string currentSectionId = oneNoteHelper.GetCurrentSectionId();
            string newPageId = oneNoteHelper.CreatePage(currentSectionId, name);
            oneNoteHelper.AddPageContent(newPageId, result.label);
        }

        private void RmDownloadForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _rmDataSource?.Dispose();
        }
    }
}
