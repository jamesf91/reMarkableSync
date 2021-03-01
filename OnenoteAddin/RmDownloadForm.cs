using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
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
            }

            public string ID { get; set; }

            public string VisibleName { get; set; }

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
        private delegate void _uiDelegate();

        [DllImport("USER32.DLL")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        public RmDownloadForm(Application application)
        {
            _rmCloudClient = new RmCloud();
            _application = application;
            InitializeComponent();
            InitializeData();
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

        private void btnOk_Click(object sender, EventArgs e)
        {
            if (rmTreeView.SelectedNode == null)
            {
                MessageBox.Show(this, "No document selected.");
                return;
            }
            string id = ((RmTreeNode)rmTreeView.SelectedNode).ID;
            Console.WriteLine($"Selected: {((RmTreeNode)rmTreeView.SelectedNode).VisibleName} | {id}");
            bool success = true;
            Console.WriteLine("Import " +  (success ? "successful" : "failed"));
            Close();
        }

        /*
        private async Task<bool> ImportDocument(string documentId)
        {
            return true;
        }
        */
    }
}
