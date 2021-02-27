using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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
            }

            string ID { get; set; }

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

        public RmDownloadForm()
        {
            _rmCloudClient = new RmCloud();
            InitializeComponent();
            InitializeData();
        }

        private async void InitializeData()
        {
            rmTreeView.Nodes.Clear();
            await Task.Run(() =>
            {
                var rootItems = _rmCloudClient.GetItemHierarchy();
                var treeNodeList = RmTreeNode.FromRmItem(rootItems);
                rmTreeView.Nodes.AddRange(treeNodeList.ToArray());
            });
            return;
        }

    }
}
