using RemarkableSync.RmLine;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Application = Microsoft.Office.Interop.OneNote.Application;

namespace RemarkableSync.OnenoteAddin
{
    public partial class RmDownloadForm : Form
    {
        enum ImportMode
        {
            Text = 0,
            Graphics,
            Both,
            Unknown
        }

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
        private string _settingsRegPath;

        public RmDownloadForm(Application application, string settingsRegPath)
        {
            _settingsRegPath = settingsRegPath;
            _configStore = new WinRegistryConfigStore(_settingsRegPath);
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
                        Logger.LogMessage($"Failed to get RmConnectionMethod config with err: {err.Message}");
                        // will default to cloud
                    }

                    switch (connMethod)
                    {
                        case (int)SettingsForm.RmConnectionMethod.Ssh:
                            _rmDataSource = new RmSftpDataSource(_configStore);
                            Logger.LogMessage("Using SFTP data source");
                            break;
                        case (int)SettingsForm.RmConnectionMethod.RmCloud:
                        default:
                            _rmDataSource = new RmCloudDataSource(_configStore, new WinRegistryConfigStore(_settingsRegPath, false));
                            Logger.LogMessage("Using rm cloud data source");
                            break;
                    }
                });
                rootItems = await _rmDataSource.GetItemHierarchy();
            }
            catch (Exception err)
            {
                Logger.LogMessage($"Error getting notebook structure from reMarkable. Err: {err.Message}");
                MessageBox.Show($"Error getting notebook structure from reMarkable.\n{err.Message}", "Error");
                Close();
                return;
            }

            Logger.LogMessage("Got item hierarchy from remarkable cloud");
            var treeNodeList = RmTreeNode.FromRmItem(rootItems);

            rmTreeView.Nodes.AddRange(treeNodeList.ToArray());
            Logger.LogMessage("Added nodes to tree view");
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
            double zoom = (double)numericGraphicWidth.Value / 100.0;
            Logger.LogMessage($"Selected: {rmTreeNode.VisibleName} | {rmTreeNode.ID}");

            ImportMode mode = ImportMode.Unknown;
            if (radioBtnImportText.Checked)
            {
                mode = ImportMode.Text;
            }
            else if (radioBtnImportGraphics.Checked)
            {
                mode = ImportMode.Graphics;
            }
            else if (radioBtnImportBoth.Checked)
            {
                mode = ImportMode.Both;
            }

            try
            {
                bool success = await ImportDocument(rmTreeNode, mode, zoom);
                Logger.LogMessage("Import " + (success ? "successful" : "failed"));
            }
            catch (Exception err)
            {
                Logger.LogMessage($"Error importing document from reMarkable. Err: {err.Message}");
                MessageBox.Show($"Error importing document from reMarkable.\n{err.Message}", "Error");
                Close();
                return;
            }
        }


        private async Task<bool> ImportDocument(RmTreeNode rmTreeNode, ImportMode mode, double zoom = 0.5)
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
                Logger.LogMessage("document downloaded");
                for (int i = 0; i < doc.PageCount; ++i)
                {
                    pages.Add(doc.GetPageContent(i));
                }
            }

            switch (mode)
            {
                case ImportMode.Text:
                    return await ImportContentAsText(pages, rmTreeNode.VisibleName);
                case ImportMode.Graphics:
                    return ImportContentAsGraphics(pages, rmTreeNode.VisibleName, zoom);
                case ImportMode.Both:
                    return await ImportContentAsBoth(pages, rmTreeNode.VisibleName, zoom);
                default:
                    Logger.LogMessage($"ImportDocument() - unknown import mode: {mode}");
                    break;
            }
            return true;
        }

        private async Task<bool> ImportContentAsText(List<RmPage> pages, string visibleName)
        {
            lblInfo.Text = $"Digitising {visibleName}...";

            List<string> results = await GetHwrResultAsync(pages);
            if (results != null)
            {
                UpdateOneNoteWithHwrResult(visibleName, results);
                lblInfo.Text = $"Imported {visibleName} successfully.";
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

        private async Task<List<string>> GetHwrResultAsync(List<RmPage> pages)
        {
            Logger.LogMessage($"GetHwrResultAsync() - requesting hand writing recognition for {pages.Count} pages");
            MyScriptClient hwrClient = new MyScriptClient(_configStore);
            var hwrResults = (await Task.WhenAll(pages.Select((page, index) => hwrClient.RequestHwr(page, index)))).ToList();
            hwrResults.Sort((result1, result2) => result1.Item1.CompareTo(result2.Item1));
            return hwrResults.Select(result => result.Item2).ToList();
        }

        private void UpdateOneNoteWithHwrResult(string name, List<string> result)
        {
            OneNoteHelper oneNoteHelper = new OneNoteHelper(_application);
            string currentSectionId = oneNoteHelper.GetCurrentSectionId();
            string newPageId = oneNoteHelper.CreatePage(currentSectionId, name);
            foreach (string content in result)
            {
                oneNoteHelper.AddPageContent(newPageId, content);
            }
        }

        private bool ImportContentAsGraphics(List<RmPage> pages, string visibleName, double zoom)
        {
            lblInfo.Text = $"Importing {visibleName} as graphics...";
            OneNoteHelper oneNoteHelper = new OneNoteHelper(_application);
            string currentSectionId = oneNoteHelper.GetCurrentSectionId();
            string newPageId = oneNoteHelper.CreatePage(currentSectionId, visibleName);

            oneNoteHelper.AppendPageImages(newPageId, RmLinesDrawer.DrawPages(pages), zoom);

            lblInfo.Text = $"Imported {visibleName} successfully.";
            Task.Run(() =>
            {
                Thread.Sleep(500);
            }).Wait();
            Close();
            return true;
        }

        private async Task<bool> ImportContentAsBoth(List<RmPage> pages, string visibleName, double zoom)
        {
            lblInfo.Text = $"Importing {visibleName} as both text and graphics...";

            List<string> textResults = await GetHwrResultAsync(pages);
            List<Bitmap> graphicsResults = RmLinesDrawer.DrawPages(pages);
            if (textResults.Count != graphicsResults.Count)
            {
                Logger.LogMessage($"ImportContentAsBoth() - got {textResults.Count} text results and {graphicsResults.Count} graphics results");
                lblInfo.Text = $"Imported {visibleName} as both text and graphics encountered error.";
                return true;
            }

            List<Tuple<string, Bitmap>> result = new List<Tuple<string, Bitmap>>(textResults.Count);
            for(int i = 0; i < textResults.Count; ++i)
            {
                result.Add(Tuple.Create(textResults[i], graphicsResults[i]));
            }

            UpdateOneNoteWithHwrResultAndGraphics(visibleName, result, zoom);

            lblInfo.Text = $"Imported {visibleName} as both text and graphics successful.";
            Task.Run(() =>
            {
                Thread.Sleep(500);
            }).Wait();
            Close();
            return true;
        }

        private void UpdateOneNoteWithHwrResultAndGraphics(string name, List<Tuple<string, Bitmap>> result, double zoom)
        {
            OneNoteHelper oneNoteHelper = new OneNoteHelper(_application);
            string currentSectionId = oneNoteHelper.GetCurrentSectionId();
            string newPageId = oneNoteHelper.CreatePage(currentSectionId, name);
            foreach (var pageResult in result)
            {
                oneNoteHelper.AddPageContent(newPageId, pageResult.Item1);
                oneNoteHelper.AppendPageImage(newPageId, pageResult.Item2, zoom);
            }
        }

        private void RmDownloadForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _rmDataSource?.Dispose();
        }
    }
}
