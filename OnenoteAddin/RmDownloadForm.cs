using RemarkableSync.document;
using RemarkableSync.MyScript;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
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

        internal class LanguageChoice
        {
            public string Value
            {
                get;
                set;
            }

            public string Label
            {
                get;
                set;
            }

            public override string ToString()
            {
                return Label;
            }

            public override bool Equals(Object obj)
            {
                //Check for null and compare run-time types.
                if ((obj == null) || !this.GetType().Equals(obj.GetType()))
                {
                    return false;
                }
                else
                {
                    LanguageChoice p = (LanguageChoice)obj;
                    return p.Value == Value;
                }
            }

            public override int GetHashCode()
            {
                return Value.GetHashCode();
            }
        }

        public class RmTreeNode : TreeNode
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
        private CancellationTokenSource _cancellationSource;

        private static string _graphicWidthSettingName = "GraphicWidth";
        private static string _languageSettingName = "RecognitionLanguage";

        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public RmDownloadForm(Application application, string settingsRegPath)
        {
            _settingsRegPath = settingsRegPath;
            _configStore = new WinRegistryConfigStore(_settingsRegPath);
            _application = application;
            _cancellationSource = new CancellationTokenSource();

            InitializeComponent();
            InitializeData();
        }

        private async void InitializeData()
        {
            InitializeConfigs();
            rmTreeView.Nodes.Clear();
            lblInfo.Text = "Loading document list from reMarkable...";

            //todo: to early to enable
            //rmTreeView.AfterSelect += RmTreeView_AfterSelect;

            List<RmItem> rootItems = new List<RmItem>();

            try
            {
                await Task.Run(async () =>
                {
                    int connMethod = -1;
                    try
                    {
                        string connMethodString = _configStore.GetConfig(SettingsForm.RmConnectionMethodConfig);
                        connMethod = Convert.ToInt32(connMethodString);
                    }
                    catch (Exception err)
                    {
                        Logger.Error($"Failed to get RmConnectionMethod config with err: {err.Message}");
                        // will default to cloud
                    }

                    switch (connMethod)
                    {
                        case (int)SettingsForm.RmConnectionMethod.Ssh:
                            _rmDataSource = new RmSftpDataSource(_configStore);
                            Logger.Debug("Using SFTP data source");
                            break;
                        case (int)SettingsForm.RmConnectionMethod.RmCloud:
                        default:
                            _rmDataSource = new RmCloudDataSource(_configStore, new WinRegistryConfigStore(_settingsRegPath, false));
                            Logger.Debug("Using rm cloud data source");
                            break;
                    }

                    Progress<string> progress = new Progress<string>((string updateText) =>
                    {
                        lblInfo.Invoke((Action)(() => lblInfo.Text = $"Getting document list:\n{updateText}"));
                    });
                    rootItems = await _rmDataSource.GetItemHierarchy(_cancellationSource.Token, progress);
                });

                Logger.Debug("Got item hierarchy from remarkable cloud");
                var treeNodeList = RmTreeNode.FromRmItem(RmItem.SortItems(rootItems));

                rmTreeView.Nodes.AddRange(treeNodeList.ToArray());
                Logger.Debug("Added nodes to tree view");
                lblInfo.Text = "Select document to load into OneNote.";
            }
            catch (Exception err)
            {
                Logger.Error($"Error getting notebook structure from reMarkable. Err: {err.Message}");
                MessageBox.Show($"Error getting notebook structure from reMarkable.\n{err.Message}", "Error");
                Close();
                return;
            }
            return;
        }

        private async void RmTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            CancellationToken t = new CancellationToken();
            RmTreeNode rmTreeNode = (RmTreeNode)rmTreeView.SelectedNode;
            if (!rmTreeNode.IsCollection)
            {
                Progress<string> progress = new Progress<string>((string updateText) =>
                {
                    lblInfo.Invoke((Action)(() => lblInfo.Text = $"Downloading \"{rmTreeNode.VisibleName}\"...\n{updateText}"));
                });
                using (RmDocument doc = await _rmDataSource.DownloadDocument(rmTreeNode.ID, t, progress))
                {
                    PreviewForm previewForm = new PreviewForm(doc.GetPagesAsImage());
                    DialogResult result = previewForm.ShowDialog();
                    if (result == DialogResult.OK)
                    {
                        List<int> selectedBitmaps = previewForm.SelectedBitmaps;
                        // do something with the selected bitmaps
                        Logger.Debug("Selection received");
                    }
                }
            }
        }

        private void InitializeConfigs()
        {
            // graphics width
            double width;
            try
            {
                string widthString = _configStore.GetConfig(_graphicWidthSettingName);
                if (widthString == null)
                {
                    throw new FormatException();
                }
                width = Convert.ToDouble(widthString);
            }
            catch (Exception)
            {
                width = 50.0;
            }
            numericGraphicWidth.Value = (decimal)width;

            // recognition lanugage 
            List<LanguageChoice> languages = new List<LanguageChoice>()
            {
                new LanguageChoice() { Label = "Chinese", Value = "zh_CN"},
                new LanguageChoice() { Label = "Danish", Value = "da_DK"},
                new LanguageChoice() { Label = "Dutch", Value = "nl_BE"},
                new LanguageChoice() { Label = "English", Value = "en_US"},
                new LanguageChoice() { Label = "Finish", Value = "fi_FI"},
                new LanguageChoice() { Label = "French", Value = "fr_FR"},
                new LanguageChoice() { Label = "German", Value = "de_DE"},
                new LanguageChoice() { Label = "Italian", Value = "it_IT"},
                new LanguageChoice() { Label = "Japanese", Value = "ja_JP"},
                new LanguageChoice() { Label = "Norwegiean", Value = "no_NO"},
                new LanguageChoice() { Label = "Spanish", Value = "es_ES"},
                new LanguageChoice() { Label = "Swedish", Value = "sv_SE"},
            };

            string language = _configStore.GetConfig(_languageSettingName);
            if (language == null)
            {
                language = "en_US";
            }

            int selectedIndex = languages.IndexOf(new LanguageChoice() { Value = language });
            if (selectedIndex == -1)
            {
                languages.IndexOf(new LanguageChoice() { Value = "en_US" });
            }

            cboLanguage.DataSource = languages;
            cboLanguage.SelectedIndex = selectedIndex;

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

            RmTreeNode rmTreeNode = (RmTreeNode)rmTreeView.SelectedNode;
            double zoom = (double)numericGraphicWidth.Value;
            Logger.Debug($"Selected: {rmTreeNode.VisibleName} | {rmTreeNode.ID}");

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

            string language = "en_US";
            if (cboLanguage.SelectedItem != null)
            {
                language = ((LanguageChoice)(cboLanguage.SelectedItem)).Value;
            }

            try
            {
                bool success = await ImportSelection(rmTreeNode, mode, zoom, language);
                Logger.Debug("Import " + (success ? "successful" : "failed"));
                if (success)
                {
                    Close();
                }
            }
            catch (Exception err)
            {
                Logger.Error($"Error importing document from reMarkable. Err: {err.Message}");
                MessageBox.Show($"Error importing document from reMarkable.\n{err.Message}", "Error");
                Close();
                return;
            }
        }


        private async Task<bool> ImportSelection(RmTreeNode rmTreeNode, ImportMode mode, double zoom = 50.0, string language = "en_US")
        {
            Logger.Debug($"Saving settings: zoom: {zoom}, language: {language}");
            Dictionary<string, string> configs = new Dictionary<string, string>();
            configs[_graphicWidthSettingName] = zoom.ToString();
            configs[_languageSettingName] = language;
            _configStore.SetConfigs(configs);

            zoom = zoom / 100.0;

            if (rmTreeNode.IsCollection)
            {
                // build up list of all documents under this folder
                List<RmItem> items = new List<RmItem>();
                GetDocumentRecursive(rmTreeNode, ref items);

                DialogResult dialogResult = MessageBox.Show($"Import all {items.Count} documents under this folder?", "Import folder", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.No)
                {
                    return true;
                }

                // proceed with downloading all documents under folder
                foreach (var item in items)
                {
                    if (_cancellationSource.Token.IsCancellationRequested)
                    {
                        Logger.Debug($"Aborting import of multiple documents as cancellation is requested");
                        break;
                    }
                    if (!await ImportDocument(item, mode, zoom, language))
                    {
                        lblInfo.Text = $"Downloading \"{item.VissibleName}\"...  Failed.\n Import stopped";
                    }
                }

                return true;
            }
            else
            {
                RmItem item = new RmItem()
                {
                    Type = RmItem.DocumentType,
                    ID = rmTreeNode.ID,
                    VissibleName = rmTreeNode.VisibleName
                };

                return await ImportDocument(item, mode, zoom, language);
            }
        }

        private void GetDocumentRecursive(RmTreeNode currNode, ref List<RmItem> items)
        {
            foreach (RmTreeNode childNode in currNode.Nodes)
            {
                if (childNode.IsCollection)
                {
                    GetDocumentRecursive(childNode, ref items);
                }
                else
                {
                    items.Add(new RmItem()
                    {
                        Type = RmItem.DocumentType,
                        ID = childNode.ID,
                        VissibleName = childNode.VisibleName
                    });
                }
            }
        }

        private async Task<bool> ImportDocument(RmItem item, ImportMode mode, double zoom, string language)
        {
            Logger.Debug($"importing {item.VissibleName}, id: {item.ID}");
            List<PageBinary> pages = new List<PageBinary>();

            lblInfo.Text = $"Downloading \"{item.VissibleName}\"...";

            Progress<string> progress = new Progress<string>((string updateText) =>
            {
                lblInfo.Invoke((Action)(() => lblInfo.Text = $"Downloading \"{item.VissibleName}\"...\n{updateText}"));
            });

            using (RmDocument doc = await _rmDataSource.DownloadDocument(item.ID, _cancellationSource.Token, progress))
            {
                Logger.Debug("document downloaded");
                switch (mode)
                {
                    case ImportMode.Text:
                        return await ImportContentAsText(doc, item.VissibleName, language);
                    case ImportMode.Graphics:
                        return ImportContentAsGraphics(doc, item.VissibleName, zoom);
                    case ImportMode.Both:
                        return await ImportContentAsBoth(doc, item.VissibleName, zoom, language);
                    default:
                        Logger.Error($"ImportDocument() - unknown import mode: {mode}");
                        break;
                }
            }


            return true;
        }

        private async Task<bool> ImportContentAsText(RmDocument doc, string visibleName, string language)
        {
            lblInfo.Text = $"Digitising {visibleName}...";

            List<string> results = await GetHwrResultAsync(doc, language);
            if (results != null)
            {
                UpdateOneNoteWithHwrResult(visibleName, results);
                lblInfo.Text = $"Imported {visibleName} successfully.";
                Task.Run(() =>
                {
                    Thread.Sleep(500);
                }).Wait();
            }
            else
            {
                lblInfo.Text = "Digitising failed";
                return false;
            }
            return true;
        }

        private async Task<List<string>> GetHwrResultAsync(RmDocument doc, string language)
        {
            Logger.Debug($"GetHwrResultAsync() - requesting hand writing recognition for {doc.PageCount} pages");
            MyScriptClient hwrClient = new MyScriptClient(_configStore);

            var hwrTasks = new List<Task<Tuple<int, string>>>();
            for (var i = 0; i < doc.PageCount; i++)
            {
                hwrTasks.Add(hwrClient.RequestHwr(doc, i, language));
            }

            var al = await Task.WhenAll(hwrTasks);
            var hwrResults = al.ToList();


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

        private bool ImportContentAsGraphics(RmDocument doc, string visibleName, double zoom)
        {
            lblInfo.Text = $"Importing {visibleName} as graphics...";
            OneNoteHelper oneNoteHelper = new OneNoteHelper(_application);
            string currentSectionId = oneNoteHelper.GetCurrentSectionId();
            string newPageId = oneNoteHelper.CreatePage(currentSectionId, visibleName);

            oneNoteHelper.AppendPageImages(newPageId, doc.GetPagesAsImage(), zoom);

            lblInfo.Text = $"Imported {visibleName} successfully.";
            Task.Run(() =>
            {
                Thread.Sleep(500);
            }).Wait();
            return true;
        }

        private async Task<bool> ImportContentAsBoth(RmDocument doc, string visibleName, double zoom, string language)
        {
            lblInfo.Text = $"Importing {visibleName} as both text and graphics...";

            List<string> textResults = await GetHwrResultAsync(doc, language);

            if (textResults.Count != doc.PageCount)
            {
                Logger.Debug($"ImportContentAsBoth() - got {textResults.Count} text results and {doc.PageCount} graphics results");
                lblInfo.Text = $"Imported {visibleName} as both text and graphics encountered error.";
                return false;
            }

            List<Tuple<string, Bitmap>> result = new List<Tuple<string, Bitmap>>(textResults.Count);
            List<Bitmap> pageImages = doc.GetPagesAsImage();
            for (int i = 0; i < textResults.Count; ++i)
            {
                result.Add(Tuple.Create(textResults[i], pageImages[i]));
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
            _cancellationSource.Cancel();
            _rmDataSource?.Dispose();
        }
    }
}
