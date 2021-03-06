using RemarkableSync.RmLine;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Application = Microsoft.Office.Interop.OneNote.Application;

namespace RemarkableSync
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

        private RmCloud _rmCloudClient;
        private Application _application;

        [DllImport("USER32.DLL")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("USER32.DLL")]
        public static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("USER32.DLL")]
        public static extern bool IsIconic(IntPtr hWnd);

        [DllImport("USER32.DLL")]
        public static extern bool ShowWindow(IntPtr hWnd, int cmd);

        public static void ForceShowWindow(IntPtr hWnd)
        {
            const int SW_RESTORE = 0x9;

            if (hWnd == IntPtr.Zero)
            {
                return;
            }
            if (!IsWindowVisible(hWnd) || IsIconic(hWnd))
            {
                ShowWindow(hWnd, SW_RESTORE);
            }
            SetForegroundWindow(hWnd);
        }

        public RmDownloadForm(Application application)
        {
            _rmCloudClient = new RmCloud();
            _application = application;
            InitializeComponent();
            InitializeData();
            //ForceShowWindow(Handle);
            this.Load += RmDownloadForm_Load;
        }

        public void BringToFrontAndActivate()
        {
            Console.WriteLine("BringToFrontAndActivate");
            SetForegroundWindow(Handle);
        }

        private void RmDownloadForm_Load(object sender, EventArgs e)
        {
            SetForegroundWindow(Handle);
        }

        private async void InitializeData()
        {
            rmTreeView.Nodes.Clear();
            lblInfo.Text = "Loading document list from reMarkable...";

            var rootItems = await _rmCloudClient.GetItemHierarchy();
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

            bool success = await ImportDocument(rmTreeNode);
            Console.WriteLine("Import " +  (success ? "successful" : "failed"));
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

            using (RmDownloadedDoc doc = await _rmCloudClient.DownloadDocument(item))
            {
                Console.WriteLine("ImportDocument() - document downloaded");
                for (int i = 0; i < doc.PageCount; ++i)
                {
                    pages.Add(doc.GetPageContent(i));
                }
            }

            lblInfo.Text = $"Digitizing {rmTreeNode.VisibleName}...";
            MyScriptClient hwrClient = new MyScriptClient();
            Console.WriteLine("ImportDocument() - requesting hand writing recognition");
            MyScriptResult result = await hwrClient.RequestHwr(pages);
            if (result != null)
            {
                lblInfo.Text = $"Import result:\n {result.label}";
            }
            else
            {
                lblInfo.Text = "Digitizing failed";
            }
            return true;
        }

    }
}
